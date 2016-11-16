using Serilog;
using VaraniumSharp.Extensions;
using VaraniumSharp.Initiator.Enumerations;

namespace VaraniumSharp.Initiator.Configuration
{
    /// <summary>
    /// Configuration for File sink
    /// </summary>
    public class FileLoggingConfiguration : BaseLogConfiguration
    {
        #region Properties

        /// <summary>
        /// Path of the file where the log should be written
        /// </summary>
        public string LogFilePath => ConfigurationKeys.LogFilePath.GetConfigurationValue<string>();

        /// <summary>
        /// Should the log be written to a file
        /// </summary>
        public bool LogToFile => ConfigurationKeys.LogToFile.GetConfigurationValue<bool>();

        #endregion

        #region Private Methods

        /// <summary>
        /// Apply the file log configuration
        /// </summary>
        /// <param name="serilogConfiguration"></param>
        protected override void LogSetup(LoggerConfiguration serilogConfiguration)
        {
            if (!LogToFile)
            {
                IsActive = false;
                return;
            }
            serilogConfiguration.WriteTo.File(LogFilePath);
            IsActive = true;
        }

        #endregion
    }
}