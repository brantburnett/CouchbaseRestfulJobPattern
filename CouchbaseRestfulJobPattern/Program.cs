using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CouchbaseRestfulJobPattern
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var webHost = CreateWebHostBuilder(args).Build();

            var configuration = webHost.Services.GetRequiredService<IConfiguration>();

            // Wait for Couchbase startup during local debugging
            var waitFile = configuration.GetValue<string>("WaitFile", null);
            if (!string.IsNullOrEmpty(waitFile))
            {
                var logger = webHost.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
                while (!File.Exists(waitFile))
                {
                    logger.LogInformation("Waiting for file {0}...", waitFile);
                    System.Threading.Thread.Sleep(1000);
                }
            }

            webHost.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
