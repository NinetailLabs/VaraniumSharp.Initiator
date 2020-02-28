using System.Reflection;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Serilog;
using VaraniumSharp.Extensions;
using VaraniumSharp.Initiator.Configuration;
using VaraniumSharp.Initiator.Tests.Helpers;

namespace VaraniumSharp.Initiator.Tests.Configuration
{
    public class SplunkLoggingConfigurationTest
    {
        #region Public Methods

        [TestCase(true, true, true)]
        [TestCase(false, false, true)]
        public void ApplyLoggingConfiguration(bool isUsed, bool isActive, bool wasApplied)
        {
            // arrange
            StringExtensions.ConfigurationLocation = Assembly.GetExecutingAssembly().Location;
            ApplicationConfigurationHelper.AdjustKeys("log.splunk", isUsed.ToString());
            var serilogConfigurationDummy = new Mock<LoggerConfiguration>();
            var sut = new SplunkLoggingConfiguration();

            // act
            sut.Apply(serilogConfigurationDummy.Object);

            // assert
            sut.SplunkHost.Should().Be(SplunkHost);
            sut.SplunkToken.Should().Be(SplunkToken);
            sut.LogToSplunk.Should().Be(isUsed);
            sut.IsActive.Should().Be(isActive);
            sut.WasApplied.Should().Be(wasApplied);
        }

        #endregion

        #region Variables

        private const string SplunkHost = "https://mysplunk:8088/services/collector";
        private const string SplunkToken = "ReplaceWithToken";

        #endregion
    }
}