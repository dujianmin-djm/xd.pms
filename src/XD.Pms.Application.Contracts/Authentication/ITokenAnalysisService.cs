using System.Threading.Tasks;

namespace XD.Pms.Authentication;

public interface ITokenAnalysisService
{
	/// <summary>
	/// 分析 Token 认证失败的具体原因
	/// </summary>
	Task<TokenAnalysisResult> AnalyzeTokenErrorAsync(string? token, string? wwwAuthenticate = null);
}