using Riok.Mapperly.Abstractions;
using System;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using XD.Pms.BaseData.Departments;
using XD.Pms.BaseData.Departments.Dto;

namespace XD.Pms.BaseData;

[Mapper]
public partial class DepartmentMapper : MapperBase<Department, DepartmentDto>
{
	private readonly IDepartmentRepository _deptRepository;
	private readonly IIdentityUserRepository _userRepository;

	public DepartmentMapper(IDepartmentRepository deptRepository, IIdentityUserRepository userRepository)
	{
		_deptRepository = deptRepository;
		_userRepository = userRepository;
	}

	[MapperIgnoreSource(nameof(Department.ExtraProperties))]
	[MapperIgnoreTarget(nameof(DepartmentDto.CreatorName))]
	[MapperIgnoreTarget(nameof(DepartmentDto.LastModifierName))]
	[MapperIgnoreTarget(nameof(DepartmentDto.ApproverName))]
	public override partial DepartmentDto Map(Department source);

	[MapperIgnoreSource(nameof(Department.ExtraProperties))]
	[MapperIgnoreTarget(nameof(DepartmentDto.CreatorName))]
	[MapperIgnoreTarget(nameof(DepartmentDto.LastModifierName))]
	[MapperIgnoreTarget(nameof(DepartmentDto.ApproverName))]
	public override partial void Map(Department source, DepartmentDto destination);

	public override void AfterMap(Department source, DepartmentDto target)
	{
		target.ParentName = source.ParentId.HasValue
			? _deptRepository.GetAsync(source.ParentId.Value).GetAwaiter().GetResult().Name
			: null;
		target.CreatorName = GetUserName(source.CreatorId);
		target.LastModifierName = GetUserName(source.LastModifierId);
		target.ApproverName = GetUserName(source.ApproverId);
	}

	private string? GetUserName(Guid? userId)
	{
		return userId.HasValue 
			? _userRepository.GetAsync(userId.Value).GetAwaiter().GetResult().UserName
			: null;
	}
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class DepartmentToDepartmentLookupDtoMapper : MapperBase<Department, DepartmentLookupDto>
{
	public override partial DepartmentLookupDto Map(Department source);
	public override partial void Map(Department source, DepartmentLookupDto destination);
}