using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;
using XD.Pms.ApiResponse;
using XD.Pms.Identity.Role.Dto;
using XD.Pms.Identity.User;
using XD.Pms.Identity.User.Dto;
using XD.Pms.Permissions;
using XD.Pms.Services.Dtos;

namespace XD.Pms.Identity;

[Route("papi/user")]
[Authorize(PmsPermissions.System.Users.Default)]
public class UserAppService : PmsAppService, IUserAppService
{
	private readonly IdentityUserManager _userManager;
	private readonly IIdentityUserRepository _identityUserRepository;
	private readonly IOptions<IdentityOptions> _identityOptions;
	private readonly IPermissionChecker _permissionChecker;
	private readonly UserLimitManager _userLimitManager;
	private readonly IUserRepository _userRepository;
	
	public UserAppService(
		IdentityUserManager userManager,
		IIdentityUserRepository identityUserRepository,
		IOptions<IdentityOptions> identityOptions,
		IPermissionChecker permissionChecker,
		UserLimitManager userLimitManager,
		IUserRepository userRepository) 
	{
		_userManager = userManager;
		_identityUserRepository = identityUserRepository;
		_identityOptions = identityOptions;
		_permissionChecker = permissionChecker;
		_userLimitManager = userLimitManager;
		_userRepository = userRepository;
	}

	[HttpGet("{id}")]
	public async Task<UserDto> GetAsync(Guid id)
	{
		return ObjectMapper.Map<IdentityUser, UserDto>(
			await _userManager.GetByIdAsync(id)
		);
	}

	[HttpGet("query")]
	public async Task<PagedResponseDto<UserDto>> GetListAsync(UserReadDto input)
	{
		int skpCount = (input.Current - 1) * input.Size;
		var list = await _userRepository.GetListAsync(input.UserName, input.Gender, input.PhoneNumber, input.Email, input.IsActive, input.Sorts, skpCount, input.Size);
		long total = await _userRepository.GetCountAsync(input.UserName, input.Gender, input.PhoneNumber, input.Email, input.IsActive);
		return new PagedResponseDto<UserDto>(total, ObjectMapper.Map<List<IdentityUser>, List<UserDto>>(list))
		{
			Current = input.Current,
			Size = input.Size,
			Sorts = input.Sorts
		};
	}

	[HttpPost("add")]
	[Authorize(PmsPermissions.System.Users.Create)]
	public async Task<UserDto> CreateAsync(UserCreateDto input)
	{
		if (input.IsActive)
		{
			await _userLimitManager.CheckUserLimitWithLockAsync();
		}
		await _identityOptions.SetAsync();

		var user = new IdentityUser(
			GuidGenerator.Create(),
			input.UserName,
			input.Email,
			CurrentTenant.Id
		);

		input.MapExtraPropertiesTo(user);

		(await _userManager.CreateAsync(user, input.Password)).CheckErrors();

		await UpdateUserByInput(user, input);
		user.SetProperty("Gender", input.Gender);
		user.SetProperty("Description", input.Description ?? string.Empty);

		(await _userManager.UpdateAsync(user)).CheckErrors();

		if (CurrentUnitOfWork != null)
		{
			await CurrentUnitOfWork.SaveChangesAsync();
		}

		return ObjectMapper.Map<IdentityUser, UserDto>(user);
	}

	[HttpPut("edit/{id}")]
	[Authorize(PmsPermissions.System.Users.Update)]
	public async Task<UserDto> UpdateAsync(Guid id, UserUpdateDto input)
	{
		await _identityOptions.SetAsync();

		var user = await _userManager.GetByIdAsync(id);
		if (!user.IsActive && input.IsActive)
		{
			await _userLimitManager.CheckUserLimitWithLockAsync();
		}

		user.SetConcurrencyStampIfNotNull(input.ConcurrencyStamp);

		(await _userManager.SetUserNameAsync(user, input.UserName)).CheckErrors();

		await UpdateUserByInput(user, input);
		user.SetProperty("Gender", input.Gender);
		user.SetProperty("Description", input.Description ?? string.Empty);
		input.MapExtraPropertiesTo(user);

		(await _userManager.UpdateAsync(user)).CheckErrors();

		if (!input.Password.IsNullOrEmpty())
		{
			(await _userManager.RemovePasswordAsync(user)).CheckErrors();
			(await _userManager.AddPasswordAsync(user, input.Password)).CheckErrors();
		}

		if (CurrentUnitOfWork != null)
		{
			await CurrentUnitOfWork.SaveChangesAsync();
		}

		return ObjectMapper.Map<IdentityUser, UserDto>(user);
	}
	
	[HttpDelete("delete/{id}")]
	[Authorize(PmsPermissions.System.Users.Delete)]
	public async Task DeleteAsync(Guid id)
	{
		if (CurrentUser.Id == id)
		{
			throw new BusinessException(code: IdentityErrorCodes.UserSelfDeletion);
		}
		var user = await _userManager.FindByIdAsync(id.ToString());
		if (user != null)
		{
			(await _userManager.DeleteAsync(user)).CheckErrors();
		}
	}

	[HttpGet("{id}/roles")]
	public async Task<ListResultDto<RoleDto>> GetRolesAsync(Guid id)
	{
		var roles = (await _identityUserRepository.GetRolesAsync(id)).OrderBy(x => x.Name).ToList();
		return new ListResultDto<RoleDto>(
			ObjectMapper.Map<List<IdentityRole>, List<RoleDto>>(roles)
		);
	}

	[HttpPut("{id}/roles")]
	[Authorize(PmsPermissions.System.Users.Update)]
	public async Task UpdateRolesAsync(Guid id, IdentityUserUpdateRolesDto input)
	{
		await _identityOptions.SetAsync();
		var user = await _userManager.GetByIdAsync(id);
		(await _userManager.SetRolesAsync(user, input.RoleNames)).CheckErrors();
		await _identityUserRepository.UpdateAsync(user);
	}

	[HttpGet("by-username/{userName}")]
	public async Task<UserDto> GetByUsernameAsync(string userName)
	{
		var user = await _userManager.FindByNameAsync(userName)
			?? throw new PmsBusinessException(ApiResponseCode.ValidationError, L["User:Validation:UserNameNotExists", userName].Value);
		return ObjectMapper.Map<IdentityUser, UserDto>(user);
	}

	[HttpGet("by-email/{email}")]
	public async Task<UserDto> GetByEmailAsync(string email)
	{
		var user = await _userManager.FindByEmailAsync(email)
			?? throw new PmsBusinessException(ApiResponseCode.ValidationError, L["User:Validation:UserEmailNotExists", email].Value);
		return ObjectMapper.Map<IdentityUser, UserDto>(user);
	}

	private async Task UpdateUserByInput(IdentityUser user, IdentityUserCreateOrUpdateDtoBase input)
	{
		if (!string.Equals(user.Email, input.Email, StringComparison.InvariantCultureIgnoreCase))
		{
			(await _userManager.SetEmailAsync(user, input.Email)).CheckErrors();
		}

		if (!string.Equals(user.PhoneNumber, input.PhoneNumber, StringComparison.InvariantCultureIgnoreCase))
		{
			(await _userManager.SetPhoneNumberAsync(user, input.PhoneNumber)).CheckErrors();
		}

		(await _userManager.SetLockoutEnabledAsync(user, input.LockoutEnabled)).CheckErrors();

		if (user.Id != CurrentUser.Id)
		{
			user.SetIsActive(input.IsActive);
		}

		user.Name = input.Name?.Trim();
		user.Surname = input.Surname?.Trim();
		(await _userManager.UpdateAsync(user)).CheckErrors();
		if (input.RoleNames != null && await _permissionChecker.IsGrantedAsync(PmsPermissions.System.Users.ManageRoles))
		{
			(await _userManager.SetRolesAsync(user, input.RoleNames)).CheckErrors();
		}
	}
}
