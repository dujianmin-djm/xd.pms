using Medallion.Threading;
using Medallion.Threading.FileSystem;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RequestLocalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Caching;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Identity.AspNetCore;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Security.Claims;
using Volo.Abp.Swashbuckle;
using Volo.Abp.Timing;
using Volo.Abp.VirtualFileSystem;
using XD.Pms.ApiKeys;
using XD.Pms.Authentication;
using XD.Pms.Authentication.ApiKey;
using XD.Pms.EntityFrameworkCore;
using XD.Pms.Filters;
using XD.Pms.HealthChecks;
using XD.Pms.Middlewares;
using XD.Pms.Permissions;

namespace XD.Pms;

[DependsOn(
    typeof(PmsHttpApiModule),
	typeof(PmsApplicationModule),
	typeof(PmsEntityFrameworkCoreModule),
	typeof(AbpAutofacModule),
	typeof(AbpDistributedLockingModule),
	//typeof(AbpCachingStackExchangeRedisModule),
	typeof(AbpIdentityAspNetCoreModule),
	typeof(AbpOpenIddictAspNetCoreModule),
	//typeof(AbpAccountWebOpenIddictModule),
	typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule)
)]
public class PmsHttpApiHostModule : AbpModule
{
	public override void PreConfigureServices(ServiceConfigurationContext context)
	{
		var hostingEnvironment = context.Services.GetHostingEnvironment();
		var configuration = context.Services.GetConfiguration();

		PreConfigure<OpenIddictBuilder>(builder =>
		{
			builder.AddValidation(options =>
			{
				/* 微服务架构可以配置单独指向 AuthServer 的授权服务器（API 网关 + 单独认证服务）
				 * 须同时启用 app.UseAbpOpenIddictValidation(); 用于远程令牌验证
				 * options.SetIssuer(configuration["AuthServer:Authority"]!);
				 */
				options.AddAudiences("Pms");
				options.UseLocalServer();
				options.UseAspNetCore();

				// 添加验证事件处理器
				options.AddEventHandler(TokenValidationHandler.Descriptor);
				//options.AddEventHandler(TokenBlacklistValidationHandler.Descriptor);
			});
		});

		var tokenSettings = configuration.GetSection(TokenSettings.SectionName).Get<TokenSettings>();
		int accessTokenExpirationMinutes = tokenSettings?.AccessTokenExpirationMinutes ?? 30;
		int refreshTokenExpirationDays = tokenSettings?.RefreshTokenExpirationDays ?? 7;
		PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
		{
			serverBuilder.SetAccessTokenLifetime(TimeSpan.FromMinutes(accessTokenExpirationMinutes));
			serverBuilder.SetRefreshTokenLifetime(TimeSpan.FromDays(refreshTokenExpirationDays));
			if (!hostingEnvironment.IsDevelopment())
			{
				serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx", configuration["AuthServer:CertificatePassPhrase"]!);
				serverBuilder.SetIssuer(new Uri(configuration["AuthServer:Authority"]!));
			}
		});
	}
	
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

		ConfigureCache();
		ConfigureAutoApiControllers();
        ConfigureAuthentication(context.Services);
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureDataProtection(context.Services, hostingEnvironment, configuration);
        ConfigureDistributedLocking(context.Services);
        ConfigureCors(context.Services, configuration);
        ConfigureSwaggerServices(context.Services, configuration);

		if (hostingEnvironment.IsDevelopment())
		{
			IdentityModelEventSource.ShowPII = true;
			IdentityModelEventSource.LogCompleteSecurityArtifact = true;
		}

		if (!configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata"))
		{
			Configure<OpenIddictServerAspNetCoreOptions>(options =>
			{
				options.DisableTransportSecurityRequirement = true;
			});

			Configure<ForwardedHeadersOptions>(options =>
			{
				options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
			});
		}
		
		Configure<PermissionManagementOptions>(options =>
		{
			options.IsDynamicPermissionStoreEnabled = true;

			// "R" = RolePermissionValueProvider.ProviderName
			options.ProviderPolicies["R"] = PmsPermissions.System.Roles.ManagePermissions;
			// "U" = UserPermissionValueProvider.ProviderName  
			options.ProviderPolicies["U"] = PmsPermissions.System.Users.ManageRoles;
		});
		
		Configure<MvcOptions>(options =>
		{
			options.Filters.Add<ApiResultFilter>();
		});

		Configure<AbpAntiForgeryOptions>(options =>
		{
			options.AutoValidate = false;
			options.TokenCookie.Name = "PMS.XSRF-TOKEN";
			options.TokenCookie.SameSite = SameSiteMode.Lax;
			options.TokenCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
		});

		Configure<AbpClockOptions>(options =>
		{
			options.Kind = DateTimeKind.Local;
		});

		context.Services.Configure<TokenSettings>(configuration.GetSection(TokenSettings.SectionName));
		context.Services.AddHttpClient();
		context.Services.AddPmsHealthChecks();
		ConfigureRequestLocalization();
	}

	private void ConfigureRequestLocalization()
	{
		Configure<AbpRequestLocalizationOptions>(options =>
		{
			options.RequestLocalizationOptionConfigurators.Add(async (serviceProvider, requestLocalizationOptions) =>
			{
				requestLocalizationOptions.DefaultRequestCulture = new RequestCulture("zh-Hans");

				// 回退策略：当请求的语言不被支持时使用默认语言
				requestLocalizationOptions.FallBackToParentCultures = true;
				requestLocalizationOptions.FallBackToParentUICultures = true;

				var providers = requestLocalizationOptions.RequestCultureProviders;

				// 保存现有的 CookieRequestCultureProvider（ABP Web UI 需要）
				var cookieProvider = providers.OfType<CookieRequestCultureProvider>().FirstOrDefault();

				// 保存现有的 QueryStringRequestCultureProvider
				var queryStringProvider = providers.OfType<QueryStringRequestCultureProvider>().FirstOrDefault();

				// 清除所有 Provider
				providers.Clear();

				// 按优先级添加 Provider
				// 1. QueryString（支持 ?lang=xxx 和 ABP 默认的 ?culture=xxx）
				providers.Add(queryStringProvider ?? new QueryStringRequestCultureProvider
				{
					QueryStringKey = "culture",
					UIQueryStringKey = "ui-culture"
				});
				providers.Add(new QueryStringRequestCultureProvider
				{
					QueryStringKey = "lang",
					UIQueryStringKey = "lang"
				});

				// 2. Cookie （保留 ABP Web UI 功能）
				providers.Add(cookieProvider ?? new CookieRequestCultureProvider());

				// 3. Accept-Language 请求头
				providers.Add(new AcceptLanguageHeaderRequestCultureProvider());

				// 如果所有 Provider 都没有匹配，将使用 DefaultRequestCulture
			});
		});
	}

	private void ConfigureCache()
    {
        Configure<AbpDistributedCacheOptions>(options => 
		{ 
			options.KeyPrefix = "Pms:"; 
		});
		// 添加内存缓存作为分布式缓存的实现
		// 如果没有配置 Redis 模块，ABP 默认会使用内存缓存 typeof(AbpCachingModule)
	}

	private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<PmsDomainSharedModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}XD.Pms.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<PmsDomainModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}XD.Pms.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<PmsApplicationContractsModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}XD.Pms.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<PmsApplicationModule>(
                    Path.Combine(hostingEnvironment.ContentRootPath,
                        $"..{Path.DirectorySeparatorChar}XD.Pms.Application"));
				options.FileSets.ReplaceEmbeddedByPhysical<PmsHttpApiModule>(
					Path.Combine(hostingEnvironment.ContentRootPath,
						$"..{Path.DirectorySeparatorChar}XD.Pms.HttpApi"));
				options.FileSets.ReplaceEmbeddedByPhysical<PmsHttpApiModule>(
					Path.Combine(hostingEnvironment.ContentRootPath, 
						string.Format("..{0}..{0}src{0}XD.Pms.HttpApi", Path.DirectorySeparatorChar)));
				options.FileSets.ReplaceEmbeddedByPhysical<PmsHttpApiHostModule>(hostingEnvironment.ContentRootPath);
			});
        }
    }

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(PmsApplicationModule).Assembly);
        });
    }

    private static void ConfigureAuthentication(IServiceCollection services)
	{
		services.AddAuthentication(options =>
		{
			options.DefaultScheme = "Pms.Smart";
			options.DefaultAuthenticateScheme = null;
			options.DefaultChallengeScheme = null;
		})
		.AddApiKey(options =>
		{
			options.Enabled = true;
			options.Realm = "Pms API";
			options.HeaderName = ApiKeyConsts.DefaultHeaderName;
		})
		.AddPolicyScheme("Pms.Smart", "Smart Auth Selector", options =>
		{
			options.ForwardDefaultSelector = context =>
			{
				if (context.Request.Headers.ContainsKey(ApiKeyConsts.DefaultHeaderName))
				{
					return ApiKeyAuthenticationOptions.DefaultScheme;
				}

				var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
				if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
				{
					return OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
				}

				return IdentityConstants.ApplicationScheme;
			};
		});

		services.ConfigureApplicationCookie(options =>
		{
			options.LoginPath = "/connect/login";
			options.LogoutPath = "/connect/logout";
			options.Cookie.Name = "Pms.Identity";
			options.Cookie.HttpOnly = true;
			options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
			options.ExpireTimeSpan = TimeSpan.FromDays(7);
			options.SlidingExpiration = true;

			options.Events.OnRedirectToLogin = context =>
			{
				if (IsApiRequest(context.Request))
				{
					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					return Task.CompletedTask;
				}
				context.Response.Redirect(context.RedirectUri);
				return Task.CompletedTask;
			};

			options.Events.OnRedirectToAccessDenied = context =>
			{
				if (IsApiRequest(context.Request))
				{
					context.Response.StatusCode = StatusCodes.Status403Forbidden;
					return Task.CompletedTask;
				}
				context.Response.Redirect(context.RedirectUri);
				return Task.CompletedTask;
			};
		});

		services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
		{
			options.IsDynamicClaimsEnabled = true;
		});
	}

	private static bool IsApiRequest(HttpRequest request)
	{
		return request.Path.StartsWithSegments("/papi") || request.Path.StartsWithSegments("/api");
	}

	private static void ConfigureSwaggerServices(IServiceCollection services, IConfiguration configuration)
    {
		var authority = configuration["AuthServer:Authority"]!.TrimEnd('/');
		services.AddAbpSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo { Title = "Pms API", Version = "v1", Description = "" });
			options.DocInclusionPredicate((docName, description) => true);
			options.CustomSchemaIds(type => type.FullName);

			options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
			{
				Type = SecuritySchemeType.OAuth2,
				Description = "OAuth2 认证",
				Flows = new OpenApiOAuthFlows
				{
					AuthorizationCode = new OpenApiOAuthFlow
					{
						AuthorizationUrl = new Uri($"{authority}/connect/authorize"),
						TokenUrl = new Uri($"{authority}/connect/token"),
						Scopes = new Dictionary<string, string>
						{
							["Pms"] = "Pms API"
						}
					},
					Password = new OpenApiOAuthFlow
					{
						TokenUrl = new Uri($"{authority}/connect/token"),
						Scopes = new Dictionary<string, string>
						{
							["Pms"] = "Pms API",
							["openid"] = "OpenID",
							["profile"] = "用户资料",
							["email"] = "邮箱",
							["roles"] = "角色",
							["offline_access"] = "离线访问（刷新令牌）"
						}
					}
				}
			});

			options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
			{
				Type = SecuritySchemeType.Http,
				Scheme = "bearer",
				BearerFormat = "JWT",
				Description = "直接输入 JWT Token 认证（不需要 Bearer 前缀）"
			});

			options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
			{
				Type = SecuritySchemeType.ApiKey,
				In = ParameterLocation.Header,
				Name = "X-API-Sign",
				Description = "API Key 认证 - 在请求头中添加 X-API-Sign"
			});

			options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecuritySchemeReference("OAuth2", document), ["Pms"]
				},
				{
					new OpenApiSecuritySchemeReference("Bearer", document), []
				},
				{
					new OpenApiSecuritySchemeReference("ApiKey", document), []
				}
			});
		});
	}

    private static void ConfigureDataProtection(IServiceCollection services,  IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
    {
        if (!hostingEnvironment.IsDevelopment())
        {
			// 集群环境，多实例共享（使用Redis存储，支持微服务架构和容器化部署）
			//var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]);
			//services.AddDataProtection()
			//	.SetApplicationName("Pms")
			//	.PersistKeysToStackExchangeRedis(redis, "data-protection-keys");

			var keysFolder = Path.Combine(hostingEnvironment.ContentRootPath, "data-protection-keys");
			if (!Directory.Exists(keysFolder))
			{
				Directory.CreateDirectory(keysFolder);
			}
			var build = services.AddDataProtection()
				.SetApplicationName("Pms")
				.PersistKeysToFileSystem(new DirectoryInfo(keysFolder));
			var certPath = Path.Combine(hostingEnvironment.ContentRootPath, "openiddict.pfx");
			if (File.Exists(certPath))
			{
				var certificate = X509CertificateLoader.LoadPkcs12FromFile(certPath, configuration["AuthServer:CertificatePassPhrase"]);
				build.ProtectKeysWithCertificate(certificate);
			}
		}
	}

    private static void ConfigureDistributedLocking(IServiceCollection services)
    {
		services.AddSingleton<IDistributedLockProvider>(sp =>
		{
			// 使用 Redis 作为分布式锁提供程序
			//var connection = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]!);
			//return new RedisDistributedSynchronizationProvider(connection.GetDatabase());

			var lockFileDirectory = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "distributed-locks"));
			if (!lockFileDirectory.Exists)
			{
				lockFileDirectory.Create();
			}
			return new FileDistributedSynchronizationProvider(lockFileDirectory);
		});
	}

    private static void ConfigureCors(IServiceCollection services, IConfiguration configuration)
    {
		var allowedOrigins = configuration.GetSection("App:CorsOrigins").Get<string[]>() ?? [];
		services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
				builder.WithOrigins(allowedOrigins)
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
		app.UseAbpRequestLocalization();

		app.UseTokenHeaderTransform();
		app.UseApiResponseHandler();

		app.UseCorrelationId();
        app.MapAbpStaticAssets();
        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
		app.UseAbpOpenIddictValidation();

		app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();

		app.UseSwagger();
		app.UseAbpSwaggerUI(options =>
		{
			options.SwaggerEndpoint("/swagger/v1/swagger.json", "Pms API");

			var configuration = context.GetConfiguration();
			options.OAuthClientId(configuration["AuthServer:Applications:Swagger:ClientId"]);
			options.OAuthScopes("Pms");
			options.OAuthUsePkce();

			// UI 配置
			options.DefaultModelsExpandDepth(-1);
			options.DisplayRequestDuration();
		});

		//app.UseAuditing();
		app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}