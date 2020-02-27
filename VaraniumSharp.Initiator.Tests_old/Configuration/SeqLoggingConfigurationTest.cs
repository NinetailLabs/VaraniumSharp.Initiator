using FluentAssertions;
using Moq;
using NUnit.Framework;
using Serilog;
using VaraniumSharp.Initiator.Configuration;
using VaraniumSharp.Initiator.Tests.Helpers;

namespace VaraniumSharp.Initiator.Tests.Configuration
{
    public class SeqLoggingConfigurationTest
    {
        #region Public Methods

        [TestCase(true, true, true)]
        [TestCase(false, false, true)]
        public void ApplyLoggingConfiguration(bool isUsed, bool isActive, bool wasApplied)
        {
            // arrange
            ApplicationConfigurationHelper.AdjustKeys("log.seq", isUsed.ToString());
            var serilogConfigurationDummy = new Mock<LoggerConfiguration>();
            var sut = new SeqLoggingConfiguration();

            // act
            sut.Apply(serilogConfigurationDummy.Object);

            // assert
            sut.SeqHost.Should().Be(SeqHost);
            sut.LogToSeq.Should().Be(isUsed);
            sut.IsActive.Should().Be(isActive);
            sut.WasApplied.Should().Be(wasApplied);
        }

        #endregion

        #region Variables

        private const string SeqHost = "https://seqhost.com";

        #endregion
    }
}