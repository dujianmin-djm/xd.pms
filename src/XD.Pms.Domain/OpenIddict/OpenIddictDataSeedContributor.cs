using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using OpenIddict.Abstractions;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.OpenIddict.Applications;
using Volo.Abp.OpenIddict.Scopes;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;

namespace XD.Pms.OpenIddict;

/* Creates initial data that is needed to property run the application
 * and make client-to-server communication possible.
 */
public class OpenIddictDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IConfiguration _configuration;
    private readonly IOpenIddictApplicationRepository _openIddictApplicationRepository;
    private readonly IAbpApplicationManager _applicationManager;
    private readonly IOpenIddictScopeRepository _openIddictScopeRepository;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly IPermissionDataSeeder _permissionDataSeeder;
    private readonly IStringLocalizer<OpenIddictResponse> L;

    public OpenIddictDataSeedContributor(
        IConfiguration configuration,
        IOpenIddictApplicationRepository openIddictApplicationRepository,
        IAbpApplicationManager applicationManager,
        IOpenIddictScopeRepository openIddictScopeRepository,
        IOpenIddictScopeManager scopeManager,
        IPermissionDataSeeder permissionDataSeeder,
        IStringLocalizer<OpenIddictResponse> l)
    {
        _configuration = configuration;
        _openIddictApplicationRepository = openIddictApplicationRepository;
        _applicationManager = applicationManager;
        _openIddictScopeRepository = openIddictScopeRepository;
        _scopeManager = scopeManager;
        _permissionDataSeeder = permissionDataSeeder;
        L = l;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        await CreateScopesAsync();
        await CreateApplicationsAsync();
    }

    private async Task CreateScopesAsync()
    {
        if (await _openIddictScopeRepository.FindByNameAsync("Pms") == null)
        {
            await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor {
                Name = "Pms", DisplayName = "Pms API", Resources = { "Pms" }
            });
        }
    }

    private async Task CreateApplicationsAsync()
    {
        var commonScopes = new List<string> {
            OpenIddictConstants.Permissions.Scopes.Address,
            OpenIddictConstants.Permissions.Scopes.Email,
            OpenIddictConstants.Permissions.Scopes.Phone,
            OpenIddictConstants.Permissions.Scopes.Profile,
            OpenIddictConstants.Permissions.Scopes.Roles,
			"Pms"
        };

        var configurationSection = _configuration.GetSection("OpenIddict:Applications");

		// SPA Client
		var spaClientId = configurationSection["Spa:ClientId"];
		if (!spaClientId.IsNullOrWhiteSpace())
		{
			var vueClientRootUrl = configurationSection["Spa:RootUrl"]?.TrimEnd('/') ?? string.Empty;

			await CreateApplicationAsync(
				applicationType: OpenIddictConstants.ApplicationTypes.Web,
				name: spaClientId!,
				type: OpenIddictConstants.ClientTypes.Public,
				consentType: OpenIddictConstants.ConsentTypes.Implicit,
				displayName: "SPA Front-end Application",
				secret: null,
				grantTypes:
				[
					OpenIddictConstants.GrantTypes.Password,
                    OpenIddictConstants.GrantTypes.RefreshToken,
                    OpenIddictConstants.GrantTypes.AuthorizationCode,
				],
				scopes: [..commonScopes, OpenIddictConstants.Scopes.OfflineAccess],
				redirectUris: [$"{vueClientRootUrl}/callback"],
				postLogoutRedirectUris: [vueClientRootUrl],
				clientUri: vueClientRootUrl,
				logoUri: "/images/clients/vue.svg"
			);
		}

		//Web Client
		var webClientId = configurationSection["Web:ClientId"];
		if (!webClientId.IsNullOrWhiteSpace())
		{
			var webClientRootUrl = configurationSection["Web:RootUrl"]!.EnsureEndsWith('/');
			await CreateApplicationAsync(
				applicationType: OpenIddictConstants.ApplicationTypes.Web,
				name: webClientId!,
				type: OpenIddictConstants.ClientTypes.Confidential,
				consentType: OpenIddictConstants.ConsentTypes.Implicit,
				displayName: "Web Application",
				secret: configurationSection["Web:ClientSecret"] ?? "1q2w3e*",
				grantTypes:
                [
					OpenIddictConstants.GrantTypes.AuthorizationCode, 
                    OpenIddictConstants.GrantTypes.Implicit
				],
				scopes: commonScopes,
				redirectUris: [$"{webClientRootUrl}signin-oidc"],
				clientUri: webClientRootUrl,
				postLogoutRedirectUris: [$"{webClientRootUrl}signout-callback-oidc"],
				logoUri: "/images/clients/angular.svg"
			);
		}

		// mobile Client
		var mobileClientId = configurationSection["Mobile:ClientId"];
		if (!mobileClientId.IsNullOrWhiteSpace())
		{
			await CreateApplicationAsync(
				applicationType: OpenIddictConstants.ApplicationTypes.Native,
				name: mobileClientId!,
				type: OpenIddictConstants.ClientTypes.Public,
				consentType: OpenIddictConstants.ConsentTypes.Implicit,
				displayName: "Mobile Application (UniApp/Android)",
				secret: null,
				grantTypes:
				[
					OpenIddictConstants.GrantTypes.Password,
					OpenIddictConstants.GrantTypes.RefreshToken,
				],
				scopes: [.. commonScopes, OpenIddictConstants.Scopes.OfflineAccess],
				redirectUris: null,
				postLogoutRedirectUris: null,
				clientUri: null,
				logoUri: "/images/clients/mobile.svg"
			);
		}

		// swagger Client
		var swaggerClientId = configurationSection["Swagger:ClientId"];
		if (!swaggerClientId.IsNullOrWhiteSpace())
		{
			var swaggerRootUrl = configurationSection["Swagger:RootUrl"]?.TrimEnd('/');

			await CreateApplicationAsync(
				applicationType: OpenIddictConstants.ApplicationTypes.Web,
				name: swaggerClientId!,
				type: OpenIddictConstants.ClientTypes.Public,
				consentType: OpenIddictConstants.ConsentTypes.Implicit,
				displayName: "Swagger UI",
				secret: null,
				grantTypes:
				[
					OpenIddictConstants.GrantTypes.AuthorizationCode,
					OpenIddictConstants.GrantTypes.Password,
                ],
				scopes: commonScopes,
				redirectUris: [$"{swaggerRootUrl}/swagger/oauth2-redirect.html"],
				clientUri: swaggerRootUrl?.EnsureEndsWith('/') + "swagger",
				logoUri: "/images/clients/swagger.svg"
			);
		}

		// Internal Service Client
		var serviceClientId = configurationSection["Service:ClientId"];
		if (!serviceClientId.IsNullOrWhiteSpace())
		{
			var serviceSecret = configurationSection["Service:ClientSecret"];

			await CreateApplicationAsync(
				applicationType: OpenIddictConstants.ApplicationTypes.Web,
				name: serviceClientId!,
				type: OpenIddictConstants.ClientTypes.Confidential,
				consentType: OpenIddictConstants.ConsentTypes.Implicit,
				displayName: "Internal Service",
				secret: serviceSecret,
				grantTypes: [OpenIddictConstants.GrantTypes.ClientCredentials],
				scopes: ["Pms"],    //通常只需要API作用域
				redirectUris: null,
				postLogoutRedirectUris: null,
				clientUri: null,
				logoUri: null
			);
		}




		//Console Test / Angular Client
		var consoleAndAngularClientId = configurationSection["Pms_App:ClientId"];
        if (!consoleAndAngularClientId.IsNullOrWhiteSpace())
        {
            var consoleAndAngularClientRootUrl = configurationSection["Pms_App:RootUrl"]?.TrimEnd('/') ?? string.Empty;
            await CreateApplicationAsync(
                applicationType: OpenIddictConstants.ApplicationTypes.Web,
                name: consoleAndAngularClientId!,
                type: OpenIddictConstants.ClientTypes.Public,
                consentType: OpenIddictConstants.ConsentTypes.Implicit,
                displayName: "Console Test / Angular Application",
                secret: null,
                grantTypes: [
                    OpenIddictConstants.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.GrantTypes.Password,
                    OpenIddictConstants.GrantTypes.ClientCredentials,
                    OpenIddictConstants.GrantTypes.RefreshToken,
                    "LinkLogin",
					"Impersonation" //Token 内省
                ],
                scopes: commonScopes,
                redirectUris: [consoleAndAngularClientRootUrl],
                postLogoutRedirectUris: [consoleAndAngularClientRootUrl],
                clientUri: consoleAndAngularClientRootUrl,
                logoUri: "/images/clients/angular.svg"
            );
        }

    }

    private async Task CreateApplicationAsync(
        [NotNull] string applicationType,
        [NotNull] string name,
        [NotNull] string type,
        [NotNull] string consentType,
        string displayName,
        string? secret,
        List<string> grantTypes,
        List<string> scopes,
        List<string>? redirectUris = null,
        List<string>? postLogoutRedirectUris = null,
        List<string>? permissions = null,
        string? clientUri = null,
        string? logoUri = null,
		bool requirePkce = false)
    {
        if (!string.IsNullOrEmpty(secret) && string.Equals(type, OpenIddictConstants.ClientTypes.Public,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(L["NoClientSecretCanBeSetForPublicApplications"]);
        }

        if (string.IsNullOrEmpty(secret) && string.Equals(type, OpenIddictConstants.ClientTypes.Confidential,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(L["TheClientSecretIsRequiredForConfidentialApplications"]);
        }

        var client = await _openIddictApplicationRepository.FindByClientIdAsync(name);

        var application = new AbpApplicationDescriptor {
            ApplicationType = applicationType,
            ClientId = name,
            ClientType = type,
            ClientSecret = secret,
            ConsentType = consentType,
            DisplayName = displayName,
            ClientUri = clientUri,
            LogoUri = logoUri,
        };

        Check.NotNullOrEmpty(grantTypes, nameof(grantTypes));
        Check.NotNullOrEmpty(scopes, nameof(scopes));

        if (new[] { OpenIddictConstants.GrantTypes.AuthorizationCode, OpenIddictConstants.GrantTypes.Implicit }.All(
                grantTypes.Contains))
        {
            application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken);

            if (string.Equals(type, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeIdTokenToken);
                application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeToken);
            }
        }

        if (!redirectUris.IsNullOrEmpty() || !postLogoutRedirectUris.IsNullOrEmpty())
        {
            application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.EndSession);
        }

        var buildInGrantTypes = new[] {
            OpenIddictConstants.GrantTypes.Implicit, OpenIddictConstants.GrantTypes.Password,
            OpenIddictConstants.GrantTypes.AuthorizationCode, OpenIddictConstants.GrantTypes.ClientCredentials,
            OpenIddictConstants.GrantTypes.DeviceCode, OpenIddictConstants.GrantTypes.RefreshToken
        };

        foreach (var grantType in grantTypes)
        {
            if (grantType == OpenIddictConstants.GrantTypes.AuthorizationCode)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
                application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Code);
            }

            if (grantType == OpenIddictConstants.GrantTypes.AuthorizationCode ||
                grantType == OpenIddictConstants.GrantTypes.Implicit)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Authorization);
            }

            if (grantType == OpenIddictConstants.GrantTypes.AuthorizationCode ||
                grantType == OpenIddictConstants.GrantTypes.ClientCredentials ||
                grantType == OpenIddictConstants.GrantTypes.Password ||
                grantType == OpenIddictConstants.GrantTypes.RefreshToken ||
                grantType == OpenIddictConstants.GrantTypes.DeviceCode)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
                application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Revocation);
                application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Introspection);
            }

            if (grantType == OpenIddictConstants.GrantTypes.ClientCredentials)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
            }

            if (grantType == OpenIddictConstants.GrantTypes.Implicit)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Implicit);
            }

            if (grantType == OpenIddictConstants.GrantTypes.Password)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Password);
            }

            if (grantType == OpenIddictConstants.GrantTypes.RefreshToken)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
            }

            if (grantType == OpenIddictConstants.GrantTypes.DeviceCode)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.DeviceCode);
                application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.DeviceAuthorization);
            }

            if (grantType == OpenIddictConstants.GrantTypes.Implicit)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.IdToken);
                if (string.Equals(type, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
                {
                    application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken);
                    application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Token);
                }
            }

            if (!buildInGrantTypes.Contains(grantType))
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.GrantType + grantType);
            }
        }

        var buildInScopes = new[] {
            OpenIddictConstants.Permissions.Scopes.Address, OpenIddictConstants.Permissions.Scopes.Email,
            OpenIddictConstants.Permissions.Scopes.Phone, OpenIddictConstants.Permissions.Scopes.Profile,
            OpenIddictConstants.Permissions.Scopes.Roles
        };

        foreach (var scope in scopes)
        {
            if (buildInScopes.Contains(scope))
            {
                application.Permissions.Add(scope);
            }
            else
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
            }
        }

		// PKCE 要求
		if (requirePkce)
		{
			application.Requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);
		}

		if (!redirectUris.IsNullOrEmpty())
        {
            foreach (var redirectUri in redirectUris!.Where(redirectUri => !redirectUri.IsNullOrWhiteSpace()))
            {
                if (!Uri.TryCreate(redirectUri, UriKind.Absolute, out var uri) || !uri.IsWellFormedOriginalString())
                {
                    throw new BusinessException(L["InvalidRedirectUri", redirectUri]);
                }

                if (application.RedirectUris.All(x => x != uri))
                {
                    application.RedirectUris.Add(uri);
                }
            }
            
        }
        
        if (!postLogoutRedirectUris.IsNullOrEmpty())
        {
            foreach (var postLogoutRedirectUri in postLogoutRedirectUris!.Where(postLogoutRedirectUri => !postLogoutRedirectUri.IsNullOrWhiteSpace()))
            {
                if (!Uri.TryCreate(postLogoutRedirectUri, UriKind.Absolute, out var uri) ||
                    !uri.IsWellFormedOriginalString())
                {
                    throw new BusinessException(L["InvalidPostLogoutRedirectUri", postLogoutRedirectUri]);
                }

                if (application.PostLogoutRedirectUris.All(x => x != uri))
                {
                    application.PostLogoutRedirectUris.Add(uri);
                }
            }
        }

        if (permissions != null)
        {
            await _permissionDataSeeder.SeedAsync(
                ClientPermissionValueProvider.ProviderName,
                name,
                permissions,
                null
            );
        }

        if (client == null)
        {
            await _applicationManager.CreateAsync(application);
            return;
        }

        if (!HasSameRedirectUris(client, application))
        {
            client.RedirectUris = JsonSerializer.Serialize(application.RedirectUris.Select(q => q.ToString().RemovePostFix("/")));
            client.PostLogoutRedirectUris = JsonSerializer.Serialize(application.PostLogoutRedirectUris.Select(q => q.ToString().RemovePostFix("/")));

            await _applicationManager.UpdateAsync(client.ToModel());
        }

        if (!HasSameScopes(client, application))
        {
            client.Permissions = JsonSerializer.Serialize(application.Permissions.Select(q => q.ToString()));
            await _applicationManager.UpdateAsync(client.ToModel());
        }
    }

    private static bool HasSameRedirectUris(OpenIddictApplication existingClient, AbpApplicationDescriptor application)
    {
        return existingClient.RedirectUris == JsonSerializer.Serialize(application.RedirectUris.Select(q => q.ToString().RemovePostFix("/")));
    }

    private static bool HasSameScopes(OpenIddictApplication existingClient, AbpApplicationDescriptor application)
    {
        return existingClient.Permissions == JsonSerializer.Serialize(application.Permissions.Select(q => q.ToString().TrimEnd('/')));
    }
}
