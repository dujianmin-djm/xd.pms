namespace XD.Pms.Authentication;

/// <summary>
/// 加密服务接口
/// </summary>
public interface ICryptoService
{
	/// <summary>
	/// 获取 RSA(非对称加密) 公钥（PEM 格式）
	/// </summary>
	string GetPublicKey();

	/// <summary>
	/// RSA 解密
	/// </summary>
	/// <param name="encryptedText">Base64 编码的密文</param>
	/// <returns>解密后的明文</returns>
	string Decrypt(string encryptedText);
}