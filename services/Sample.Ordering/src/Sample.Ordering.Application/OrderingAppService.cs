using System;
using System.Collections.Generic;
using System.Text;
using Sample.Ordering.Localization;
using Volo.Abp.Application.Services;

namespace Sample.Ordering;

/* Inherit your application services from this class.
 */
public abstract class OrderingAppService : ApplicationService
{
    protected OrderingAppService()
    {
        LocalizationResource = typeof(OrderingResource);
    }
}
