using System.Threading.Tasks;

namespace XD.Pms.ApiKeys;

/// <summary>
/// API Key 验证器接口
/// </summary>
public interface IApiKeyValidator
{
	/// <summary>
	/// 验证 API Key
	/// </summary>
	/// <param name="apiKey">明文 API Key</param>
	/// <param name="ipAddress">请求 IP 地址</param>
	Task<ApiKeyValidationResult> ValidateAsync(string apiKey, string? ipAddress = null);
}