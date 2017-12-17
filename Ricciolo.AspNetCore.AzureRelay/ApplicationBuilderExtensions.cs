using System;
using System.Collections.Generic;
using System.Text;
using Ricciolo.AspNetCore.AzureRelay;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseAzureRelay(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RelaySenderMiddleware>();
        }
    }
}
