using Volo.Abp.Identity;

namespace XD.Pms;

public static class PmsConsts
{
    public const string AdminEmailDefaultValue = IdentityDataSeedContributor.AdminEmailDefaultValue;
    public const string AdminPasswordDefaultValue = IdentityDataSeedContributor.AdminPasswordDefaultValue;

	public const string? DbSchema = null;
	public const string DefaultDbTablePrefix = "T_";

	public static class DbTablePrefix
	{
		public const string Oidc = DefaultDbTablePrefix + "OIDC_";
		public const string System = DefaultDbTablePrefix + "SYS_";
		public const string BaseData = DefaultDbTablePrefix + "BD_";
		public const string Business = DefaultDbTablePrefix + "BIZ_";
	}
}
