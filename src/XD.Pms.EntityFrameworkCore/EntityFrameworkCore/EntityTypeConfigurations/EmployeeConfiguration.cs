using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;
using XD.Pms.BaseData.Employees;
using XD.Pms.Enums;

namespace XD.Pms.EntityFrameworkCore.EntityTypeConfigurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
	public void Configure(EntityTypeBuilder<Employee> b)
	{
		b.ToTable(PmsConsts.DbTablePrefix.BaseData + "Employee", PmsConsts.DbSchema);
		b.ConfigureByConvention();

		b.Property(x => x.Number).IsRequired().HasMaxLength(50);
		b.Property(x => x.Name).IsRequired().HasMaxLength(100);
		b.Property(x => x.Description).HasMaxLength(256);
		b.Property(x => x.Phone).HasMaxLength(50);
		b.Property(x => x.Email).HasMaxLength(50);
		b.Property(x => x.Address).HasMaxLength(256);
		b.Property(x => x.DocumentStatus).HasDefaultValue(DocumentStatus.Created);
		b.HasIndex(x => x.Number).IsUnique();
		b.HasMany(x => x.Positions)
			.WithOne()
			.HasForeignKey(x => x.EmployeeId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}

public class EmployeePositionConfiguration : IEntityTypeConfiguration<EmployeePosition>
{
	public void Configure(EntityTypeBuilder<EmployeePosition> b)
	{
		b.ToTable(PmsConsts.DbTablePrefix.BaseData + "EmployeePosition", PmsConsts.DbSchema);
		b.ConfigureByConvention();

		b.HasIndex(x => new { x.EmployeeId, x.DepartmentId, x.PositionId });

		b.HasOne(x => x.Department)
			.WithMany()
			.HasForeignKey(x => x.DepartmentId)
			.OnDelete(DeleteBehavior.Restrict);

		b.HasOne(x => x.Position)
			.WithMany()
			.HasForeignKey(x => x.PositionId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
