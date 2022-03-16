﻿using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Sample.Products.Data;

/* This is used if database provider does't define
 * IProductsDbSchemaMigrator implementation.
 */
public class NullProductsDbSchemaMigrator : IProductsDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
