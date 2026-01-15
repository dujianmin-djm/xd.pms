using System.Text.Json.Serialization;

namespace XD.Pms.ApiResponse;

/// <summary>
/// 统一 API 响应模型
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class ApiResponse<T>
{
	/// <summary>
	/// 状态码
	/// </summary>
	[JsonPropertyName("code")]
	public string Code { get; set; } = ApiResponseCode.Success;

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

	public ApiResponse() { }

	public ApiResponse(T? data, string code = ApiResponseCode.Success, string message = "")
	{
		Code = code;
		Data = data;
		Message = message;
	}

	/// <summary>
	/// 成功响应
	/// </summary>
	public static ApiResponse<T> Success(T? data, string message = "操作成功")
	{
		return new ApiResponse<T>
		{
			Code = ApiResponseCode.Success,
			Data = data,
			Message = message
		};
	}

	/// <summary>
	/// 失败响应
	/// </summary>
	public static ApiResponse<T> Fail(string code, string message)
	{
		return new ApiResponse<T>
		{
			Code = code,
			Data = default,
			Message = message
		};
	}
}

/// <summary>
/// 无数据响应
/// </summary>
public class ApiResponse : ApiResponse<object>
{
	public static ApiResponse Success(string message = "操作成功")
	{
		return new ApiResponse
		{
			Code = ApiResponseCode.Success,
			Data = null,
			Message = message
		};
	}

	public new static ApiResponse Fail(string code, string message)
	{
		return new ApiResponse
		{
			Code = code,
			Data = null,
			Message = message
		};
	}
}