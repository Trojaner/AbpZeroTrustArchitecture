using System;
using System.Collections.Generic;
using System.Text;
using Sample.Products.Localization;
using Volo.Abp.Application.Services;

namespace Sample.Products;

/* Inherit your application services from this class.
 */
public abstract class ProductsAppService : ApplicationService
{
    protected ProductsAppService()
    {
        LocalizationResource = typeof(ProductsResource);
    }
}
