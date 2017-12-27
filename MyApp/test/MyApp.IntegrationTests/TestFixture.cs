using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyApp.IntegrationTests.Grabber;

namespace MyApp.IntegrationTests
{
    /// <summary>
    /// A test fixture which hosts the target project (project we wish to test) in an in-memory server.
    /// </summary>
    /// <typeparam name="TTestStartup"></typeparam>
    /// <typeparam name="TBaseStartup"></typeparam>
    public class TestFixture<TTestStartup, TBaseStartup> : IDisposable
    {
        private readonly TestServer server;

        public TestFixture()
            : this(Path.Combine("src"))
        { }

        protected TestFixture(string relativeTargetProjectParentDir)
        {
            var startupAssembly = typeof(TBaseStartup).GetTypeInfo().Assembly;
            var contentRoot = GetProjectPath(relativeTargetProjectParentDir, startupAssembly);

            var builder = new WebHostBuilder()
                .UseContentRoot(contentRoot)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    config.Sources.Clear();
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices(InitializeServices)
                .UseStartup(typeof(TTestStartup));

            server = new TestServer(builder);

            HttpClient = server.CreateClient();
            HttpClient.BaseAddress = new Uri("http://localhost");

            this.ConfigureTestFixture(this.server.Host.Services);
            this.ConfigurateDatbase(this.server.Host.Services);
            
        }

        public HttpClient HttpClient { get; }

        public DbContext DbContext { get; private set; }

        public IRequestGrabber RequestGrabber { get; set; }

        protected virtual void InitializeServices(IServiceCollection services)
        {
            var startupAssembly = typeof(TBaseStartup).GetTypeInfo().Assembly;

            // Inject a custom application part manager. 
            // Overrides AddMvcCore() because it uses TryAdd().
            var manager = new ApplicationPartManager();
            manager.ApplicationParts.Add(new AssemblyPart(startupAssembly));
            manager.FeatureProviders.Add(new ControllerFeatureProvider());
            manager.FeatureProviders.Add(new ViewComponentFeatureProvider());

            services.AddSingleton(manager);
            SetupTestDependencies(services);
        }

        /// <summary>
        /// Gets the full path to the target project that we wish to test
        /// </summary>
        /// <param name="projectRelativePath">
        /// The parent directory of the target project.
        /// e.g. src, samples, test, or test/Websites
        /// </param>
        /// <param name="startupAssembly">The target project's assembly.</param>
        /// <returns>The full path to the target project.</returns>
        private static string GetProjectPath(string projectRelativePath, Assembly startupAssembly)
        {
            // Get name of the target project which we want to test
            var projectName = startupAssembly.GetName().Name;

            // Get currently executing test project path
            var applicationBasePath = AppContext.BaseDirectory;

            // Find the path to the target project
            var directoryInfo = new DirectoryInfo(applicationBasePath);
            do
            {
                directoryInfo = directoryInfo.Parent;

                var projectDirectoryInfo = new DirectoryInfo(Path.Combine(directoryInfo.FullName, projectRelativePath));
                if (projectDirectoryInfo.Exists)
                {
                    var projectFileInfo = new FileInfo(Path.Combine(projectDirectoryInfo.FullName, projectName, $"{projectName}.csproj"));
                    if (projectFileInfo.Exists)
                    {
                        return Path.Combine(projectDirectoryInfo.FullName, projectName);
                    }
                }
            } while (directoryInfo.Parent != null);

            throw new Exception($"Project root could not be located using the application root {applicationBasePath}.");
        }

        private void ConfigurateDatbase(IServiceProvider provider)
        {
            this.DbContext = provider.GetService<DbContext>();
            //this.DbContext.Database.OpenConnection();
            this.DbContext.Database.Migrate();
        }

        private void ConfigureTestFixture(IServiceProvider provider)
        {
            this.RequestGrabber = provider.GetService<IRequestGrabber>();
        }

        /// <summary>
        /// This should only be used to add dependencies which are added to services needed in the fixture so they are available in the 
        /// </summary>
        /// <param name="services"></param>
        private static void SetupTestDependencies(IServiceCollection services)
        {
            services.AddSingleton<IRequestGrabber, RequestGrabber>();
        }

        //Fixture test clean up.
        public void Dispose()
        {
            HttpClient.Dispose();
            this.DbContext.Database.EnsureDeleted();
            this.DbContext.Dispose();
        }
    }
}