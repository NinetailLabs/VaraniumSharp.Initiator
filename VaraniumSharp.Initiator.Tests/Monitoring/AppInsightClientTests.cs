using FluentAssertions;
using HttpMockSlim;
using Microsoft.ApplicationInsights.DataContracts;
using Moq;
using NUnit.Framework;
using Serilog;
using System;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using VaraniumSharp.Initiator.Monitoring;
using VaraniumSharp.Initiator.Tests.Fixtures;

namespace VaraniumSharp.Initiator.Tests.Monitoring
{
    public class AppInsightClientTests
    {
        #region Public Methods

        [Test]
        public void DeviceInformationIsCorrectlyRetrieved()
        {
            // arrange

            const string computerSystem = "Win32_ComputerSystem";
            const string unknown = "Unknown";

            var oem = RetrieveValueFromManagementInformation("Manufacturer", computerSystem, unknown);
            var model = RetrieveValueFromManagementInformation("Model", computerSystem, unknown);
            var expectedValue = $"{oem} {model}";

            // act
            // assert
            AppInsightClient.DeviceDetails.Should().Be(expectedValue);
        }

        [Test]
        public void ExternalDependencyCallEventIsPublishedCorrectly()
        {
            // arrange
            using (var httpDummy = new HttpMockFixture())
            {
                httpDummy.SetupServer();

                // act
                AppInsightClient.TrackDependency("Test", "testCommand", DateTimeOffset.Now, TimeSpan.Zero, true);
                AppInsightClient.Flush();

                // assert
                httpDummy.PathWasCalled.Should().BeTrue();
            }
        }

        [Test]
        public async Task InitializationCanOccurOnlyOnce()
        {
            // arrange
            // act
            await AppInsightClient.InitializeAsync(TestKey, TestUserKey);

            // assert
            AppInsightClient.IsInitialized.Should().BeTrue();
            _logMock.Verify(t => t.Warning("Client can only be initialized once"), Times.Once);
        }

        [Test]
        public void InitializationWorksCorrectly()
        {
            // arrange
            // act
            // assert
            AppInsightClient.IsInitialized.Should().BeTrue();
        }

        [Test]
        public void MetricIsNotPublishedIfPublishingIsDisabled()
        {
            // arrange
            using (var httpDummy = new HttpMockFixture())
            {
                httpDummy.SetupServer();
                AppInsightClient.TrackTelemetry = false;

                // act
                AppInsightClient.TrackMetric("Test", 10.5);
                AppInsightClient.Flush();

                // assert
                httpDummy.PathWasCalled.Should().BeFalse();
                AppInsightClient.TrackTelemetry = true;
            }
        }

        [Test]
        public void PublishingAnEventWorksCorrectly()
        {
            // arrange
            using (var httpDummy = new HttpMockFixture())
            {
                httpDummy.SetupServer();

                // act
                AppInsightClient.TrackEvent("Test");
                AppInsightClient.Flush();

                // assert
                httpDummy.PathWasCalled.Should().BeTrue();
            }
        }

        [Test]
        public void PublishingMetricWorksCorrectly()
        {
            // arrange
            using (var httpDummy = new HttpMockFixture())
            {
                httpDummy.SetupServer();

                // act
                AppInsightClient.TrackMetric("Test", 12.2);
                AppInsightClient.Flush();

                // assert
                httpDummy.CallCount.Should().Be(1);
            }
        }

        /*
         * So this is a little... unconventional, however there is no other way to verify
         * that we cannot post prior to AppInsightClient initialization.
         * Should this go wrong when the method executes it will still break testing
         */

        [OneTimeSetUp]
        public async Task SetupWithCheckThatPostCannotOccurPriorToInitialization()
        {
            // arrange
            _logMock = LoggerFixture.SetupLogCatcher();

            // act
            AppInsightClient.TrackEvent("BeforeStart");

            // assert
            _logMock.Verify(t => t.Warning("Cannot track telemetry - Client has not been initialized"), Times.Once);

            await AppInsightClient.InitializeAsync(TestKey, TestUserKey);
        }

        [Test]
        public void TackingATaceWithSeverityLevelWorksCorrectly()
        {
            // arrange
            using (var httpDummy = new HttpMockFixture())
            {
                httpDummy.SetupServer();

                // act
                AppInsightClient.TrackTrace("Test", SeverityLevel.Critical);
                AppInsightClient.Flush();

                // assert
                httpDummy.PathWasCalled.Should().BeTrue();
            }
        }

        [Test]
        public void TelemetryClientCorrectlyRetrievesOperatingSystem()
        {
            // arrange
            var expectedOs = RetrieveValueFromManagementInformation("Caption", "Win32_OperatingSystem", "Unknown");

            // act
            // assert
            AppInsightClient.OperatingSystem.Should().Be(expectedOs);
        }

        [Test]
        public void TelemetryClientGeneratesASessionKey()
        {
            // arrange
            // act
            // assert
            AppInsightClient.SessionKey.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void TelemetryClientKeysAreSetUpCorrectly()
        {
            // arrange
            // act
            // assert
            AppInsightClient.InstrumentationKey.Should().Be(TestKey);
            AppInsightClient.UserKey.Should().Be(TestUserKey);
        }

        [Test]
        public void TrackingATraceWorksCorrectly()
        {
            // arrange
            using (var httpDummy = new HttpMockFixture())
            {
                httpDummy.SetupServer();

                // act
                AppInsightClient.TrackTrace("Test");
                AppInsightClient.Flush();

                // assert
                httpDummy.PathWasCalled.Should().BeTrue();
            }
        }

        [Test]
        public void TrackingExceptionWorksCorrectly()
        {
            // arrange
            using (var httpDummy = new HttpMockFixture())
            {
                httpDummy.SetupServer();

                // act
                AppInsightClient.TrackException(new Exception("Oh no!"));
                AppInsightClient.Flush();

                // assert
                httpDummy.PathWasCalled.Should().BeTrue();
            }
        }

        [Test]
        public void TrackingPageViewWorksCorrectly()
        {
            // arrange
            using (var httpDummy = new HttpMockFixture())
            {
                httpDummy.SetupServer();

                // act
                AppInsightClient.TrackPageView("Test Page");
                AppInsightClient.Flush();

                // assert
                httpDummy.PathWasCalled.Should().BeTrue();
            }
        }

        [Test]
        public void TrackingRequestWorksCorrectly()
        {
            // arrange
            using (var httpDummy = new HttpMockFixture())
            {
                httpDummy.SetupServer();

                // act
                AppInsightClient.TrackRequest("Test", DateTimeOffset.Now, TimeSpan.Zero, "202", true);
                AppInsightClient.Flush();

                // assert
                httpDummy.PathWasCalled.Should().BeTrue();
            }
        }

        [Test]
        public void VersionNumberIsRetrievedCorrectly()
        {
            // arrange
            var expectedVersion = Assembly
                                      .GetEntryAssembly()
                                      ?.GetName()
                                      .Version
                                      .ToString()
                                  ?? "0.0.0.0";

            // act
            // assert
            AppInsightClient.VersionNumber.Should().Be(expectedVersion);
        }

        #endregion

        #region Private Methods

        private static string RetrieveValueFromManagementInformation(string propertyToRetrieve,
            string componentFromWhichToRetrieve, string fallbackValue)
        {
            return new ManagementObjectSearcher($"SELECT {propertyToRetrieve} FROM {componentFromWhichToRetrieve}")
                       .Get()
                       .OfType<ManagementObject>()
                       .Select(x => x.GetPropertyValue(propertyToRetrieve))
                       .FirstOrDefault()
                       ?.ToString()
                   ?? fallbackValue;
        }

        #endregion

        #region Variables

        private const string UrlPath = "/v2/track";

        private const string TestKey = "TestKey";

        private const string TestUserKey = "TestUser";

        private Mock<ILogger> _logMock;

        #endregion

        private class HttpMockFixture : IDisposable
        {
            #region Properties

            public int CallCount { get; private set; }

            public bool PathWasCalled { get; private set; }

            #endregion

            #region Public Methods

            public void Dispose()
            {
                _httpMock.Stop();
            }

            public void SetupServer()
            {
                const string url = "http://localhost:8888/";

                _httpMock = new HttpMock();
                _httpMock.Start(url);
                _httpMock.Add("POST", UrlPath,
                    (request, response) =>
                    {
                        PathWasCalled = true;
                        CallCount++;
                        response.StatusCode = (int)HttpStatusCode.OK;
                    });
            }

            #endregion

            #region Variables

            private HttpMock _httpMock;

            #endregion
        }
    }
}