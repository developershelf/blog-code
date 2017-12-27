using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Web;

namespace MyApp.IntegrationTests
{
    public class TestStatup : Startup
    {
        //private SqliteConnection connection;

        public TestStatup(IConfiguration configuration) 
            : base(configuration)
        { }

        /// <summary>
        /// Injecting stuff that needs to be modified for testing and mocking
        /// </summary>
        /// <param name="services"></param>
        protected override void ConfigureMockableServices(IServiceCollection services)
        {

        }

        /// <summary>
        /// Injects an in memory instance of the db context using sqlite in memory.
        /// The IOC scope here is set to Singlton to that the memeory instnace exists for as long as the tests are running
        /// As such any db data you would like to clean up between tests runs needs to be done in a disposable method, XUnit will obay this request.
        /// </summary>
        /// <param name="services"></param>
        protected override void SetupDatabase(IServiceCollection services)
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
}