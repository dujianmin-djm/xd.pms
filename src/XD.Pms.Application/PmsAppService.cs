using XD.Pms.Localization;
using Volo.Abp.Application.Services;

namespace XD.Pms;

/* Inherit your application services from this class.
 */
public abstract class PmsAppService : ApplicationService
{
    protected PmsAppService()
    {
        LocalizationResource = typeof(PmsResource);
    }
}
