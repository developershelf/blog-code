using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace MyApp.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        private static int GetHttpPort()
        {
            var port = 8080;
            try
            {
                port = Convert.ToInt32(Environment.GetEnvironmentVariable("HTTP_PORT"));
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Something went really wrong getting the http port for the application, check if it set", e.Message);
            }
            return port;
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    Console.WriteLine("The environment is " + env.EnvironmentName);

                    config.Sources.Clear();
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false, reloadOnChange: true);
                })
                .UseUrls($"http://+:{GetHttpPort()}")
                .Build();
    }
}
