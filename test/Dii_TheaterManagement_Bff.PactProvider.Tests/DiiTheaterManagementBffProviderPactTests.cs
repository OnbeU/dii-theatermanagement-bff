using Microsoft.AspNetCore.Mvc.Testing;
using PactNet;
using PactNet.Infrastructure.Outputters;
using PactTestingTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Dii_TheaterManagement_Bff.PactProvider.Tests
{
    public class DiiTheaterManagementBffProviderPactTests
         : IClassFixture<WebApplicationFactory<Dii_TheaterManagement_Bff.Startup>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly WebApplicationFactory<Dii_TheaterManagement_Bff.Startup> _factory;

        public DiiTheaterManagementBffProviderPactTests(ITestOutputHelper testOutputHelper, WebApplicationFactory<Dii_TheaterManagement_Bff.Startup> factory)
        {
            _outputHelper = testOutputHelper;
            _factory = factory;
        }

        [Fact]
        public void HonorPactWithMvc()
        {
            // Arrange
            var config = new PactVerifierConfig
            {
                Outputters = new List<IOutput> { new XUnitOutput(_outputHelper) },
                Verbose = true, // Output verbose verification logs to the test output
            };

            //Act / Assert
            string executingDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); // e.g. C:\src\dii\dii-admin-web\tests\FakeTheaterBff.Tests\bin\Debug\net5.0
            string pactsSharedItemsDir = Path.Combine(executingDir, @"..\..\..\..\Pacts");                           // e.g. C:\src\dii\dii-admin-web\tests\FakeTheaterBff.Tests\bin\Debug\net5.0\..\..\..\..\Pacts
            string absolutePathToSharedItemsDir = Path.GetFullPath(pactsSharedItemsDir);                             // e.g. C:\src\dii\dii-admin-web\tests\Pacts
            string absolutePathToPactFile = Path.Combine(absolutePathToSharedItemsDir, $"dii-theater-mvc-dii-theater-bff.json");
            string consumerId = "dii-theater-mvc";
            string providerId = "dii-theater-bff";

            var httpClientForInMemoryInstanceOfApp = _factory.CreateClient();
            using (var inMemoryReverseProxy = new InMemoryReverseProxy(httpClientForInMemoryInstanceOfApp))
            {
                string providerBase = inMemoryReverseProxy.LocalhostAddress;

                IPactVerifier pactVerifier = new PactVerifier(config);
                pactVerifier.ProviderState($"{providerBase}/provider-states")
                    .ServiceProvider(providerId, providerBase)
                    .HonoursPactWith(consumerId)
                    .PactUri(absolutePathToPactFile)
                    .Verify();
            }
        }

        [Fact]
        public void HonorPactWithSpa()
        {
            // Arrange
            var config = new PactVerifierConfig
            {
                Outputters = new List<IOutput> { new XUnitOutput(_outputHelper) },
                Verbose = true, // Output verbose verification logs to the test output
            };

            //Act / Assert
            string executingDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);  // e.g. C:\src\dii\dii-admin-web\tests\FakeTheaterBff.Tests\bin\Debug\net5.0
            string pactsDir = Path.Combine(executingDir, @"..\..\..\..\..\src\bcfinal-theatermanagement-spa\pacts");  // e.g. C:\src\dii\dii-admin-web\tests\FakeTheaterBff.Tests\bin\Debug\net5.0\..\..\..\..\..\src\bcfinal-theatermanagement-spa\pacts
            string absolutePathToSharedItemsDir = Path.GetFullPath(pactsDir);                                         // e.g. C:\src\dii\dii-admin-web\src\bcfinal-theatermanagement-spa\pacts
            string absolutePathToPactFile = Path.Combine(absolutePathToSharedItemsDir, $"bcfinal-theatermanagement-spa-bcfinal-theatermanagement-bff.json");
            string consumerId = "dii-theatermanagement-spa";
            string providerId = "dii-theatermanagement-bff";

            var httpClientForInMemoryInstanceOfApp = _factory.CreateClient();
            using (var inMemoryReverseProxy = new InMemoryReverseProxy(httpClientForInMemoryInstanceOfApp))
            {
                string providerBase = inMemoryReverseProxy.LocalhostAddress;

                IPactVerifier pactVerifier = new PactVerifier(config);
                pactVerifier.ProviderState($"{providerBase}/provider-states")
                    .ServiceProvider(providerId, providerBase)
                    .HonoursPactWith(consumerId)
                    .PactUri(absolutePathToPactFile)
                    .Verify();
            }
        }
    }
}
