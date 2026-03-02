using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using XD.Pms.BaseData.Departments;
using XD.Pms.Enums;

namespace XD.Pms.EntityFrameworkCore.EntityTypeConfigurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
	public void Configure(EntityTypeBuilder<Department> b)
	{
		b.ToTable(PmsConsts.DbTablePrefix.BaseData + "Department", PmsConsts.DbSchema);
		b.ConfigureByConvention();

		b.Property(x => x.Number).IsRequired().HasMaxLength(50);
		b.Property(x => x.Name).IsRequired().HasMaxLength(100);
		b.Property(x => x.Description).HasMaxLength(256);
		b.Property(x => x.FullName).IsRequired().HasMaxLength(512);
		b.Property(x => x.DocumentStatus).HasDefaultValue(DocumentStatus.Created);
		b.HasIndex(x => x.Number).IsUnique();
		b.HasIndex(x => x.ParentId);

		b.HasOne(x => x.Parent)
			.WithMany()
			.HasForeignKey(x => x.ParentId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}