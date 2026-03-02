using Volo.Abp.Authorization.Permissions;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.SettingManagement;
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
		roles.AddChild(PmsPermissions.System.Roles.Update, L("Permission:Update"));
		roles.AddChild(PmsPermissions.System.Roles.Delete, L("Permission:Delete"));
		roles.AddChild(PmsPermissions.System.Roles.ManagePermissions, L("Permission:ChangePermissions"));

		var users = systemGroup.AddPermission(PmsPermissions.System.Users.Default, L("Permission:UserManagement"));
		users.AddChild(PmsPermissions.System.Users.Create, L("Permission:Create"));
		users.AddChild(PmsPermissions.System.Users.Update, L("Permission:Update"));
		users.AddChild(PmsPermissions.System.Users.Delete, L("Permission:Delete"));
		users.AddChild(PmsPermissions.System.Users.ManageRoles, L("Permission:ManageRoles"));
		users.AddChild(PmsPermissions.System.Users.ResetPassword, L("Permission:ResetPassword"));

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

		var depts = basedataGroup.AddPermission(PmsPermissions.BaseData.Departments.Default, L("Permission:Departments"));
		depts.AddChild(PmsPermissions.BaseData.Departments.Create, L("Permission:Create"));
		depts.AddChild(PmsPermissions.BaseData.Departments.Update, L("Permission:Update"));
		depts.AddChild(PmsPermissions.BaseData.Departments.Delete, L("Permission:Delete"));
		depts.AddChild(PmsPermissions.BaseData.Departments.Submit, L("Permission:Submit"));
		depts.AddChild(PmsPermissions.BaseData.Departments.Cancel, L("Permission:Cancel"));
		depts.AddChild(PmsPermissions.BaseData.Departments.Audit, L("Permission:Audit"));
		depts.AddChild(PmsPermissions.BaseData.Departments.UnAudit, L("Permission:UnAudit"));

		var posts = basedataGroup.AddPermission(PmsPermissions.BaseData.Positions.Default, L("Permission:Positions"));
		posts.AddChild(PmsPermissions.BaseData.Positions.Create, L("Permission:Create"));
		posts.AddChild(PmsPermissions.BaseData.Positions.Update, L("Permission:Update"));
		posts.AddChild(PmsPermissions.BaseData.Positions.Delete, L("Permission:Delete"));
		posts.AddChild(PmsPermissions.BaseData.Positions.Submit, L("Permission:Submit"));
		posts.AddChild(PmsPermissions.BaseData.Positions.Cancel, L("Permission:Cancel"));
		posts.AddChild(PmsPermissions.BaseData.Positions.Audit, L("Permission:Audit"));
		posts.AddChild(PmsPermissions.BaseData.Positions.UnAudit, L("Permission:UnAudit"));

		var employees = basedataGroup.AddPermission(PmsPermissions.BaseData.Employees.Default, L("Permission:Employees"));
		employees.AddChild(PmsPermissions.BaseData.Employees.Create, L("Permission:Create"));
		employees.AddChild(PmsPermissions.BaseData.Employees.Update, L("Permission:Update"));
		employees.AddChild(PmsPermissions.BaseData.Employees.Delete, L("Permission:Delete"));
		employees.AddChild(PmsPermissions.BaseData.Employees.Submit, L("Permission:Submit"));
		employees.AddChild(PmsPermissions.BaseData.Employees.Cancel, L("Permission:Cancel"));
		employees.AddChild(PmsPermissions.BaseData.Employees.Audit, L("Permission:Audit"));
		employees.AddChild(PmsPermissions.BaseData.Employees.UnAudit, L("Permission:UnAudit"));

		// Business
		var bizGroup = context.AddGroup(PmsPermissions.Business.GroupName, L("Permission:BusinessManagement"));

	}

	public override void PostDefine(IPermissionDefinitionContext context)
	{
		// 移除不需要的ABP权限组
		context.RemoveGroup(IdentityPermissions.GroupName);
		context.RemoveGroup(FeatureManagementPermissions.GroupName);
		context.RemoveGroup(SettingManagementPermissions.GroupName);
		//context.RemoveGroup(TenantManagementPermissions.GroupName);
	}

	private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PmsResource>(name);
    }
}
