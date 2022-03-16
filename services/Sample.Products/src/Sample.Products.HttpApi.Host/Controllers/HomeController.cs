using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Sample.Products.Controllers;

public class HomeController : AbpController
{
    public ActionResult Index()
    {
        return Redirect("~/swagger");
    }
}
