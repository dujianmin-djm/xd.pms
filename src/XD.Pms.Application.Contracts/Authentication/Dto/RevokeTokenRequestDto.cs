using System.ComponentModel.DataAnnotations;

namespace XD.Pms.Authentication.Dto;

public class RevokeTokenRequestDto
{
	/// <summary>
	/// 访问令牌
	/// </summary>
	[Required(ErrorMessage = "访问令牌不能为空")]
	public required string AccessToken { get; set; }
}
