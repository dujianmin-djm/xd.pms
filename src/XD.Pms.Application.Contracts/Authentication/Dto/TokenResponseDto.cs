using System;

namespace XD.Pms.Authentication.Dto;

public class TokenResponseDto
{
	public string AccessToken { get; set; } = default!;
	public string RefreshToken { get; set; } = default!;
	public string TokenType { get; set; } = "Bearer";
	public int ExpiresIn { get; set; }
	public DateTime AccessTokenExpiration { get; set; }
	public DateTime RefreshTokenExpiration { get; set; }
}
