using XD.Pms.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace XD.Pms.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class PmsControllerBase : AbpControllerBase
{
    protected PmsControllerBase()
    {
        LocalizationResource = typeof(PmsResource);
    }
}
