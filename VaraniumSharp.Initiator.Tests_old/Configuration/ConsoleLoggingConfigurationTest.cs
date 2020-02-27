using FluentAssertions;
using Moq;
using NUnit.Framework;
using Serilog;
using System;
using VaraniumSharp.Initiator.Configuration;
using VaraniumSharp.Initiator.Tests.Helpers;

namespace VaraniumSharp.Initiator.Tests.Configuration
{
    public class ConsoleLoggingConfigurationTest
    {
        #region Public Methods

        [TestCase(true, true, true)]
        [TestCase(false, false, true)]
        public void ApplyLoggingConfiguration(bool isUsed, bool isActive, bool wasApplied)
        {
            // arrange
            ApplicationConfigurationHelper.AdjustKeys("log.console", isUsed.ToString());
            var serilogConfigurationDummy = new Mock<LoggerConfiguration>();
            var sut = new ConsoleLoggingConfiguration();

            // act
            sut.Apply(serilogConfigurationDummy.Object);

            // assert
            sut.LogToConsole.Should().Be(isUsed);
            sut.IsActive.Should().Be(isActive);
            sut.WasApplied.Should().Be(wasApplied);
        }

        [Test]
        public void ConfigurationCannotBeAppliedTwice()
        {
            // arrange
            var serilogConfigurationDummy = new Mock<LoggerConfiguration>();
            var sut = new ConsoleLoggingConfiguration();
            var action = new Action(() => sut.Apply(serilogConfigurationDummy.Object));
            sut.Apply(serilogConfigurationDummy.Object);

            // act
            // assert
            action.ShouldThrow<InvalidOperationException>();
        }

        #endregion
    }
}