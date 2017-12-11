using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using ASPNetCoreSamples;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ASPNETCoreIntegrationTests
{
    public class Message
    {
        public Message(string content)
        {
            this.Content = content;
        }

        private string Name { get; set; }

        public string Content { get; }
    }


    public interface IQueueProvider
    {
        Task AddMessageAsync(Message message);

        Task<Message> ReceiveMessageAsync();

        Task<bool> DeleteMessageAsync(string messageReceipt);
    }

    public class SQSQueueProvider : IQueueProvider
    {
        public Task AddMessageAsync(Message message)
        {
            throw new NotImplementedException();
        }

        public Task<Message> ReceiveMessageAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteMessageAsync(string messageReceipt)
        {
            throw new NotImplementedException();
        }
    }

    public class ServiceBusQueueProvider : IQueueProvider
    {
        public Task AddMessageAsync(Message message)
        {
            throw new NotImplementedException();
        }

        public Task<Message> ReceiveMessageAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteMessageAsync(string messageReceipt)
        {
            throw new NotImplementedException();
        }
    }

    public class SampleDataIntegrationTest
    {
        [Fact]
        public void Test1()
        {

        }
    }
    
    /// <summary>
    /// Interface for a message tester which retains the payload for use when comparing output data.
    /// </summary>
    public interface IRequestGrabber
    {
        /// <summary>
        /// Gets any data that was sent and saved under a specific key in the internal data structure
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        (bool hasPayload, RequestData payload) GetPayload(string key);

        /// <summary>
        /// Gets all the keys used in the internal data structure
        /// </summary>
        /// <returns></returns>
        List<string> GetKeys();

        /// <summary>
        /// Adds a message to the internal data structure under a specific key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="message"></param>
        void AddRequest(string key, string message);

        /// <summary>
        /// Adds a message to the internal data structure under a specific key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="headers"></param>
        /// <param name="message"></param>
        void AddRequest(string key, Dictionary<string, string> headers, string message);

        /// <summary>
        /// Resets the grabber
        /// </summary>
        void Reset();
    }


    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public class RequestGrabber : IRequestGrabber
    {
        private readonly Dictionary<string, RequestData> data;
        private readonly Dictionary<string, int> counter;

        public RequestGrabber()
        {
            data = new Dictionary<string, RequestData>();
            counter = new Dictionary<string, int>();
        }

        public (bool hasPayload, RequestData payload) GetPayload(string key)
        {
            var result = this.data.TryGetValue(key.ToUpper(), out var request);

            return (result, request);
        }

        public List<string> GetKeys()
        {
            return new List<string>(this.data.Keys);
        }

        public void AddRequest(string key, string message)
        {
            var request = new RequestData
            {
                Body = message
            };

            InsertData(key, request);
        }

        public void AddRequest(string key, Dictionary<string, string> headers, string message)
        {
            var request = new RequestData
            {
                Headers = headers,
                Body = message
            };

            InsertData(key, request);
        }

        private void InsertData(string key, RequestData requestData)
        {
            var count = 1;
            var result = this.counter.ContainsKey(key);
            if (result)
            {
                count = this.counter[key];
                count++;
            }

            this.data.Add($"{key.ToUpper()}-{count}", requestData);
            this.counter[key] = count;
        }

        public void Reset()
        {
            this.data.Clear();
            this.counter.Clear();
        }
    }

    /// <summary>
    /// Holds a request, body and any headers/message attributes
    /// </summary>
    public class RequestData
    {
        public RequestData()
        {
            this.Headers = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Headers { get; set; }

        public string Body { get; set; }
    }

    public class TestStatup : Startup
    {
        //private SqliteConnection connection;

        public TestStatup(IConfiguration configuration) : base(configuration)
        { }

        /// <summary>
        /// Injecting stuff that needs to be modified for testing and mocking
        /// </summary>
        /// <param name="services"></param>
        protected override void ConfigureMockableServices(IServiceCollection services)
        {
            services.AddTransient<ISqsClient, SqsClientMock>();
        }

        /// <summary>
        /// Injects an in memory instance of the db context using sqlite in memory.
        /// The IOC scope here is set to Singlton to that the memeory instnace exists for as long as the tests are running
        /// As such any db data you would like to clean up between tests runs needs to be done in a disposable method, XUnit will obay this request.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="dbType"></param>
        protected override void SetupDatabase(IServiceCollection services, DBConfig.DBtypes dbType)
        {
            ////var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:", Cache = SqliteCacheMode.Shared };
            //var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:", Cache = SqliteCacheMode.Shared };
            //var connectionString = connectionStringBuilder.ToString();
            //var connection = new SqliteConnection(connectionString);
            ////connection.Open();

            //services.AddDbContext<StagingDbContext>(
            //    options => options.UseSqlite(connection, b =>
            //        b.MigrationsAssembly("Idm.StagingProcessor.Application")));

            const string connectionString = "server=localhost;userid=rostering;pwd=juizypassu;port=3306;database=idm_stagingdb_integration_tests;";

            services.AddDbContextPool<DbContext>(options =>
                options.UseMySql(connectionString, b =>
                    b.MigrationsAssembly("Idm.StagingProcessor.Application")));
        }
    }
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

            this.EbrMockResponseHandler = this.server.Host.Services.GetService<IEbrMockResponseHandler>();
        }

        public HttpClient HttpClient { get; }

        public StagingDbContext DbContext { get; private set; }

        public IRequestGrabber RequestGrabber { get; set; }

        public IEbrMockResponseHandler EbrMockResponseHandler { get; set; }

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