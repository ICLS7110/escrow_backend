using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Localization;
using EscrowAPI.Localization;

namespace EscrowAPI.Web;

[Dependency(ReplaceServices = true)]
public class EscrowAPIBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<EscrowAPIResource> _localizer;

    public EscrowAPIBrandingProvider(IStringLocalizer<EscrowAPIResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
