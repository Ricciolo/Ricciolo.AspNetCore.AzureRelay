using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ricciolo.AspNetCore.AzureRelay;

namespace ConsoleTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildSenderWebHost(args).Run();
        }

        public static IWebHost BuildSenderWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureServices((b, s) => s.Configure<RelayOptions>(b.Configuration.GetSection("AzureRelay")))
                .UseStartup<Startup>()
                .Build();

    }
}
