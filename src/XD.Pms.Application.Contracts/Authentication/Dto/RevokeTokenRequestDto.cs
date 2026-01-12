namespace XD.Pms.Authentication.Dto;

public class RevokeTokenRequestDto
{
	/// <summary>
	/// 要撤销的刷新令牌（为空则撤销当前令牌）
	/// </summary>
	public string? RefreshToken { get; set; }

	/// <summary>
	/// 是否撤销所有设备的令牌
	/// </summary>
	public bool RevokeAll { get; set; }
}
