using System;

namespace XD.Pms.Authentication.Dto;

public class LoginResponseDto
{
	/// <summary>
	/// 访问令牌
	/// </summary>
	public string AccessToken { get; set; } = default!;

	/// <summary>
	/// 刷新令牌
	/// </summary>
	public string RefreshToken { get; set; } = default!;

	/// <summary>
	/// 令牌类型
	/// </summary>
	public string TokenType { get; set; } = "Bearer";

	/// <summary>
	/// 访问令牌过期时间（秒）
	/// </summary>
	public int ExpiresIn { get; set; }

	/// <summary>
	/// 访问令牌过期时间点
	/// </summary>
	public DateTime AccessTokenExpiration { get; set; }

	/// <summary>
	/// 刷新令牌过期时间点
	/// </summary>
	public DateTime RefreshTokenExpiration { get; set; }

	/// <summary>
	/// 用户信息
	/// </summary>
	public UserInfoDto User { get; set; } = default!;
}

public class UserInfoDto
{
	public Guid Id { get; set; }
	public string UserName { get; set; } = default!;
	public string? Email { get; set; }
	public string? Name { get; set; }
	public string? Surname { get; set; }
	public string? PhoneNumber { get; set; }
	public string[] Roles { get; set; } = [];
}
