using System;

namespace XD.Pms.Authentication;

public class JwtTokenResult
{
	public string AccessToken { get; set; } = default!;
	public string RefreshToken { get; set; } = default!;
	public DateTime AccessTokenExpiration { get; set; }
	public DateTime RefreshTokenExpiration { get; set; }
	public string TokenType { get; set; } = "Bearer";
}
