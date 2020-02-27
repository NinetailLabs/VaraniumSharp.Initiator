using System;
using System.Configuration;

namespace VaraniumSharp.Initiator.Tests.Helpers
{
    public static class ApplicationConfigurationHelper
    {
        #region Public Methods

        /// <summary>
        /// Adjust, save and apply a key in App.Config AppSetting section
        /// </summary>
        /// <param name="keyname"></param>
        /// <param name="keyvalue"></param>
        public static void AdjustKeys(string keyname, string keyvalue)
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var configFile = System.IO.Path.Combine(appPath, "VaraniumSharp.Initiator.Tests.dll.config");
            var configFileMap = new ExeConfigurationFileMap { ExeConfigFilename = configFile };
            var config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            config.AppSettings.Settings[keyname].Value = keyvalue;
            config.Save();
            ConfigurationManager.RefreshSection("appSettings");
        }

        #endregion
    }
}