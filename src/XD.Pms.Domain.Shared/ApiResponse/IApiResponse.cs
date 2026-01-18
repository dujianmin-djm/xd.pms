namespace XD.Pms.ApiResponse;

public interface IApiResponse
{
	bool Success { get; set; }
	string Code { get; set; }
	string Message { get; set; }
}