using System.ComponentModel.DataAnnotations;

namespace XD.Pms.Authentication.Dto;

public class RefreshTokenRequestDto
{
	/// <summary>
	/// 刷新令牌
	/// </summary>
	[Required(ErrorMessage = "刷新令牌不能为空")]
	public string RefreshToken { get; set; } = default!;
}
