using FluentAssertions;
using Moq;
using NUnit.Framework;
using Serilog;
using VaraniumSharp.Initiator.Configuration;
using VaraniumSharp.Initiator.Tests.Helpers;

namespace VaraniumSharp.Initiator.Tests.Configuration
{
    public class FileLoggingConfigurationTest
    {
        [TestCase(true, true, true)]
        [TestCase(false, false, true)]
        public void ApplyLoggingConfiguration(bool isUsed, bool isActive, bool wasApplied)
        {
            // arrange
            ApplicationConfigurationHelper.AdjustKeys("log.file", isUsed.ToString());
            var serilogConfigurationDummy = new Mock<LoggerConfiguration>();
            var sut = new FileLoggingConfiguration();

            // act
            sut.Apply(serilogConfigurationDummy.Object);

            // assert
            sut.LogFilePath.Should().Be(FilePath);
            sut.LogToFile.Should().Be(isUsed);
            sut.IsActive.Should().Be(isActive);
            sut.WasApplied.Should().Be(wasApplied);
        }

        private const string FilePath = "log.txt";
    }
}