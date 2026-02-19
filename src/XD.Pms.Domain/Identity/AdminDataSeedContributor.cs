using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Uow;

namespace XD.Pms.Identity;

public class AdminDataSeedContributor : IDataSeedContributor, ITransientDependency
{
	private readonly IdentityUserManager _userManager;
	private readonly IdentityRoleManager _roleManager;
	private readonly IIdentityUserRepository _userRepository;
	private readonly IIdentityRoleRepository _roleRepository;

	public AdminDataSeedContributor(
		IdentityUserManager userManager,
		IdentityRoleManager roleManager,
		IIdentityUserRepository userRepository,
		IIdentityRoleRepository roleRepository)
	{
		_userManager = userManager;
		_roleManager = roleManager;
		_userRepository = userRepository;
		_roleRepository = roleRepository;
	}

	[UnitOfWork]
	public async Task SeedAsync(DataSeedContext context)
	{
		// 更新 admin 角色的自定义字段
		var adminRole = await _roleRepository.FindByNormalizedNameAsync("ADMIN");
		if (adminRole != null)
		{
			adminRole.SetProperty("Number", "admin");
			adminRole.SetProperty("Description", "系统管理员");
			adminRole.SetProperty("IsActive", true);
			await _roleManager.UpdateAsync(adminRole);
		}

		// 更新 admin 用户的自定义字段
		var adminUser = await _userRepository.FindByNormalizedUserNameAsync("ADMIN");
		if (adminUser != null)
		{
			adminUser.SetProperty("Gender", 0);
			adminUser.SetProperty("Description", "系统管理员");
			await _userManager.UpdateAsync(adminUser);
		}
	}
}
