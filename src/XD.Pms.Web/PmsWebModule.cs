using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
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
using XD.Pms.Localization;
using XD.Pms.Permissions;
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
            });
        });

        if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

			// 配置 OpenIddict 服务端
			PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
			{
                // 启用 Password Grant（用于前端直接登录）
				serverBuilder.AllowPasswordFlow();

				// 启用 Refresh Token
				serverBuilder.AllowRefreshTokenFlow();

				// 启用 Client Credentials 客户端凭证（服务间调用，微服务间调用、定时任务）
				serverBuilder.AllowClientCredentialsFlow();

				// 设置 Token 有效期
				serverBuilder.SetAccessTokenLifetime(TimeSpan.FromMinutes(30));
				serverBuilder.SetRefreshTokenLifetime(TimeSpan.FromDays(7));

				// 开发环境禁用 HTTPS 要求（生产环境必须启用）
				serverBuilder.UseAspNetCore().DisableTransportSecurityRequirement();

				serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx", configuration["AuthServer:CertificatePassPhrase"]!);
                serverBuilder.SetIssuer(new Uri(configuration["AuthServer:Authority"]!));
            });
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        if (!configuration.GetValue<bool>("App:DisablePII"))
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
			//options.Conventions.AuthorizePage("/Index", PmsPermissions.Books.Default);
			options.Conventions.AuthorizePage("/Books/Index", PmsPermissions.Books.Default);
            options.Conventions.AuthorizePage("/Books/CreateModal", PmsPermissions.Books.Create);
            options.Conventions.AuthorizePage("/Books/EditModal", PmsPermissions.Books.Edit);
        });


		// 配置 JWT Settings
		ConfigureJwtSettings(context, configuration);

		// 配置 JWT 认证
		//ConfigureJwtAuthentication(context, configuration);


		//Configure<AbpAntiForgeryOptions>(options =>
		//{
		//    options.TokenCookie.SameSite = SameSiteMode.Lax;
		//    options.TokenCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
		//});
	}

	private static void ConfigureJwtSettings(ServiceConfigurationContext context, IConfiguration configuration)
	{
		context.Services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
	}

	private static void ConfigureJwtAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
	{
		var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;

		context.Services.AddAuthentication(options =>
		{
			// 把默认认证方案改成了 JwtBearer（DefaultAuthenticate / Challenge / Scheme 都指向 JWT）。
			// 这会把整个应用（包括 Razor Pages UI）默认的认证方式改为 Bearer Token。

			//options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			//options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			//options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

			// 取消把应用的默认认证方案强制设为 JWT。保留框架 / Identity / OpenIddict 为默认（用于交互式登录和 Cookie）
            // 只注册 JwtBearer 用于 API / SignalR 场景。
		})
		.AddJwtBearer(options =>
		{
			options.SaveToken = true;
			options.RequireHttpsMetadata = false; // 生产环境建议设为 true
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ClockSkew = TimeSpan.Zero, // 不允许时钟偏移
				ValidIssuer = jwtSettings.Issuer,
				ValidAudience = jwtSettings.Audience,
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
			};

			// 从 Query String 读取 Token（用于 SignalR 等场景）
			options.Events = new JwtBearerEvents
			{
				OnMessageReceived = context =>
				{
					var accessToken = context.Request.Query["access_token"];
					var path = context.HttpContext.Request.Path;

					// SignalR Hub 路径
					if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/signalr-hubs"))
					{
						context.Token = accessToken;
					}
					return Task.CompletedTask;
				},
				OnAuthenticationFailed = context =>
				{
					if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
					{
						context.Response.Headers.Append("Token-Expired", "true");
					}
					return Task.CompletedTask;
				}
			};
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

	/// <summary>
	///  ABP 的约定式 API 控制器自动注册，它会自动将 Application 层中的服务注册为 API 控制器
	/// </summary>
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
			options.SwaggerDoc("v1", new OpenApiInfo { Title = "Pms API", Version = "v1" });
			options.DocInclusionPredicate((docName, description) => true);
			options.CustomSchemaIds(type => type.FullName);

			// 添加 JWT 认证配置
			options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
			{
				Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
				Name = "Authorization",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.ApiKey,
				Scheme = "Bearer"
			});

			options.AddSecurityRequirement(new OpenApiSecurityRequirement
		    {
			    {
				    new OpenApiSecurityScheme
				    {
					    Reference = new OpenApiReference
					    {
						    Type = ReferenceType.SecurityScheme,
						    Id = "Bearer"
					    }
				    },
				    Array.Empty<string>()
			    }
		    });
		});
	}


    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
		await base.OnApplicationInitializationAsync(context);
		// 注册刷新令牌清理后台任务
		await context.AddBackgroundWorkerAsync<RefreshTokenCleanupWorker>();

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
