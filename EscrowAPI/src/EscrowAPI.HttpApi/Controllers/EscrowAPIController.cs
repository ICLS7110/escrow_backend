using EscrowAPI.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace EscrowAPI.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class EscrowAPIController : AbpControllerBase
{
    protected EscrowAPIController()
    {
        LocalizationResource = typeof(EscrowAPIResource);
    }
}
