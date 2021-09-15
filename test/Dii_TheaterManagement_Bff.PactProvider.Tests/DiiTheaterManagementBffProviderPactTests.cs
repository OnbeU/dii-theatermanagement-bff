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
         : IClassFixture<WebApplicationFactory<Dii_TheaterManagement_Bff.Startup>>,
        IClassFixture<WebApplicationFactory<Dii_OrderingSvc.Fake.Startup>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly WebApplicationFactory<Dii_TheaterManagement_Bff.Startup> _factory;
        private readonly WebApplicationFactory<Dii_OrderingSvc.Fake.Startup> _orderServiceFakeFactory;
        private readonly PactVerifierConfig pactVerifierConfig;
        private const string providerId = "dii-theatermanagement-bff";
        public DiiTheaterManagementBffProviderPactTests(ITestOutputHelper testOutputHelper, 
            WebApplicationFactory<Dii_TheaterManagement_Bff.Startup> factory
            , WebApplicationFactory<Dii_OrderingSvc.Fake.Startup> orderServiceFakeFactory)
        {
            _outputHelper = testOutputHelper;
            _factory = factory;
            _orderServiceFakeFactory = orderServiceFakeFactory;
            // Arrange
            pactVerifierConfig = new PactVerifierConfig
            {
                Outputters = new List<IOutput> { new XUnitOutput(_outputHelper) },
                Verbose = true, // Output verbose verification logs to the test output
                ProviderVersion = Environment.GetEnvironmentVariable("GIT_COMMIT"),
                PublishVerificationResults = "true".Equals(Environment.GetEnvironmentVariable("PublishPactVerificationResults"))
            };
        }

        [Fact (Skip = "To do Later")]
        public void HonorPactWithMvc()
        {

            // Arrange
            string consumerId = "dii-theatermanagement-web";

            //Act / Assert
            var httpClientForInMemoryInstanceOfApp = _factory.CreateClient();
            using (var inMemoryReverseProxy = new InMemoryReverseProxy(httpClientForInMemoryInstanceOfApp))
            {
                string providerBase = inMemoryReverseProxy.LocalhostAddress;

                IPactVerifier pactVerifier = new PactVerifier(pactVerifierConfig);
                pactVerifier.ProviderState($"{providerBase}/provider-states")
                    .ServiceProvider(providerId, providerBase)
                    .HonoursPactWith(consumerId)
                    //.PactUri(absolutePathToPactFile)
                    .PactBroker(
                        "https://onbe.pactflow.io",
                        consumerVersionSelectors: new List<VersionTagSelector>
                        {
                            new VersionTagSelector("main", latest: true),
                            new VersionTagSelector("production", latest: true)
                        })
                    .Verify();
            }
        }

        [Fact]
        public void HonorPactWithSpa()
        {
            string consumerId = "dii-theatermanagement-spa";


            var httpClientForInMemoryInstanceOfApp = _factory.CreateClient();
            var httpClientForInMemoryInstanceOfOrderingSvcApp = _orderServiceFakeFactory.CreateClient();

            using (var inMemoryReverseProxyOrderinfSvc = new InMemoryReverseProxy(httpClientForInMemoryInstanceOfOrderingSvcApp))
            using (var inMemoryReverseProxy = new InMemoryReverseProxy(httpClientForInMemoryInstanceOfApp))
            {
                string providerBase = inMemoryReverseProxyOrderinfSvc.LocalhostAddress;
                string providerUri = inMemoryReverseProxy.LocalhostAddress;

                IPactVerifier pactVerifier = new PactVerifier(pactVerifierConfig);
                pactVerifier.ProviderState($"{providerBase}/provider-states")
                    .ServiceProvider(providerId, providerUri)
                    .HonoursPactWith(consumerId)
                    //.PactUri(absolutePathToPactFile)
                    .PactBroker(
                        "https://onbe.pactflow.io",
                        consumerVersionSelectors: new List<VersionTagSelector>
                        {
                            new VersionTagSelector("stagesite", latest: true),
                           // new VersionTagSelector("production", latest: true)
                        })
                    .Verify();
            }
        }
    }
}
