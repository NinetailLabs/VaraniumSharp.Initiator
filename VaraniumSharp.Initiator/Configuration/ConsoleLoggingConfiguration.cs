using Serilog;
using VaraniumSharp.Extensions;
using VaraniumSharp.Initiator.Enumerations;

namespace VaraniumSharp.Initiator.Configuration
{
    public sealed class ConsoleLoggingConfiguration : BaseLogConfiguration
    {
        #region Properties

        /// <summary>
        /// Should the log be written to the console
        /// </summary>
        public bool LogToConsole => ConfigurationKeys.LogToConsole.GetConfigurationValue<bool>();

        #endregion Properties

        #region Private Methods

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