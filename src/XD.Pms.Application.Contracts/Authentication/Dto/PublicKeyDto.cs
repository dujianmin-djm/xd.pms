namespace XD.Pms.Authentication.Dto;

public class PublicKeyDto
{
	/// <summary>
	/// RSA 公钥（PEM 格式）
	/// </summary>
	public string PublicKey { get; set; } = default!;

	/// <summary>
	/// 密钥过期时间（Unix 时间戳，毫秒）
	/// </summary>
	public long ExpiresAt { get; set; }
}