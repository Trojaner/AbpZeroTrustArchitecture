using System;
using System.Collections.Generic;
using System.Text;
using Sample.Auditing.Localization;
using Volo.Abp.Application.Services;

namespace Sample.Auditing;

/* Inherit your application services from this class.
 */
public abstract class AuditingAppService : ApplicationService
{
    protected AuditingAppService()
    {
        LocalizationResource = typeof(AuditingResource);
    }
}
