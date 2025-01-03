using EscrowAPI.Localization;
using Volo.Abp.Application.Services;

namespace EscrowAPI;

/* Inherit your application services from this class.
 */
public abstract class EscrowAPIAppService : ApplicationService
{
    protected EscrowAPIAppService()
    {
        LocalizationResource = typeof(EscrowAPIResource);
    }
}
