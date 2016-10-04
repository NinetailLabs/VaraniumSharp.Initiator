using Serilog;
using VaraniumSharp.Initiator.Configuration;

namespace VaraniumSharp.Initiator.ServiceSetup
{
    //TODO - Add summaries
    public static class LoggingExtension
    {
        public static void SetupLogging(this LoggerConfiguration serilogConfig, LogConfiguration configuration)
        {
            if (configuration.LogToConsole)
            {
                serilogConfig.WriteTo.ColoredConsole();
            }
            if (configuration.LogToFile)
            {
                serilogConfig.WriteTo.File(configuration.LogFilePath);
            }
            if (configuration.LogToSplunk)
            {
                serilogConfig.WriteTo.EventCollector(configuration.SplunkHost, configuration.SplunkToken);
            }
            Log.Logger = serilogConfig.CreateLogger();
        }
    }
}