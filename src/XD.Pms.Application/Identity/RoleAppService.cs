using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using XD.Pms.ApiResponse;
using XD.Pms.Identity.Role;
using XD.Pms.Identity.Role.Dto;
using XD.Pms.Permissions;
using XD.Pms.Services.Dtos;

namespace XD.Pms.Identity;

[Route("papi/role")]
[Authorize(PmsPermissions.System.Roles.Default)]
public class RoleAppService : PmsAppService, IRoleAppService
{
	readonly IRoleRepository _roleRepository;
	readonly IdentityRoleManager _roleManager;
	public RoleAppService(IdentityRoleManager roleManager, IRoleRepository roleRepository) 
	{
		_roleRepository = roleRepository;
		_roleManager = roleManager;
	}

	[HttpGet("{id}")]
	public async Task<RoleDto> GetAsync(Guid id)
	{
		var role = await _roleRepository.GetAsync(id);
		return ObjectMapper.Map<IdentityRole, RoleDto>(role);
	}

	[HttpGet("query")]
	public async Task<PagedResponseDto<RoleDto>> GetListAsync(RoleReadDto input)
	{
		int skpCount = (input.Current - 1) * input.Size;
		var list = await _roleRepository.GetListAsync(input.Number, input.Name, input.IsActive, input.Sorts, skpCount, input.Size);
		long total = await _roleRepository.GetCountAsync(input.Number, input.Name, input.IsActive);
		return new PagedResponseDto<RoleDto>(total, ObjectMapper.Map<List<IdentityRole>, List<RoleDto>>(list))
		{
			Current = input.Current,
			Size = input.Size,
			Sorts = input.Sorts
		};
	}

	[HttpGet("assignable-roles")]
	public async Task<List<RoleDto>> GetAssignableRolesAsync()
	{
		var roles = await _roleRepository.GetListAsync(isActive: true, sorting: nameof(IdentityRole.Name));
		return ObjectMapper.Map<List<IdentityRole>, List<RoleDto>>(roles);
	}

	[HttpPost("add")]
	[Authorize(PmsPermissions.System.Roles.Create)]
	public async Task<RoleDto> CreateAsync(RoleCreateDto input)
	{
		await CheckNumberUniquenessAsync(input.Number);

		var role = new IdentityRole(GuidGenerator.Create(), input.Name, CurrentTenant?.Id)
		{
			IsDefault = input.IsDefault,
			IsPublic = input.IsPublic
		};

		role.SetProperty("Number", input.Number);
		role.SetProperty("Description", input.Description ?? string.Empty);
		role.SetProperty("IsActive", input.IsActive);

		(await _roleManager.CreateAsync(role)).CheckErrors();
		if (CurrentUnitOfWork != null)
		{
			await CurrentUnitOfWork.SaveChangesAsync();
		}

		return ObjectMapper.Map<IdentityRole, RoleDto>(role);
	}

	[HttpPut("edit/{id}")]
	[Authorize(PmsPermissions.System.Roles.Update)]
	public async Task<RoleDto> UpdateAsync(Guid id, RoleUpdateDto input)
	{
		await CheckNumberUniquenessAsync(input.Number, id);

		var role = await _roleRepository.GetAsync(id);
		role.SetConcurrencyStampIfNotNull(input.ConcurrencyStamp);

		if (role.Name != input.Name)
		{
			(await _roleManager.SetRoleNameAsync(role, input.Name)).CheckErrors();
		}

		role.IsDefault = input.IsDefault;
		role.IsPublic = input.IsPublic;

		role.SetProperty("Number", input.Number);
		role.SetProperty("Description", input.Description ?? string.Empty);
		role.SetProperty("IsActive", input.IsActive);

		(await _roleManager.UpdateAsync(role)).CheckErrors();
		if (CurrentUnitOfWork != null)
		{
			await CurrentUnitOfWork.SaveChangesAsync();
		}

		return ObjectMapper.Map<IdentityRole, RoleDto>(role);
	}

	[HttpDelete("delete/{id}")]
	[Authorize(PmsPermissions.System.Roles.Delete)]
	public async Task DeleteAsync(Guid id)
	{
		var role = await _roleRepository.GetAsync(id);
		(await _roleManager.DeleteAsync(role)).CheckErrors();
	}

	[HttpDelete("batch-delete")]
	[Authorize(PmsPermissions.System.Roles.Delete)]
	public async Task DeleteManyAsync(List<Guid> ids)
	{
		foreach (var id in ids)
		{
			var role = await _roleRepository.FindAsync(id);
			if (role == null)
			{
				continue;
			}
			(await _roleManager.DeleteAsync(role)).CheckErrors();
		}
		//await _roleRepository.DeleteManyAsync(ids);
	}

	private async Task CheckNumberUniquenessAsync(string number, Guid? excludeRoleId = null)
	{
		var exists = await _roleRepository.CheckNumberExistsAsync(number, excludeRoleId);
		if (exists)
		{
			throw new PmsBusinessException(ApiResponseCode.ValidationError, L["Role:Validation:NumberExists", number].Value);
		}
	}
}
