namespace XD.Pms.ApiKeys.Dto;

public class RegenerateApiKeyOutput
{
	/// <summary>
	/// 新的 API Key 明文（仅返回一次）
	/// </summary>
	public string ApiKey { get; set; } = default!;

	/// <summary>
	/// Key 前缀
	/// </summary>
	public string KeyPrefix { get; set; } = default!;

	/// <summary>
	/// 警告消息
	/// </summary>
	public string Warning { get; set; } = "旧的 API Key 已失效，请使用新的 Key！";
}