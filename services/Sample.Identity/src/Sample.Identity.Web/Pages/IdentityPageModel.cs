using Sample.Identity.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Sample.Identity.Web.Pages;

/* Inherit your PageModel classes from this class.
 */
public abstract class IdentityPageModel : AbpPageModel
{
    protected IdentityPageModel()
    {
        LocalizationResourceType = typeof(IdentityResource);
    }
}
