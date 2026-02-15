using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;
using Volo.Abp.DependencyInjection;
using XD.Pms.ApiResponse;

namespace XD.Pms.Authentication;

/// <summary>
/// RSA 加密服务
/// </summary>
public class CryptoService : ICryptoService, ISingletonDependency
{
	private readonly RSA _rsa;
	private readonly string _publicKey;
	private readonly IMemoryCache _cache;
	private readonly int _keyRotationMinutes;

	private const string RSA_KEY_CACHE_KEY = "PMS_RSA_KEY_PAIR";

	public CryptoService(IConfiguration configuration, IMemoryCache cache)
	{
		_cache = cache;
		_keyRotationMinutes = configuration.GetValue("Security:RsaKeyRotationMinutes", 60);

		// 获取或创建 RSA 密钥对
		var (Rsa, PublicKey) = GetOrCreateKeyPair();
		_rsa = Rsa;
		_publicKey = PublicKey;
	}

	/// <summary>
	/// 获取公钥
	/// </summary>
	public string GetPublicKey()
	{
		return _publicKey;
	}

	/// <summary>
	/// RSA 解密
	/// </summary>
	public string Decrypt(string encryptedText)
	{
		if (string.IsNullOrEmpty(encryptedText))
		{
			throw new PmsBusinessException(ApiResponseCode.BadRequest, "用于解密的加密文本不能为空");
		}

		try
		{
			var encryptedBytes = Convert.FromBase64String(encryptedText);
			var decryptedBytes = _rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
			return Encoding.UTF8.GetString(decryptedBytes);
		}
		catch (FormatException)
		{
			throw new PmsBusinessException(ApiResponseCode.ValidationError, "密码解密失败：无效的加密格式");
		}
		catch (CryptographicException)
		{
			throw new PmsBusinessException(ApiResponseCode.ValidationError, "密码解密失败：解密错误");
		}
	}

	private (RSA Rsa, string PublicKey) GetOrCreateKeyPair()
	{
		return _cache.GetOrCreate(RSA_KEY_CACHE_KEY, entry =>
		{
			entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_keyRotationMinutes);

			var rsa = RSA.Create(2048);
			var publicKey = ExportPublicKeyPem(rsa);

			return (rsa, publicKey);
		})!;
	}

	private static string ExportPublicKeyPem(RSA rsa)
	{
		var publicKeyBytes = rsa.ExportSubjectPublicKeyInfo();
		var base64 = Convert.ToBase64String(publicKeyBytes);

		var sb = new StringBuilder();
		sb.AppendLine("-----BEGIN PUBLIC KEY-----");

		// 每 64 个字符换行
		for (int i = 0; i < base64.Length; i += 64)
		{
			sb.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));
		}

		sb.AppendLine("-----END PUBLIC KEY-----");

		return sb.ToString();
	}
}