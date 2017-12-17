using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ricciolo.AspNetCore.AzureRelay;
using Ricciolo.AspNetCore.AzureRelay.FeatureSerializer;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class RelayServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureRelay(this IServiceCollection services, Action<RelayOptions> configure)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            services.Configure(configure);
            AddAzureRelay(services);

            return services;
        }

        public static IServiceCollection AddAzureRelay(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            services.AddTransient<RelaySender>();
            services.AddTransient<FeatureSerializerManager>();
            services.AddTransient<IFeatureSerializer, HttpRequestFeatureSerializer>();
            services.AddTransient<IFeatureSerializer, HttpResponseFeatureSerializer>();

            return services;
        }
    }
}
