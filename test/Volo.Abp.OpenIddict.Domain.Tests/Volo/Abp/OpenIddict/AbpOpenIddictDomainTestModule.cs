﻿using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.EntityFrameworkCore;

namespace Volo.Abp.OpenIddict;

/* Domain tests are configured to use the EF Core provider.
 * You can switch to MongoDB, however your domain tests should be
 * database independent anyway.
 */
[DependsOn(
    typeof(AbpOpenIddictEntityFrameworkCoreTestModule)
    )]
public class AbpOpenIddictDomainTestModule : AbpModule
{

}
