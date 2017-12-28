using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MyApp.Infrastructure;
using MyApp.Web;
using MyApp.Web.Controllers;
using Newtonsoft.Json;
using Xunit;

namespace MyApp.IntegrationTests.SampleData
{
    public struct URLs
    {
        private const string DefaultPath = "api";

        public const string GetSampleData = DefaultPath + "/sampledata";
    }

    public class SampleDataTests : IClassFixture<TestFixture<TestStatup, Startup>>, IDisposable
    {
        private readonly HttpClient client;

        private readonly MyAppDbContext dbContext;

        public SampleDataTests(TestFixture<TestStatup, Startup> fixture)
        {
            this.client = fixture.HttpClient;
            this.dbContext = fixture.DbContext;
        }

        [Fact]
        public async Task GetSampleData()
        {
            var response = await this.client.GetAsync(URLs.GetSampleData);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<SampleDataController.WeatherForecast>>(responseString);

            //Assert
            Assert.NotNull(result);
        }

        /// <inheritdoc />
        /// <summary>
        /// Get rid of any data or objs we want between Xunit test runs
        /// </summary>
        public void Dispose()
        {

        }
    }
}
