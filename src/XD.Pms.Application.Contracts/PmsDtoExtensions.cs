using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Threading;
using XD.Pms.Enums;

namespace XD.Pms;

public static class PmsDtoExtensions
{
    private static readonly OneTimeRunner OneTimeRunner = new();

    public static void Configure()
    {
        OneTimeRunner.Run(() =>
        {
			/* You can add extension properties to DTOs defined in the depended modules.
			 *
			 * Example:
			 *
			 * ObjectExtensionManager.Instance
			 *   .AddOrUpdateProperty<IdentityRoleDto, string>("Title");
			 *
			 * See the documentation for more:
			 * https://docs.abp.io/en/abp/latest/Object-Extensions
			 */

			//ObjectExtensionManager.Instance
			//	.AddOrUpdateProperty<string>(
			//		[
			//			typeof(IdentityRoleDto),
			//			typeof(IdentityRoleCreateDto),
			//			typeof(IdentityRoleUpdateDto)
			//		],
			//		"Number"
			//	)
			//	.AddOrUpdateProperty<bool>(
			//		[
			//			typeof(IdentityRoleDto),
			//			typeof(IdentityRoleCreateDto),
			//			typeof(IdentityRoleUpdateDto)
			//		],
			//		"IsActive"
			//	)
			//	.AddOrUpdateProperty<string>(
			//		[
			//			typeof(IdentityRoleDto),
			//			typeof(IdentityRoleCreateDto),
			//			typeof(IdentityRoleUpdateDto),
			//			typeof(IdentityUserDto),
			//			typeof(IdentityUserCreateDto),
			//			typeof(IdentityUserUpdateDto)
			//		],
			//		"Description"
			//	)
			//	.AddOrUpdateProperty<Gender>(
			//		[
			//			typeof(IdentityUserDto),
			//			typeof(IdentityUserCreateDto),
			//			typeof(IdentityUserUpdateDto)
			//		],
			//		"Gender"
			//	);
		});
    }
}
