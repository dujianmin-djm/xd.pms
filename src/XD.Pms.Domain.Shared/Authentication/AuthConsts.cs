namespace XD.Pms.Authentication;

public static class AuthConsts
{
	/// <summary>
	/// API 资源/Scope 名称
	/// </summary>
	public const string ApiScope = "Pms";

	/// <summary>
	/// 客户端标识
	/// </summary>
	public static class Clients
	{
		/// <summary>
		/// Vue 前端应用（Soybean Admin）
		/// </summary>
		public const string VueApp = "Pms_App";

		/// <summary>
		/// 移动端应用
		/// </summary>
		public const string MobileApp = "Pms_Mobile";

		/// <summary>
		/// Swagger 测试
		/// </summary>
		public const string Swagger = "Pms_Swagger";
	}
}
