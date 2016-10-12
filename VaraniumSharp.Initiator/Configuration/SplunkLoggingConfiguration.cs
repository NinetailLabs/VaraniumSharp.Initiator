using Serilog;
using VaraniumSharp.Extensions;
using VaraniumSharp.Initiator.Enumerations;

namespace VaraniumSharp.Initiator.Configuration
{
    /// <summary>
    /// Configuration for Splunk sink
    /// </summary>
    public class SplunkLoggingConfiguration : BaseLogConfiguration
    {
        #region Properties

        /// <summary>
        /// Should the log be written to Splunk
        /// </summary>
        public bool LogToSplunk => ConfigurationKeys.LogToSplunk.GetConfigurationValue<bool>();

        /// <summary>
        /// Splunk host URL
        /// </summary>
        public string SplunkHost => ConfigurationKeys.SplunkHost.GetConfigurationValue<string>();

        /// <summary>
        /// Splunk API token
        /// </summary>
        public string SplunkToken => ConfigurationKeys.SplunkToken.GetConfigurationValue<string>();

        #endregion Properties

        #region Private Methods

        /// <summary>
        /// Apply the Splunk log configuration
        /// </summary>
        /// <param name="serilogConfiguration"></param>
        protected override void LogSetup(LoggerConfiguration serilogConfiguration)
        {
            if (!LogToSplunk)
            {
                IsActive = false;
                return;
            }
            serilogConfiguration.WriteTo.EventCollector(SplunkHost, SplunkHost);
            IsActive = true;
        }

        #endregion Private Methods
    }
}