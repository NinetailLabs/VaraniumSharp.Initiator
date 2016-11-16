using Serilog;
using VaraniumSharp.Extensions;
using VaraniumSharp.Initiator.Enumerations;

namespace VaraniumSharp.Initiator.Configuration
{
    /// <summary>
    /// Configuration for Seq sink
    /// </summary>
    public class SeqLoggingConfiguration : BaseLogConfiguration
    {
        #region Properties

        /// <summary>
        /// Should log be written to Seq
        /// </summary>
        public bool LogToSeq => ConfigurationKeys.LogToSeq.GetConfigurationValue<bool>();

        /// <summary>
        /// Url for the Seq host
        /// </summary>
        public string SeqHost => ConfigurationKeys.SeqHost.GetConfigurationValue<string>();

        #endregion

        #region Private Methods

        /// <summary>
        /// Apply the Seq log configuration
        /// </summary>
        /// <param name="serilogConfiguration"></param>
        protected override void LogSetup(LoggerConfiguration serilogConfiguration)
        {
            if (!LogToSeq)
            {
                IsActive = false;
                return;
            }
            serilogConfiguration.WriteTo.Seq(SeqHost);
            IsActive = true;
        }

        #endregion
    }
}