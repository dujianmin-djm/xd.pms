namespace XD.Pms.ApiKeys.Dto;

public class CreateApiKeyOutput
{
	/// <summary>
	/// API Key 明文（仅创建时返回一次，请妥善保存）
	/// </summary>
	public string ApiKey { get; set; } = default!;

	/// <summary>
	/// API Key 信息
	/// </summary>
	public ApiKeyDto KeyInfo { get; set; } = default!;

	/// <summary>
	/// 警告消息
	/// </summary>
	public string Warning { get; set; } = "请妥善保存此 API Key，它只会显示一次！";
}