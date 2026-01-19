using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RequestLocalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared.Toolbars;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity.Web;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Security.Claims;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;
using XD.Pms.Authentication;
using XD.Pms.EntityFrameworkCore;
using XD.Pms.Filters;
using XD.Pms.Localization;
using XD.Pms.Permissions;
using XD.Pms.Web.Authentication;
using XD.Pms.Web.HealthChecks;
using XD.Pms.Web.Menus;

namespace XD.Pms.Web;

[DependsOn(
    typeof(PmsHttpApiModule),
    typeof(PmsApplicationModule),
    typeof(PmsEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpIdentityWebModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpFeatureManagementWebModule),
    typeof(AbpSwashbuckleModule),
    typeof(AbpAspNetCoreSerilogModule)
)]
public class PmsWebModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(PmsResource),
                typeof(PmsDomainModule).Assembly,
                typeof(PmsDomainSharedModule).Assembly,
                typeof(PmsApplicationModule).Assembly,
                typeof(PmsApplicationContractsModule).Assembly,
                typeof(PmsWebModule).Assembly
            );
        });

		PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("Pms");
                options.UseLocalServer();
                options.UseAspNetCore();

				// 添加黑名单验证处理器
				options.AddEventHandler(TokenBlacklistValidationHandler.Descriptor);
			});
        });

		// 配置 OpenIddict 服务器
		var tokenSettings = configuration.GetSection(TokenSettings.SectionName).Get<TokenSettings>();
        int accessTokenExpirationMinutes = tokenSettings?.AccessTokenExpirationMinutes ?? 30;
        int refreshTokenExpirationDays = tokenSettings?.RefreshTokenExpirationDays ?? 7;
		PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
		{
			serverBuilder.SetAccessTokenLifetime(TimeSpan.FromMinutes(accessTokenExpirationMinutes));
			serverBuilder.SetRefreshTokenLifetime(TimeSpan.FromDays(refreshTokenExpirationDays));
			//serverBuilder.SetIdentityTokenLifetime(null);

			// ======== 启用 Reference Tokens ========
			// Access Token 使用 Reference Token（存储在数据库，支持即时撤销）
			//serverBuilder.UseReferenceAccessTokens();

			// Refresh Token 使用 Reference Token
			//serverBuilder.UseReferenceRefreshTokens();
			// ======== 未用，使用 Token 黑名单代替 ========
		});

		if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

			PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
			{
				serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx", configuration["AuthServer:CertificatePassPhrase"]!);
                serverBuilder.SetIssuer(new Uri(configuration["AuthServer:Authority"]!));
            });
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

		// 显示详细身份认证错误信息
		if (!configuration.GetValue<bool>("App:DisablePII"))
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

		ConfigureBundles();
        ConfigureUrls(configuration);
        ConfigureHealthChecks(context);
        ConfigureAuthentication(context);
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureNavigationServices();
        ConfigureAutoApiControllers();
        ConfigureSwaggerServices(context.Services);

		Configure<PermissionManagementOptions>(options =>
        {
            options.IsDynamicPermissionStoreEnabled = true;
        });
        
        Configure<RazorPagesOptions>(options =>
        {
			options.Conventions.AuthorizePage("/Books/Index", PmsPermissions.Books.Default);
            options.Conventions.AuthorizePage("/Books/CreateModal", PmsPermissions.Books.Create);
            options.Conventions.AuthorizePage("/Books/EditModal", PmsPermissions.Books.Edit);
        });

		// 注册 TokenSettings
		context.Services.Configure<TokenSettings>(configuration.GetSection(TokenSettings.SectionName));

		// 注册 HttpClient
		context.Services.AddHttpClient();

		// 配置 CORS
		ConfigureCors(context, configuration);

        // 添加响应包装过滤器
        Configure<MvcOptions>(options =>
        {
            //options.Filters.Add<ApiResponseWrapperFilter>();
        });

        ConfigureCookieAuthentication();
		ConfigureRequestLocalization();
		ConfigureDataProtection(context, hostingEnvironment);

		Configure<AbpAntiForgeryOptions>(options =>
        {
            options.TokenCookie.SameSite = SameSiteMode.Lax;
            options.TokenCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });
    }

	private static void ConfigureDataProtection(ServiceConfigurationContext context, IWebHostEnvironment hostingEnvironment)
	{
		if (!hostingEnvironment.IsDevelopment())
        {
			// 修复 IIS 重启程序池后密钥丢失导致已发放的 Antiforgery / Token 解密失败的问题
			var keysFolder = Path.Combine(hostingEnvironment.ContentRootPath, "DataProtection-Keys");

			if (!Directory.Exists(keysFolder))
			{
				Directory.CreateDirectory(keysFolder);
			}

			context.Services.AddDataProtection()
				.SetApplicationName("PmsApp")
				.PersistKeysToFileSystem(new DirectoryInfo(keysFolder));
		}
	}

	private void ConfigureRequestLocalization()
    {
		Configure<AbpRequestLocalizationOptions>(options =>
		{
			options.RequestLocalizationOptionConfigurators.Add(async (serviceProvider, requestLocalizationOptions) =>
            {
				requestLocalizationOptions.DefaultRequestCulture = new RequestCulture("zh-Hans");

    //            var supportedCultures = new[]
    //            {
    //                new CultureInfo("zh-Hans"),
    //                new CultureInfo("zh-Hant"),
    //                new CultureInfo("en")
    //            };
    //            requestLocalizationOptions.SupportedCultures = supportedCultures;
    //            requestLocalizationOptions.SupportedUICultures = supportedCultures;

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

				//await Task.CompletedTask;
			});
		});
	}

	private void ConfigureCookieAuthentication()
    {
		// 配置 Cookie 认证，对 API 请求返回 401 而非重定向
		Configure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
		{
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
	}

	private static bool IsApiRequest(HttpRequest request)
	{
		var path = request.Path.Value?.ToLower() ?? "";
		return path.StartsWith("/api/") ||
			   request.Headers.XRequestedWith.ToString() == "XMLHttpRequest" ||
			   request.Headers.Accept.ToString().Contains("application/json");
	}

	private static void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
	{
		// 配置 CORS 策略，允许前端应用跨域访问 API
		var corsSettings = configuration.GetSection("Cors");
		var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? [];
		var allowedMethods = corsSettings.GetSection("AllowedMethods").Get<string[]>() ?? ["GET"];

		context.Services.AddCors(options =>
		{
			options.AddDefaultPolicy(builder =>
			{
				builder.WithOrigins(allowedOrigins)
					.WithAbpExposedHeaders()
					.SetIsOriginAllowedToAllowWildcardSubdomains()
					.AllowAnyHeader()
					.WithMethods(allowedMethods)
					.AllowCredentials();
			});
		});
	}

	private static void ConfigureHealthChecks(ServiceConfigurationContext context)
    {
        context.Services.AddPmsHealthChecks();
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );

            options.ScriptBundles.Configure(
                LeptonXLiteThemeBundles.Scripts.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-scripts.js");
                }
            );
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
        });
    }

    private static void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<PmsWebModule>();

            if (hostingEnvironment.IsDevelopment())
            {
                options.FileSets.ReplaceEmbeddedByPhysical<PmsDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}XD.Pms.Domain.Shared", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<PmsDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}XD.Pms.Domain", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<PmsApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}XD.Pms.Application.Contracts", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<PmsApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}XD.Pms.Application", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<PmsHttpApiModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}..{0}src{0}XD.Pms.HttpApi", Path.DirectorySeparatorChar)));
                options.FileSets.ReplaceEmbeddedByPhysical<PmsWebModule>(hostingEnvironment.ContentRootPath);
            }
        });
    }

    private void ConfigureNavigationServices()
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new PmsMenuContributor());
        });

        Configure<AbpToolbarOptions>(options =>
        {
            options.Contributors.Add(new PmsToolbarContributor());
        });
    }

	private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(PmsApplicationModule).Assembly);
        });
    }

    private static void ConfigureSwaggerServices(IServiceCollection services)
    {
		services.AddAbpSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo { Title = "Pms API", Version = "v1", Description = "" });
			options.DocInclusionPredicate((docName, description) => true);
			options.CustomSchemaIds(type => type.FullName);

			// 添加 OAuth2 密码模式支持
			options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
			{
				Type = SecuritySchemeType.OAuth2,
				Flows = new OpenApiOAuthFlows
				{
					Password = new OpenApiOAuthFlow
					{
						TokenUrl = new Uri("/connect/token", UriKind.Relative),
						Scopes = new Dictionary<string, string>
						{
							["openid"] = "OpenID",
							["profile"] = "Profile",
							["email"] = "Email",
							["roles"] = "Roles",
							["Pms"] = "Pms API"
						}
					}
				},
				Description = "OAuth2 密码模式认证"
			});

			options.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = "oauth2"
						}
					},
					new[] { "Pms" }
				}
			});

		});
	}


    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
		await base.OnApplicationInitializationAsync(context);
		// 注册刷新令牌清理后台任务
		// await context.AddBackgroundWorkerAsync<RefreshTokenCleanupWorker>();

		var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

		app.UseForwardedHeaders();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

		app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            //app.UseErrorPage();
            //app.UseHsts();
        }

        app.UseCorrelationId();
        app.UseRouting();

		app.UseCors();

		app.MapAbpStaticAssets();
        app.UseAbpSecurityHeaders();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Pms API");
		});
        //app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
