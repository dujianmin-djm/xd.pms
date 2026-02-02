using System;
using System.Collections.Generic;

namespace XD.Pms.ApiKeys;

/// <summary>
/// API Key 验证结果
/// </summary>
public class ApiKeyValidationResult
{
	/// <summary>
	/// 是否有效
	/// </summary>
	public bool IsValid { get; set; }

	/// <summary>
	/// 失败消息
	/// </summary>
	public string? FailureMessage { get; set; }

	/// <summary>
	/// 错误码
	/// </summary>
	public string? ErrorCode { get; set; }

	/// <summary>
	/// API Key ID
	/// </summary>
	public Guid? ApiKeyId { get; set; }

	/// <summary>
	/// 客户端 ID
	/// </summary>
	public string? ClientId { get; set; }

	/// <summary>
	/// 客户端名称
	/// </summary>
	public string? ClientName { get; set; }

	/// <summary>
	/// 关联用户 ID
	/// </summary>
	public Guid? UserId { get; set; }

	/// <summary>
	/// 角色列表
	/// </summary>
	public List<string>? Roles { get; set; }

	/// <summary>
	/// 权限列表
	/// </summary>
	public List<string>? Permissions { get; set; }

	public static ApiKeyValidationResult Success(
		Guid apiKeyId,
		string clientId,
		string clientName,
		Guid? userId = null,
		List<string>? roles = null,
		List<string>? permissions = null)
	{
		return new ApiKeyValidationResult
		{
			IsValid = true,
			ApiKeyId = apiKeyId,
			ClientId = clientId,
			ClientName = clientName,
			UserId = userId,
			Roles = roles ?? [],
			Permissions = permissions ?? []
		};
	}

	public static ApiKeyValidationResult Fail(string errorCode, string message)
	{
		return new ApiKeyValidationResult
		{
			IsValid = false,
			ErrorCode = errorCode,
			FailureMessage = message
		};
	}
}