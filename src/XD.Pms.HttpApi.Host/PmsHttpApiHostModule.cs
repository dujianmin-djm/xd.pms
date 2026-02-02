using Autofac.Core;
using Medallion.Threading;
using Medallion.Threading.FileSystem;
using Microsoft.AspNetCore.Authentication.Cookies;
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
using Microsoft.OpenApi.Models;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
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
using Volo.Abp.VirtualFileSystem;
using XD.Pms.ApiKeys;
using XD.Pms.Authentication;
using XD.Pms.Authentication.ApiKey;
using XD.Pms.EntityFrameworkCore;
using XD.Pms.Filters;
using XD.Pms.HealthChecks;
using XD.Pms.Middlewares;

namespace XD.Pms;

[DependsOn(
    typeof(PmsHttpApiModule),
	typeof(PmsApplicationModule),
	typeof(PmsEntityFrameworkCoreModule),
	typeof(AbpAutofacModule),
	typeof(AbpDistributedLockingModule),
	//typeof(AbpCachingStackExchangeRedisModule), // 取消 Redis 缓存
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

		// 配置 OpenIddict 验证
		PreConfigure<OpenIddictBuilder>(builder =>
		{
			builder.AddValidation(options =>
			{
				/* 微服务架构（API 网关 + 单独认证服务）可以配置单独指向 AuthServer 的授权服务器 
				 * 须同时启用 app.UseAbpOpenIddictValidation(); 用于远程令牌验证
				 * options.SetIssuer(configuration["AuthServer:Authority"]!);
				 */
				options.AddAudiences("Pms");
				options.UseLocalServer();	//本地验证，不需要指向授权服务器
				options.UseAspNetCore();

				// 添加事件验证处理器
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
        ConfigureAuthentication(context.Services, configuration);
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureDataProtection(context.Services, hostingEnvironment);
        ConfigureDistributedLocking(context.Services);
        ConfigureCors(context.Services, configuration);
        ConfigureSwaggerServices(context.Services, configuration);

		// 显示详细身份认证错误信息
		if (hostingEnvironment.IsDevelopment())
		{
			IdentityModelEventSource.ShowPII = true;
			IdentityModelEventSource.LogCompleteSecurityArtifact = true;
		}

		// 禁用 HTTPS 要求
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
		});

		// 注册 TokenSettings
		context.Services.Configure<TokenSettings>(configuration.GetSection(TokenSettings.SectionName));

		// 注册 HttpClient
		context.Services.AddHttpClient();

		context.Services.AddPmsHealthChecks();

		// 添加响应包装过滤器
		Configure<MvcOptions>(options =>
		{
			options.Filters.Add<ApiResponseWrapperFilter>();
		});

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

				// 3. Cookie （保留 ABP Web UI 功能）
				providers.Add(cookieProvider ?? new CookieRequestCultureProvider());

				// 4. Accept-Language 请求头
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

    private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
	{
		//// 配置默认认证方案
		//services.AddAuthentication(options =>
		//{
		//	options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
		//	options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
		//	options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
		//});

		//services.AddAuthentication().AddApiKey(options =>
		//{
		//	options.HeaderName = configuration["AuthServer:ApiKey:HeaderName"] ?? ApiKeyConsts.DefaultHeaderName;
		//	options.Realm = configuration["AuthServer:ApiKey:Realm"] ?? "Pms API";
		//	options.Enabled = configuration.GetValue("AuthServer:ApiKey:Enabled", true);
		//});

		services.ConfigureApplicationCookie(options =>
		{
			options.LoginPath = "/connect/login";
			options.LogoutPath = "/connect/logout";
			options.Cookie.Name = "Pms.Identity";
			options.Cookie.HttpOnly = true;
			options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
			options.ExpireTimeSpan = TimeSpan.FromDays(7);
			options.SlidingExpiration = true;

			options.ForwardDefaultSelector = ctx =>
			{
				string authorization = ctx.Request.Headers.Authorization.ToString();
				if (!authorization.IsNullOrWhiteSpace() && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
				{
					// 认证方案转发到 OpenIddict
					return OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme ?? "Bearer";
				}
				// 默认：Cookie 认证
				return null;
			};

			options.Events.OnRedirectToLogin = context =>
			{
				if (IsApiRequest(context.Request))
				{
					// Cookie 认证，对 API 请求返回 401 而非重定向
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

		// 配置授权策略 可以配合Controller [Authorize(AuthenticationSchemes = "ApiKeyOnly")]
		services.AddAuthorizationBuilder()
			.AddPolicy("ApiAccess", policy =>
			{
				policy.AddAuthenticationSchemes(
					OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
					ApiKeyAuthenticationOptions.DefaultScheme);
				policy.RequireAuthenticatedUser();
			})
			.AddPolicy("ApiKeyOnly", policy =>
			{
				policy.AddAuthenticationSchemes(ApiKeyAuthenticationOptions.DefaultScheme);
				policy.RequireAuthenticatedUser();
			})
			.AddPolicy("BearerOnly", policy =>
			{
				policy.AddAuthenticationSchemes(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
				policy.RequireAuthenticatedUser();
			});
	}

	private static bool IsApiRequest(HttpRequest request)
	{
		var path = request.Path.Value?.ToLower() ?? "";
		return path.StartsWith("/papi/") || path.StartsWith("/api/");
	}

	private static void ConfigureSwaggerServices(IServiceCollection services, IConfiguration configuration)
    {
		var authority = configuration["AuthServer:Authority"]!.TrimEnd('/');// 认证服务地址
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

			options.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "OAuth2" }
					},
					["Pms"]
				},
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
					},
					Array.Empty<string>()
				},
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" }
					},
					Array.Empty<string>()
				}
			});

		});
	}

    private static void ConfigureDataProtection(IServiceCollection services, IWebHostEnvironment hostingEnvironment)
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
			services.AddDataProtection()
				.SetApplicationName("Pms")
				.PersistKeysToFileSystem(new DirectoryInfo(keysFolder));
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