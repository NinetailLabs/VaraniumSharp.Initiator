namespace VaraniumSharp.Initiator.Enumerations
{
    /// <summary>
    /// Contains keys for reading configuration values from App.config
    /// </summary>
    public static class ConfigurationKeys
    {
        #region Variables

        /// <summary>
        /// Indicate if logging should be done to the console
        /// </summary>
        public const string LogToConsole = "log.console";

        /// <summary>
        /// Indicate if logging should be done to a file
        /// </summary>
        public const string LogToFile = "log.file";

        /// <summary>
        /// Path where log file should be written to
        /// </summary>
        public const string LogFilePath = "log.file.path";

        /// <summary>
        /// Indicate if logging should be done to Seq
        /// </summary>
        public const string LogToSeq = "log.seq";

        /// <summary>
        /// Url of the Seq host
        /// </summary>
        public const string SeqHost = "seq.host";

        /// <summary>
        /// Indicate if logging should be done to Splunk
        /// </summary>
        public const string LogToSplunk = "log.splunk";

        /// <summary>
        /// Url of the Splunk Event Collector
        /// </summary>
        public const string SplunkHost = "log.splunk.host";

        /// <summary>
        /// Event Collector token for Splunk
        /// </summary>
        public const string SplunkToken = "log.splunk.token";

        #endregion
    }
}