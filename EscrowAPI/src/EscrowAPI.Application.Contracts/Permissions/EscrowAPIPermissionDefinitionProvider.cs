using EscrowAPI.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace EscrowAPI.Permissions;

public class EscrowAPIPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(EscrowAPIPermissions.GroupName);

        var booksPermission = myGroup.AddPermission(EscrowAPIPermissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(EscrowAPIPermissions.Books.Create, L("Permission:Books.Create"));
        booksPermission.AddChild(EscrowAPIPermissions.Books.Edit, L("Permission:Books.Edit"));
        booksPermission.AddChild(EscrowAPIPermissions.Books.Delete, L("Permission:Books.Delete"));
        //Define your own permissions here. Example:
        //myGroup.AddPermission(EscrowAPIPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<EscrowAPIResource>(name);
    }
}
