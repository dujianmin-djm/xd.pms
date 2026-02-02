namespace XD.Pms.ApiKeys;

public static class ApiKeyErrorCodes
{
	public const string InvalidApiKey = "Pms:ApiKey:001";
	public const string ApiKeyExpired = "Pms:ApiKey:002";
	public const string ApiKeyDisabled = "Pms:ApiKey:003";
	public const string ApiKeyNotFound = "Pms:ApiKey:004";
	public const string IpAddressNotAllowed = "Pms:ApiKey:005";
	public const string RateLimitExceeded = "Pms:ApiKey:006";
	public const string DuplicateClientId = "Pms:ApiKey:007";
}