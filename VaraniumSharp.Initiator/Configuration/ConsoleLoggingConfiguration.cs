using Serilog;
using VaraniumSharp.Extensions;
using VaraniumSharp.Initiator.Enumerations;

namespace VaraniumSharp.Initiator.Configuration
{
    /// <summary>
    /// Configuration for ColoredConsole sink
    /// </summary>
    public sealed class ConsoleLoggingConfiguration : BaseLogConfiguration
    {
        #region Properties

        /// <summary>
        /// Should the log be written to the console
        /// </summary>
        public bool LogToConsole => ConfigurationKeys.LogToConsole.GetConfigurationValue<bool>();

        #endregion Properties

        #region Private Methods

        /// <summary>
        /// Apply the Console log configuration
        /// </summary>
        /// <param name="serilogConfiguration"></param>
        protected override void LogSetup(LoggerConfiguration serilogConfiguration)
        {
            if (!LogToConsole)
            {
                IsActive = false;
                return;
            }
            serilogConfiguration.WriteTo.ColoredConsole();
            IsActive = true;
        }

        #endregion Private Methods
    }
}