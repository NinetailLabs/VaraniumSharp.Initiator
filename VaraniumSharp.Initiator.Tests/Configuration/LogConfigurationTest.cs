using FluentAssertions;
using NUnit.Framework;
using VaraniumSharp.Initiator.Configuration;

namespace VaraniumSharp.Initiator.Tests.Configuration
{
    public class LogConfigurationTest
    {
        [Test]
        public void ReadLogConfiguration()
        {
            // arrange
            // act
            var sut = new LogConfiguration();

            // assert
            //sut.LogToConsole.Should().BeTrue();
            sut.LogToFile.Should().BeTrue();
            sut.LogToSplunk.Should().BeTrue();
            sut.LogFilePath.Should().Be(TestLogPath);
            sut.SplunkHost.Should().Be(TestSplunkHost);
            sut.SplunkToken.Should().Be(TestSplunkToken);
        }

        private const string TestLogPath = "log.txt";
        private const string TestSplunkHost = "https://mysplunk:8088/services/collector";
        private const string TestSplunkToken = "ReplaceWithToken";
    }
}