using Volo.Abp;

namespace XD.Pms.Authentication;

/// <summary>
/// 认证异常（携带业务错误码）
/// </summary>
public class AuthenticationException : BusinessException
{
	public string ErrorCode { get; }

	public AuthenticationException(string code, string message) : base(message: message)
	{
		ErrorCode = code;
	}
}