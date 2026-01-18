using System.Text.Json.Serialization;

namespace XD.Pms.ApiResponse;

/// <summary>
/// 统一 API 响应模型
/// </summary>
public class ApiResponse<T> : IApiResponse
{
	/// <summary>
	/// 状态码
	/// </summary>
	[JsonPropertyName("code")]
	public string Code { get; set; } = ApiResponseCode.Success;

	/// <summary>
	/// 操作是否成功
	/// </summary>
	[JsonPropertyName("success")]
	public bool Success { get; set; }

	/// <summary>
	/// 响应数据
	/// </summary>
	[JsonPropertyName("data")]
	public T? Data { get; set; }

	/// <summary>
	/// 提示消息
	/// </summary>
	[JsonPropertyName("message")]
	public string Message { get; set; } = string.Empty;

	public ApiResponse(string code, bool success, T? data, string message = "")
	{
		Code = code;
		Success = success;
		Data = data;
		Message = message;
	}

	/// <summary>
	/// 成功响应
	/// </summary>
	public static ApiResponse<T> Succeed(bool isSuccess, T? data, string message = "")
	{
		return new ApiResponse<T>(ApiResponseCode.Success, isSuccess, data, message);
	}

	/// <summary>
	/// 失败响应
	/// </summary>
	public static ApiResponse<object> Fail(string code, string message)
	{
		return new ApiResponse<object>(code, false, default, message);
	}
}


/// <summary>
/// 无数据响应
/// </summary>
public class ApiResponse : ApiResponse<object>
{
	public ApiResponse() : base(ApiResponseCode.Success, true, null, "") { }

	public ApiResponse(string code, bool success, object? data, string message)
		: base(code, success, data, message) { }

	public new static ApiResponse Succeed(bool isSuccess, object? data, string message = "")
	{
		return new ApiResponse(ApiResponseCode.Success, isSuccess, data, message);
	}

	public new static ApiResponse Fail(string code, string message)
	{
		return new ApiResponse(code, false, null, message);
	}
}