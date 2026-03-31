using Riok.Mapperly.Abstractions;
using System;
using System.Linq;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using XD.Pms.BaseData.Employees;
using XD.Pms.BaseData.Employees.Dto;

namespace XD.Pms.BaseData;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class EmployeeMapper : MapperBase<Employee, EmployeeDto>
{
	private readonly IIdentityUserRepository _userRepository;
	public EmployeeMapper(IIdentityUserRepository userRepository)
	{
		_userRepository = userRepository;
	}
	public override partial EmployeeDto Map(Employee source);
	public override partial void Map(Employee source, EmployeeDto destination);
	public override void AfterMap(Employee source, EmployeeDto destination)
	{
		var primaryPost = destination.Positions.FirstOrDefault(position => position.IsPrimary);
		destination.PrimaryDepartmentName = primaryPost?.DepartmentName;
		destination.PrimaryPositionName = primaryPost?.PositionName;
		destination.CreatorName = GetUserName(source.CreatorId);
		destination.LastModifierName = GetUserName(source.LastModifierId);
		destination.ApproverName = GetUserName(source.ApproverId);
	}
	private string? GetUserName(Guid? userId)
	{
		return userId.HasValue ? _userRepository.GetAsync(userId.Value).GetAwaiter().GetResult().UserName : null;
	}
}
