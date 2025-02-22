﻿using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Domain;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict.Authorizations;
using Volo.Abp.OpenIddict.Tokens;

namespace Volo.Abp.OpenIddict;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(AbpBackgroundWorkersModule),
    typeof(AbpIdentityDomainModule),
    typeof(AbpOpenIddictDomainSharedModule)
)]
public class AbpOpenIddictDomainModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        AddOpenIddict(context.Services);
    }

    private static void AddOpenIddict(IServiceCollection services)
    {
        var openIddictBuilder = services.AddOpenIddict();

        openIddictBuilder.AddAbpOpenIddict();

        var openIddictCoreBuilder = openIddictBuilder.AddAbpOpenIddictCore();

        services.ExecutePreConfiguredActions(openIddictBuilder);

        services.ExecutePreConfiguredActions(openIddictCoreBuilder);
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var options = context.ServiceProvider.GetRequiredService<IOptions<AbpOpenIddictCleanupOptions>>().Value;
        var backgroundWorkerManager = context.ServiceProvider.GetRequiredService<IBackgroundWorkerManager>();
        if (options.IsCleanupAuthorizationEnabled)
        {
            await backgroundWorkerManager.AddAsync(
                 context.ServiceProvider
                     .GetRequiredService<OpenIddictAuthorizationCleanupBackgroundWorker>()
             );
        }
        if (options.IsCleanupTokenEnabled)
        {
            await backgroundWorkerManager.AddAsync(
                context.ServiceProvider
                    .GetRequiredService<OpenIddictTokenCleanupBackgroundWorker>()
            );
        }
    }
}