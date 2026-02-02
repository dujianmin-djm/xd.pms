using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Authentication.OpenIdConnect;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared.Toolbars;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity.Web;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;
using XD.Pms.Localization;
using XD.Pms.Permissions;
using XD.Pms.Web.HealthChecks;
using XD.Pms.Web.Menus;

namespace XD.Pms.Web;

[DependsOn(
	typeof(PmsHttpApiClientModule),
	typeof(PmsHttpApiModule),
	typeof(AbpAutofacModule),
	typeof(AbpAspNetCoreAuthenticationOpenIdConnectModule),
	typeof(AbpIdentityWebModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
	//typeof(AbpAccountWebModule),
	//typeof(AbpAccountWebOpenIddictModule),
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
                typeof(PmsDomainSharedModule).Assembly,
                typeof(PmsApplicationContractsModule).Assembly,
                typeof(PmsWebModule).Assembly
            );
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

		ConfigureBundles();
        ConfigureUrls(configuration);
        ConfigureAuthentication(context.Services, configuration);
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureNavigationServices();
        ConfigureSwaggerServices(context.Services);
		context.Services.AddPmsHealthChecksUi();

		Configure<RazorPagesOptions>(options =>
        {
			options.Conventions.AuthorizePage("/Books/Index", PmsPermissions.Books.Default);
            options.Conventions.AuthorizePage("/Books/CreateModal", PmsPermissions.Books.Create);
            options.Conventions.AuthorizePage("/Books/EditModal", PmsPermissions.Books.Edit);
        });

		ConfigureDataProtection(context.Services, hostingEnvironment);

		Configure<AbpAntiForgeryOptions>(options =>
        {
			// 禁用基于 Cookie 的防伪造验证 (适用 JWT 纯 API 场景，没有 Razor Pages/MVC 视图)
			options.AutoValidate = true;
			options.AutoValidateFilter = type => type.Namespace?.Contains("Controllers") != true;

			// 设置 AntiForgery Cookie 策略
            options.TokenCookie.Name = "PMS.XSRF-TOKEN";
			options.TokenCookie.SameSite = SameSiteMode.Lax;
            options.TokenCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });

		context.Services.AddHttpContextAccessor();
	}

	private static void ConfigureDataProtection(IServiceCollection services, IWebHostEnvironment hostingEnvironment)
	{
		if (!hostingEnvironment.IsDevelopment())
        {
			// 修复 IIS 重启程序池后密钥丢失导致已发放的 Antiforgery / Token 解密失败的问题
			var keysFolder = Path.Combine(hostingEnvironment.ContentRootPath, "DataProtection-Keys");

			if (!Directory.Exists(keysFolder))
			{
				Directory.CreateDirectory(keysFolder);
			}

			services.AddDataProtection()
				.SetApplicationName("PmsApp")
				.PersistKeysToFileSystem(new DirectoryInfo(keysFolder));
		}
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

    private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
		//services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
		services.AddAuthentication(options =>
		{
			options.DefaultScheme = "Cookies";
			options.DefaultChallengeScheme = "oidc";
		})
			.AddCookie("Cookies", options =>
			{
				options.ExpireTimeSpan = TimeSpan.FromDays(365);
				options.CheckTokenExpiration("oidc");

				options.Events.OnRedirectToLogin = context =>
				{
					// API 请求返回 401，页面请求重定向到登录
					if (context.Request.Path.StartsWithSegments("/api") || 
						context.Request.Path.StartsWithSegments("/papi"))
					{
						context.Response.StatusCode = 401;
						return Task.CompletedTask;
					}
					context.Response.Redirect(context.RedirectUri);
					return Task.CompletedTask;
				};
			})
			.AddAbpOpenIdConnect("oidc", options =>
			{
				options.Authority = configuration["AuthServer:Authority"];
				options.RequireHttpsMetadata = configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata");
				options.ClientId = configuration["AuthServer:ClientId"];
				options.ClientSecret = configuration["AuthServer:ClientSecret"];

				options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
				options.UsePkce = true;
				options.SaveTokens = true;
				options.GetClaimsFromUserInfoEndpoint = true;

				options.Scope.Add("roles");
				options.Scope.Add("email");
				options.Scope.Add("phone");
				options.Scope.Add("Pms");

				// Token 验证参数
				//options.TokenValidationParameters = new TokenValidationParameters
				//{
				//	NameClaimType = "name",
				//	RoleClaimType = "role",
				//	ValidateIssuer = true,
				//	ValidateAudience = false
				//};

				//// 事件处理
				//options.Events = new OpenIdConnectEvents
				//{
				//	OnTokenValidated = async context =>
				//	{
				//		// 可以在这里添加额外的Claims处理
				//		var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;

				//		// 获取 access_token 用于后续 API 调用
				//		var accessToken = context.TokenEndpointResponse?.AccessToken;
				//		if (!string.IsNullOrEmpty(accessToken) && claimsIdentity != null)
				//		{
				//			claimsIdentity.AddClaim(new Claim("access_token", accessToken));
				//		}
				//		await Task.CompletedTask;
				//	},
				//	OnRedirectToIdentityProvider = context =>
				//	{
				//		// 可以添加额外的参数
				//		// context.ProtocolMessage.SetParameter("custom_param", "value");
				//		return Task.CompletedTask;
				//	},
				//	OnRemoteFailure = context =>
				//	{
				//		context.Response.Redirect($"/Error?message={context.Failure?.Message}");
				//		context.HandleResponse();
				//		return Task.CompletedTask;
				//	},
				//	OnAccessDenied = context =>
				//	{
				//		context.Response.Redirect("/Error?message=Access Denied");
				//		context.HandleResponse();
				//		return Task.CompletedTask;
				//	}
				//};
			});
		services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
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
                options.FileSets.ReplaceEmbeddedByPhysical<PmsApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, string.Format("..{0}XD.Pms.Application.Contracts", Path.DirectorySeparatorChar)));
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
                    ["Pms"]
				}
			});

		});
	}

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
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
            app.UseErrorPage();
            //app.UseHsts();
        }

        app.UseCorrelationId();
        app.UseRouting();

		app.UseCors();

		app.MapAbpStaticAssets();
        app.UseAbpSecurityHeaders();
        app.UseAuthentication();

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
