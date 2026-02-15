using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using XD.Pms.ApiKeys;

namespace XD.Pms.EntityFrameworkCore.EntityTypeConfigurations;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
	public void Configure(EntityTypeBuilder<ApiKey> builder)
	{
		builder.ToTable(PmsConsts.DbTablePrefix.System + "ApiKeys", PmsConsts.DbSchema);

		builder.ConfigureByConvention();

		builder.Property(x => x.KeyHash)
			.IsRequired()
			.HasMaxLength(ApiKeyConsts.KeyHashLength);

		builder.Property(x => x.KeyPrefix)
			.IsRequired()
			.HasMaxLength(ApiKeyConsts.MaxKeyPrefixLength);

		builder.Property(x => x.ClientId)
			.IsRequired()
			.HasMaxLength(ApiKeyConsts.MaxClientIdLength);

		builder.Property(x => x.ClientName)
			.IsRequired()
			.HasMaxLength(ApiKeyConsts.MaxClientNameLength);

		builder.Property(x => x.Description)
			.HasMaxLength(ApiKeyConsts.MaxDescriptionLength);

		builder.Property(x => x.Roles)
			.HasMaxLength(ApiKeyConsts.MaxRolesLength);

		builder.Property(x => x.Permissions)
			.HasMaxLength(ApiKeyConsts.MaxPermissionsLength);

		builder.Property(x => x.AllowedIpAddresses)
			.HasMaxLength(ApiKeyConsts.MaxAllowedIpAddressesLength);

		builder.Property(x => x.LastUsedIp)
			.HasMaxLength(50);

		builder.HasIndex(x => x.KeyHash).IsUnique();
		builder.HasIndex(x => x.ClientId).IsUnique();
		builder.HasIndex(x => x.IsActive);
		builder.HasIndex(x => x.UserId);
	}
}