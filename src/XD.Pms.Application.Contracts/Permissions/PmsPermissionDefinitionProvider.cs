using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using XD.Pms.Localization;

namespace XD.Pms.Permissions;

public class PmsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
		// System
		var systemGroup = context.AddGroup(PmsPermissions.System.GroupName, L("Permission:SystemManagement"));

		var roles = systemGroup.AddPermission(PmsPermissions.System.Roles.Default, L("Permission:RoleManagement"));
		roles.AddChild(PmsPermissions.System.Roles.Create, L("Permission:Create"));
		roles.AddChild(PmsPermissions.System.Roles.Update, L("Permission:Uptate"));
		roles.AddChild(PmsPermissions.System.Roles.Delete, L("Permission:Delete"));
		roles.AddChild(PmsPermissions.System.Roles.ManagePermissions, L("Permission:ChangePermissions"));

		var users = systemGroup.AddPermission(PmsPermissions.System.Users.Default, L("Permission:UserManagement"));
		users.AddChild(PmsPermissions.System.Users.Create, L("Permission:Create"));
		users.AddChild(PmsPermissions.System.Users.Update, L("Permission:Uptate"));
		users.AddChild(PmsPermissions.System.Users.Delete, L("Permission:Delete"));
		users.AddChild(PmsPermissions.System.Users.ManagePermissions, L("Permission:ChangePermissions"));
		users.AddChild(PmsPermissions.System.Users.ManageRoles, L("Permission:ManageRoles"));

		var apiKeys = systemGroup.AddPermission(PmsPermissions.System.ApiKeys.Default, L("Permission:ApiKeyManagement"));
		apiKeys.AddChild(PmsPermissions.System.ApiKeys.Create, L("Permission:Create"));
		apiKeys.AddChild(PmsPermissions.System.ApiKeys.Update, L("Permission:Update"));
		apiKeys.AddChild(PmsPermissions.System.ApiKeys.Delete, L("Permission:Delete"));

		// BaseData
		var basedataGroup = context.AddGroup(PmsPermissions.BaseData.GroupName, L("Permission:BaseDataManagement"));

		var books = basedataGroup.AddPermission(PmsPermissions.BaseData.Books.Default, L("Permission:Books"));
		books.AddChild(PmsPermissions.BaseData.Books.Create, L("Permission:Create"));
		books.AddChild(PmsPermissions.BaseData.Books.Update, L("Permission:Update"));
		books.AddChild(PmsPermissions.BaseData.Books.Delete, L("Permission:Delete"));

		// Business
		var bizGroup = context.AddGroup(PmsPermissions.Business.GroupName, L("Permission:BusinessManagement"));

	}

	private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PmsResource>(name);
    }
}
