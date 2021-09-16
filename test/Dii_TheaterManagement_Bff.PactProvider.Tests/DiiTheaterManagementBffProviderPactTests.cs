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
using Dii_TheaterManagement_Bff.PactProvider;

namespace Dii_TheaterManagement_Bff.PactProvider.Tests
{
    public class DiiTheaterManagementBffProviderPactTests
         : IClassFixture<CustomWebApplicationFactory<Startup>>,
        IClassFixture<WebApplicationFactory<Dii_OrderingSvc.Fake.Startup>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly CustomWebApplicationFactory<Startup> _factory;
        private readonly WebApplicationFactory<Dii_OrderingSvc.Fake.Startup> _orderServiceFakeFactory;
        private readonly PactVerifierConfig pactVerifierConfig;
        private const string providerId = "dii-theatermanagement-bff";
        public DiiTheaterManagementBffProviderPactTests(ITestOutputHelper testOutputHelper,
            CustomWebApplicationFactory<Startup> factory
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

        [Fact(Skip = "To do Later")]
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

            using (var inMemoryReverseProxy_OrderingSvc = new InMemoryReverseProxy(httpClientForInMemoryInstanceOfOrderingSvcApp))
            using (var inMemoryReverseProxy = new InMemoryReverseProxy(httpClientForInMemoryInstanceOfApp))
            {
                string ProviderStateBase = inMemoryReverseProxy_OrderingSvc.LocalhostAddress;
                string providerBase = inMemoryReverseProxy.LocalhostAddress;
                Startup.OrderingHttpClientBaseAddress = ProviderStateBase;

                IPactVerifier pactVerifier = new PactVerifier(pactVerifierConfig);
                pactVerifier.ProviderState($"{ProviderStateBase}/provider-states")
                    .ServiceProvider(providerId, providerBase)
                    .HonoursPactWith(consumerId)
 //.PactUri(absolutePathToPactFile)
 //.PactBroker(
 //    "https://onbe.pactflow.io",
 //    consumerVersionSelectors: new List<VersionTagSelector>
 //    {
 //        new VersionTagSelector("stagesite", latest: true),
 //       // new VersionTagSelector("production", latest: true)
 //    })
                    .PactUri($"https://onbe.pactflow.io/pacts/provider/{providerId}/consumer/{consumerId}/latest", new PactUriOptions("4pYBeAABs7e_1rTC98q07w"))

                    .Verify();


            }
        }
    }
}
