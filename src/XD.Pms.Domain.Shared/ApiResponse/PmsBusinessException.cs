using Microsoft.Extensions.Logging;
using System;
using Volo.Abp;

namespace XD.Pms.ApiResponse;

public class PmsBusinessException : BusinessException
{
	public string ErrorCode { get; }
	public PmsBusinessException(
		string code,
		string message,
		string? details = null, 
		Exception? innerException = null,
		LogLevel logLevel = LogLevel.Error)
		: base(message: message, details: details, innerException: innerException, logLevel: logLevel)
	{
		ErrorCode = code;
	}
}
