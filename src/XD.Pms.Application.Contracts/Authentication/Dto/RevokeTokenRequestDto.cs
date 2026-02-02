using System.ComponentModel.DataAnnotations;

namespace XD.Pms.Authentication.Dto;

public class RevokeTokenRequestDto
{
	/// <summary>
	/// 访问令牌
	/// </summary>
	[Required]
	public required string AccessToken { get; set; }
}
