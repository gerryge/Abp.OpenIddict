﻿using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace OpenIddictDemo.Web
{
    [Dependency(ReplaceServices = true)]
    public class OpenIddictDemoBrandingProvider : DefaultBrandingProvider
    {
        public override string AppName => "OpenIddictDemo";
    }
}
