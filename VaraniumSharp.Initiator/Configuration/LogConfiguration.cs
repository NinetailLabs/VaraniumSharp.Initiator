using VaraniumSharp.Extensions;
using VaraniumSharp.Initiator.Enumerations;

namespace VaraniumSharp.Initiator.Configuration
{
    /// <summary>
    /// Configuration used to set up logging
    /// </summary>
    public class LogConfiguration
    {
        
        /// <summary>
        /// Should the log be written to a file
        /// </summary>
        public bool LogToFile => ConfigurationKeys.LogToFile.GetConfigurationValue<bool>();

        /// <summary>
        /// Should the log be written to Splunk
        /// </summary>
        public bool LogToSplunk => ConfigurationKeys.LogToSplunk.GetConfigurationValue<bool>();

        /// <summary>
        /// Path of the file where the log should be written
        /// </summary>
        public string LogFilePath => ConfigurationKeys.LogFilePath.GetConfigurationValue<string>();

        /// <summary>
        /// Splunk host URL
        /// </summary>
        public string SplunkHost => ConfigurationKeys.SplunkHost.GetConfigurationValue<string>();

        /// <summary>
        /// Splunk API token
        /// </summary>
        public string SplunkToken => ConfigurationKeys.SplunkToken.GetConfigurationValue<string>();
    }
}