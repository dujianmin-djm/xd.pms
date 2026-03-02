using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using XD.Pms.BaseData.Positions;
using XD.Pms.Enums;

namespace XD.Pms.EntityFrameworkCore.EntityTypeConfigurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
	public void Configure(EntityTypeBuilder<Position> b)
	{
		b.ToTable(PmsConsts.DbTablePrefix.BaseData + "Position", PmsConsts.DbSchema);
		b.ConfigureByConvention();

		b.Property(x => x.Number).IsRequired().HasMaxLength(50);
		b.Property(x => x.Name).IsRequired().HasMaxLength(100);
		b.Property(x => x.Description).HasMaxLength(256);
		b.Property(x => x.DocumentStatus).HasDefaultValue(DocumentStatus.Created);
		b.HasIndex(x => x.Number).IsUnique();
		b.HasIndex(x => x.DepartmentId);

		b.HasOne(x => x.Department)
			.WithMany()
			.HasForeignKey(x => x.DepartmentId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}