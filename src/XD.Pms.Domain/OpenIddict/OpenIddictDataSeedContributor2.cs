using OpenIddict.Abstractions;
using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace XD.Pms.OpenIddict;

public class OpenIddictDataSeedContributor2 : IDataSeedContributor//, ITransientDependency
{
	private readonly IOpenIddictApplicationManager _applicationManager;
	private readonly IOpenIddictScopeManager _scopeManager;

	public OpenIddictDataSeedContributor2(
		IOpenIddictApplicationManager applicationManager,
		IOpenIddictScopeManager scopeManager)
	{
		_applicationManager = applicationManager;
		_scopeManager = scopeManager;
	}

	public async Task SeedAsync(DataSeedContext context)
	{
		// 1. 创建 Soybean Admin 前端应用（Password Grant）
		await CreateApplicationAsync(
			name: "Pms_Web",
			displayName: "Soybean Admin Front",
			type: OpenIddictConstants.ClientTypes.Public,
			consentType: OpenIddictConstants.ConsentTypes.Implicit,
			scopes: ["openid", "profile", "email", "roles", "YourProject"],
			grantTypes:
			[
				OpenIddictConstants.GrantTypes.Password,
				OpenIddictConstants.GrantTypes.RefreshToken
			],
			redirectUris: ["http://localhost:3000/callback"]
		);

		// 2. 创建移动端应用（Password Grant）
		await CreateApplicationAsync(
			name: "YourProject_Mobile",
			displayName: "Uniapp",
			type: OpenIddictConstants.ClientTypes.Public,
			consentType: OpenIddictConstants.ConsentTypes.Implicit,
			scopes: ["openid", "profile", "email", "roles", "YourProject"],
			grantTypes:
			[
				OpenIddictConstants.GrantTypes.Password,
				OpenIddictConstants.GrantTypes.RefreshToken
			]
		);

		// 3. 创建服务间调用应用（Client Credentials）
		await CreateApplicationAsync(
			name: "YourProject_Service",
			displayName: "内部服务",
			type: OpenIddictConstants.ClientTypes.Confidential,
			secret: "YourServiceSecret123!",
			consentType: OpenIddictConstants.ConsentTypes.Implicit,
			scopes: ["YourProject"],
			grantTypes: [OpenIddictConstants.GrantTypes.ClientCredentials]
		);

		// 创建 API 作用域
		await CreateScopeAsync("YourProject", "YourProject API");
	}

	private async Task CreateApplicationAsync(
		string name,
		string displayName,
		string type,
		string consentType,
		string[] scopes,
		string[] grantTypes,
		string? secret = null,
		string[]? redirectUris = null)
	{
		var client = await _applicationManager.FindByClientIdAsync(name);

		if (client == null)
		{
			var descriptor = new OpenIddictApplicationDescriptor
			{
				ClientId = name,
				DisplayName = displayName,
				ClientType = type,
				ConsentType = consentType
			};

			if (!string.IsNullOrEmpty(secret))
			{
				descriptor.ClientSecret = secret;
			}

			foreach (var scope in scopes)
			{
				descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
			}

			foreach (var grantType in grantTypes)
			{
				descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.GrantType + grantType);
			}

			if (redirectUris != null)
			{
				foreach (var uri in redirectUris)
				{
					descriptor.RedirectUris.Add(new Uri(uri));
				}
			}

			// 基础权限
			descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
			descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Revocation);

			await _applicationManager.CreateAsync(descriptor);
		}
	}

	private async Task CreateScopeAsync(string name, string displayName)
	{
		var scope = await _scopeManager.FindByNameAsync(name);

		if (scope == null)
		{
			await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor
			{
				Name = name,
				DisplayName = displayName,
				Resources = { name }
			});
		}
	}
}