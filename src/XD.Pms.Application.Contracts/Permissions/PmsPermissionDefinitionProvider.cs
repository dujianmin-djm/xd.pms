using XD.Pms.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace XD.Pms.Permissions;

public class PmsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(PmsPermissions.GroupName);

        var booksPermission = myGroup.AddPermission(PmsPermissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(PmsPermissions.Books.Create, L("Permission:Books.Create"));
        booksPermission.AddChild(PmsPermissions.Books.Edit, L("Permission:Books.Edit"));
        booksPermission.AddChild(PmsPermissions.Books.Delete, L("Permission:Books.Delete"));
        //Define your own permissions here. Example:
        //myGroup.AddPermission(PmsPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PmsResource>(name);
    }
}
