using System;
using System.Collections.Generic;

namespace XD.Pms.Authentication.Dto;

public class UserInfoDto
{
	public Guid Id { get; set; }
	public string UserName { get; set; } = default!;
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
	public List<string> Roles { get; set; } = [];
	public List<string> Permissions { get; set; } = [];
}