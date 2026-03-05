using Riok.Mapperly.Abstractions;
using System;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using XD.Pms.BaseData.Departments;
using XD.Pms.BaseData.Positions;
using XD.Pms.BaseData.Positions.Dto;

namespace XD.Pms.BaseData;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class PositionMapper : MapperBase<Position, PositionDto>
{
	private readonly IDepartmentRepository _deptRepository;
	private readonly IIdentityUserRepository _userRepository;
	public PositionMapper(IDepartmentRepository deptRepository, IIdentityUserRepository userRepository)
	{
		_deptRepository = deptRepository;
		_userRepository = userRepository;
	}
	public override partial PositionDto Map(Position source);
	public override partial void Map(Position source, PositionDto target);
	public override void AfterMap(Position source, PositionDto target)
	{
		target.DepartmentName = _deptRepository.GetAsync(source.DepartmentId).GetAwaiter().GetResult().Name;
		target.DepartmentFullName = _deptRepository.GetAsync(source.DepartmentId).GetAwaiter().GetResult().FullName;
		target.CreatorName = GetUserName(source.CreatorId);
		target.LastModifierName = GetUserName(source.LastModifierId);
		target.ApproverName = GetUserName(source.ApproverId);
	}

	private string? GetUserName(Guid? userId)
	{
		return userId.HasValue ? _userRepository.GetAsync(userId.Value).GetAwaiter().GetResult().UserName : null;
	}
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class PositionToPositionLookupDtoMapper : MapperBase<Position, PositionLookupDto>
{
	private readonly IDepartmentRepository _deptRepository;
	public PositionToPositionLookupDtoMapper(IDepartmentRepository deptRepository) 
	{
		_deptRepository = deptRepository;
	}
	public override partial PositionLookupDto Map(Position source);
	public override partial void Map(Position source, PositionLookupDto destination);
	public override void AfterMap(Position source, PositionLookupDto destination)
	{
		destination.DepartmentName = _deptRepository.GetAsync(source.DepartmentId).GetAwaiter().GetResult().Name;
		destination.DepartmentFullName = _deptRepository.GetAsync(source.DepartmentId).GetAwaiter().GetResult().FullName;
	}
}