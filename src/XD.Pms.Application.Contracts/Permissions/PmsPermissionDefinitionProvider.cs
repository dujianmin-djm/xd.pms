using XD.Pms.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace XD.Pms.Permissions;

public class PmsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var pmsGroup = context.AddGroup(PmsPermissions.GroupName);

        var booksPermission = pmsGroup.AddPermission(PmsPermissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(PmsPermissions.Books.Create, L("Permission:Books.Create"));
        booksPermission.AddChild(PmsPermissions.Books.Edit, L("Permission:Books.Edit"));
        booksPermission.AddChild(PmsPermissions.Books.Delete, L("Permission:Books.Delete"));

		var apiKeysPermission = pmsGroup.AddPermission(PmsPermissions.ApiKeys.Default, L("Permission:ApiKeys"));
		apiKeysPermission.AddChild(PmsPermissions.ApiKeys.Create, L("Permission:ApiKeys.Create"));
		apiKeysPermission.AddChild(PmsPermissions.ApiKeys.Edit, L("Permission:ApiKeys.Update"));
		apiKeysPermission.AddChild(PmsPermissions.ApiKeys.Delete, L("Permission:ApiKeys.Delete"));
	}

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PmsResource>(name);
    }
}
