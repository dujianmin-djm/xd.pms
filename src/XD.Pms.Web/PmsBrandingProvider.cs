using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Localization;
using XD.Pms.Localization;

namespace XD.Pms.Web;

[Dependency(ReplaceServices = true)]
public class PmsBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<PmsResource> _localizer;

    public PmsBrandingProvider(IStringLocalizer<PmsResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];

	public override string? LogoUrl => "images/logo/leptonxlite/logo-light.png";

	public override string? LogoReverseUrl => "images/logo/leptonxlite/logo-dark.png";
}
