using System;
using System.Configuration;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Serilog;
using VaraniumSharp.Initiator.Configuration;

namespace VaraniumSharp.Initiator.Tests.Configuration
{
    public class ConsoleLoggingConfigurationTest
    {
        [Test]
        public void ApplyLoggingConfiguration()
        {
            // arrange
            AdjustKeys("log.console", "true");
            var serilogConfiguration = new Mock<LoggerConfiguration>();
            var sut = new ConsoleLoggingConfiguration();

            // act
            sut.Apply(serilogConfiguration.Object);

            // assert
            sut.LogToConsole.Should().BeTrue();
            sut.IsActive.Should().BeTrue();
            sut.WasApplied.Should().BeTrue();
        }

        [Test]
        public void ApplyLoggingConfigurationWhenNotUsed()
        {
            // arrange
            AdjustKeys("log.console", "false");
            var serilogConfiguration = new Mock<LoggerConfiguration>();
            var sut = new ConsoleLoggingConfiguration();

            // act
            sut.Apply(serilogConfiguration.Object);

            // assert
            sut.LogToConsole.Should().BeFalse();
            sut.IsActive.Should().BeFalse();
            sut.WasApplied.Should().BeTrue();
        }

        [Test]
        public void ConfigurationCannotBeAppliedTwice()
        {
            // arrange
            var serilogConfiguration = new Mock<LoggerConfiguration>();
            var sut = new ConsoleLoggingConfiguration();
            var action = new Action(() => sut.Apply(serilogConfiguration.Object));
            sut.Apply(serilogConfiguration.Object);

            // act
            // assert
            action.ShouldThrow<InvalidOperationException>();
        }

        private static void AdjustKeys(string keyname, string keyvalue)
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var configFile = System.IO.Path.Combine(appPath, "VaraniumSharp.Initiator.Tests.dll.config");
            var configFileMap = new ExeConfigurationFileMap {ExeConfigFilename = configFile};
            var config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            config.AppSettings.Settings[keyname].Value = keyvalue;
            config.Save();
            ConfigurationManager.RefreshSection("appSettings");
        }

    }
}