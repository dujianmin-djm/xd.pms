using XD.Pms.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace XD.Pms.Web.Pages;

public abstract class PmsPageModel : AbpPageModel
{
    protected PmsPageModel()
    {
        LocalizationResourceType = typeof(PmsResource);
    }
}
