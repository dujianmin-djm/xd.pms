using Riok.Mapperly.Abstractions;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using XD.Pms.Identity.Role.Dto;

namespace XD.Pms.Identity;

[Mapper]
public partial class RoleToRoleDtoMapper : MapperBase<IdentityRole, RoleDto>
{
	[MapperIgnoreTarget(nameof(RoleDto.Number))]
	[MapperIgnoreTarget(nameof(RoleDto.Description))]
	[MapperIgnoreTarget(nameof(RoleDto.IsActive))]
	[MapperIgnoreSource(nameof(IdentityRole.ExtraProperties))]
	[MapperIgnoreSource(nameof(IdentityRole.NormalizedName))]
	[MapperIgnoreSource(nameof(IdentityRole.TenantId))]
	[MapperIgnoreSource(nameof(IdentityRole.Claims))]
	[MapperIgnoreSource(nameof(IdentityRole.EntityVersion))]
	public override partial RoleDto Map(IdentityRole source);

	[MapperIgnoreTarget(nameof(RoleDto.Number))]
	[MapperIgnoreTarget(nameof(RoleDto.Description))]
	[MapperIgnoreTarget(nameof(RoleDto.IsActive))]
	[MapperIgnoreSource(nameof(IdentityRole.ExtraProperties))]
	[MapperIgnoreSource(nameof(IdentityRole.NormalizedName))]
	[MapperIgnoreSource(nameof(IdentityRole.TenantId))]
	[MapperIgnoreSource(nameof(IdentityRole.Claims))]
	[MapperIgnoreSource(nameof(IdentityRole.EntityVersion))]
	public override partial void Map(IdentityRole source, RoleDto destination);

	public override void AfterMap(IdentityRole source, RoleDto target)
	{
		target.Number = source.GetProperty<string>("Number") ?? string.Empty;
		target.Description = source.GetProperty<string>("Description") ?? string.Empty;
		target.IsActive = source.GetProperty<bool>("IsActive");
	}
}