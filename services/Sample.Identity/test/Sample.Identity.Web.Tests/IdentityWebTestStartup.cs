using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp;

namespace Sample.Identity;

public class IdentityWebTestStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddApplication<IdentityWebTestModule>();
    }

    public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
        app.InitializeApplication();
    }
}
