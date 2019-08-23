using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Quarkless
{
    /// <summary>
    /// The Main function can be used to run the ASP.NET Core application locally using the Kestrel webserver.
    /// </summary>
    public class LocalEntryPoint
    {
        public static async Task Main(string[] args)
        {
            var webHost = BuildWebHost(args);
            using (var scope = webHost.Services.CreateScope())
            {
	            // get the IpPolicyStore instance
	            var ipPolicyStore = scope.ServiceProvider.GetRequiredService<IIpPolicyStore>();

	            // seed IP data from appsettings
	            await ipPolicyStore.SeedAsync();
            }

            await webHost.RunAsync();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
