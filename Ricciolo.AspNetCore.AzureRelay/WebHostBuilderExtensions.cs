using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using Ricciolo.AspNetCore.AzureRelay;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Hosting
{
    public static class WebHostBuilderRelayExtensions
    {
        public static IWebHostBuilder UseAzureRelayServer(this IWebHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<IServer, RelayServer>();
                services.AddTransient<RelayListener>();
            });
        }

        public static IWebHostBuilder UseAzureRelayServer(this IWebHostBuilder hostBuilder, Action<RelayOptions> options)
        {
            UseAzureRelayServer(hostBuilder);
            return hostBuilder.ConfigureServices(services => services.Configure(options));
        }
    }
}
