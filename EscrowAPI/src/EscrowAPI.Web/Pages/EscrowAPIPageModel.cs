using EscrowAPI.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace EscrowAPI.Web.Pages;

public abstract class EscrowAPIPageModel : AbpPageModel
{
    protected EscrowAPIPageModel()
    {
        LocalizationResourceType = typeof(EscrowAPIResource);
    }
}
