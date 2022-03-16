using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace Sample.Identity.Web;

[Dependency(ReplaceServices = true)]
public class IdentityBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "Identity";
}
