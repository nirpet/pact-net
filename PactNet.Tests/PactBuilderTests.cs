﻿using System;
using Newtonsoft.Json;
using NSubstitute;
using PactNet.Configuration.Json;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;
using PactNet.Models;
using Xunit;

namespace PactNet.Tests
{
    public class PactBuilderTests
    {
        private IPactBuilder GetSubject()
        {
            return new PactBuilder();
        }

        [Fact]
        public void ServiceConsumer_WithConsumerName_SetsConsumerName()
        {
            const string consumerName = "My Service Consumer";
            var pactBuilder = GetSubject();

            pactBuilder.ServiceConsumer(consumerName);

            Assert.Equal(consumerName, ((PactBuilder)pactBuilder).ConsumerName);
        }

        [Fact]
        public void ServiceConsumer_WithNullConsumerName_ThrowsArgumentException()
        {
            var pactBuilder = GetSubject();

            Assert.Throws<ArgumentException>(() => pactBuilder.ServiceConsumer(null));
        }

        [Fact]
        public void ServiceConsumer_WithEmptyConsumerName_ThrowsArgumentException()
        {
            var pactBuilder = GetSubject();

            Assert.Throws<ArgumentException>(() => pactBuilder.ServiceConsumer(String.Empty));
        }

        [Fact]
        public void HasPactWith_WithProviderName_SetsProviderName()
        {
            const string providerName = "My Service Provider";
            var pact = GetSubject();

            pact.HasPactWith(providerName);

            Assert.Equal(providerName, ((PactBuilder)pact).ProviderName);
        }

        [Fact]
        public void HasPactWith_WithNullProviderName_ThrowsArgumentException()
        {
            var pactBuilder = GetSubject();

            Assert.Throws<ArgumentException>(() => pactBuilder.HasPactWith(null));
        }

        [Fact]
        public void HasPactWith_WithEmptyProviderName_ThrowsArgumentException()
        {
            var pactBuilder = GetSubject();

            Assert.Throws<ArgumentException>(() => pactBuilder.HasPactWith(String.Empty));
        }

        [Fact]
        public void MockService_WhenCalled_StartIsCalledAndMockProviderServiceIsReturned()
        {
            var mockMockProviderService = Substitute.For<IMockProviderService>();

            IPactBuilder pactBuilder = new PactBuilder((port, enableSsl, consumerName, providerName) => mockMockProviderService);

            pactBuilder
                .ServiceConsumer("Event Client")
                .HasPactWith("Event API");

            var mockProviderService = pactBuilder.MockService(1234);

            mockMockProviderService.Received(1).Start();
            Assert.Equal(mockMockProviderService, mockProviderService);
        }

        [Fact]
        public void MockService_WhenCalledTwice_StopIsCalledTheSecondTime()
        {
            var mockMockProviderService = Substitute.For<IMockProviderService>();

            IPactBuilder pactBuilder = new PactBuilder((port, enableSsl, consumerName, providerName) => mockMockProviderService);

            pactBuilder
                .ServiceConsumer("Event Client")
                .HasPactWith("Event API");

            pactBuilder.MockService(1234);
            mockMockProviderService.Received(0).Stop();

            pactBuilder.MockService(1234);
            mockMockProviderService.Received(1).Stop();
        }

        [Fact]
        public void MockService_WhenCalled_MockProviderServiceFactoryIsInvokedWithSslNotEnabled()
        {
            var calledWithSslEnabled = false;
            var mockMockProviderService = Substitute.For<IMockProviderService>();

            IPactBuilder pactBuilder = new PactBuilder((port, enableSsl, consumerName, providerName) =>
            {
                calledWithSslEnabled = enableSsl;
                return mockMockProviderService;
            });

            pactBuilder
                .ServiceConsumer("Event Client")
                .HasPactWith("Event API");

            pactBuilder.MockService(1234);

            Assert.False(calledWithSslEnabled);
        }

        [Fact]
        public void MockService_WhenCalledWithEnableSslFalse_MockProviderServiceFactoryIsInvokedWithSslNotEnabled()
        {
            var calledWithSslEnabled = false;
            var mockMockProviderService = Substitute.For<IMockProviderService>();

            IPactBuilder pactBuilder = new PactBuilder((port, enableSsl, consumerName, providerName) =>
            {
                calledWithSslEnabled = enableSsl;
                return mockMockProviderService;
            });

            pactBuilder
                .ServiceConsumer("Event Client")
                .HasPactWith("Event API");

            pactBuilder.MockService(1234, false);

            Assert.False(calledWithSslEnabled);
        }

        [Fact]
        public void MockService_WhenCalledWithEnableSslTrue_MockProviderServiceFactoryIsInvokedWithSslEnabled()
        {
            var calledWithSslEnabled = false;
            var mockMockProviderService = Substitute.For<IMockProviderService>();

            IPactBuilder pactBuilder = new PactBuilder((port, enableSsl, consumerName, providerName) =>
            {
                calledWithSslEnabled = enableSsl;
                return mockMockProviderService;
            });

            pactBuilder
                .ServiceConsumer("Event Client")
                .HasPactWith("Event API");

            pactBuilder.MockService(1234, true);

            Assert.True(calledWithSslEnabled);
        }

        [Fact]
        public void MockService_WhenCalledWithJsonSerializerSettings_SetsTheGlobalApiSerializerSettings()
        {
            var serializerSettings = new JsonSerializerSettings();
            var mockMockProviderService = Substitute.For<IMockProviderService>();

            IPactBuilder pactBuilder = new PactBuilder((port, enableSsl, consumerName, providerName) => mockMockProviderService);

            pactBuilder
                .ServiceConsumer("Event Client")
                .HasPactWith("Event API");

            pactBuilder.MockService(1234, serializerSettings);

            Assert.Equal(serializerSettings, JsonConfig.ApiSerializerSettings);

            //Reset the settings
            JsonConfig.ApiSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None
            };
        }

        [Fact]
        public void MockService_WhenCalledWithNoJsonSerializerSettings_DoesNotSetTheGlobalApiSerializerSettingsToNull()
        {
            var mockMockProviderService = Substitute.For<IMockProviderService>();

            IPactBuilder pactBuilder = new PactBuilder((port, enableSsl, consumerName, providerName) => mockMockProviderService);

            pactBuilder
                .ServiceConsumer("Event Client")
                .HasPactWith("Event API");

            pactBuilder.MockService(1234);

            Assert.NotNull(JsonConfig.ApiSerializerSettings);
        }

        [Fact]
        public void MockService_WhenCalledWithoutConsumerNameSet_ThrowsInvalidOperationException()
        {
            IPactBuilder pactBuilder = new PactBuilder((port, ssl, consumerName, providerName) => Substitute.For<IMockProviderService>());
            pactBuilder
                .HasPactWith("Event API");

            Assert.Throws<InvalidOperationException>(() => pactBuilder.MockService(1234));
        }

        [Fact]
        public void MockService_WhenCalledWithoutProviderNameSet_ThrowsInvalidOperationException()
        {
            IPactBuilder pactBuilder = new PactBuilder((port, ssl, consumerName, providerName) => Substitute.For<IMockProviderService>());
            pactBuilder
                .ServiceConsumer("Event Client");

            Assert.Throws<InvalidOperationException>(() => pactBuilder.MockService(1234));
        }

        [Fact]
        public void Build_WhenCalledBeforeTheMockProviderServiceIsInitialised_ThrowsInvalidOperationException()
        {
            IPactBuilder pactBuilder = new PactBuilder(mockProviderServiceFactory: null);

            Assert.Throws<InvalidOperationException>(() => pactBuilder.Build());
        }

        [Fact]
        public void Build_WhenCalledWithTheMockProviderServiceInitialised_CallsSendAdminHttpRequestOnTheMockProviderService()
        {
            const string testConsumerName = "Event Client";
            const string testProviderName = "Event API";
            var mockProviderService = Substitute.For<IMockProviderService>();

            IPactBuilder pactBuilder = new PactBuilder((port, ssl, consumerName, providerName) => mockProviderService);
            pactBuilder
                .ServiceConsumer(testConsumerName)
                .HasPactWith(testProviderName);

            pactBuilder.MockService(1234);

            pactBuilder.Build();

            mockProviderService.Received(1).SendAdminHttpRequest(HttpVerb.Post, Constants.PactPath);
        }

        [Fact]
        public void Build_WhenCalledWithAnInitialisedMockProviderService_StopIsCalledOnTheMockServiceProvider()
        {
            var mockMockProviderService = Substitute.For<IMockProviderService>();

            IPactBuilder pactBuilder = new PactBuilder((port, enableSsl, consumerName, providerName) => mockMockProviderService)
                .ServiceConsumer("Event Client")
                .HasPactWith("Event API");

            pactBuilder.MockService(1234);

            pactBuilder.Build();

            mockMockProviderService.Received(1).Stop();
        }
    }
}
