using Microsoft.AspNetCore.Authentication;
using XD.Pms.ApiKeys;

namespace XD.Pms.Authentication.ApiKey;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
	public const string DefaultScheme = "ApiKey";

	/// <summary>
	/// 请求头名称
	/// </summary>
	public string HeaderName { get; set; } = ApiKeyConsts.DefaultHeaderName;

	/// <summary>
	/// 认证域
	/// </summary>
	public string Realm { get; set; } = "Pms API";

	/// <summary>
	/// 是否启用
	/// </summary>
	public bool Enabled { get; set; } = true;
}