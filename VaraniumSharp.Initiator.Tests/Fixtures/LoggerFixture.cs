using Moq;
using Serilog;

namespace VaraniumSharp.Initiator.Tests.Fixtures
{
    public static class LoggerFixture
    {
        #region Public Methods

        /// <summary>
        /// Set up Log with a fixtured ILogger so log writing can be interrogated
        /// </summary>
        /// <returns>Tuple containing the original ILogger and the new mocked ILogger</returns>
        public static Mock<ILogger> SetupLogCatcher()
        {
            var loggerFixture = new Mock<ILogger>();
            loggerFixture.Setup(t => t.ForContext("Module", It.IsAny<string>(), false)).Returns(loggerFixture.Object);
            Log.Logger = loggerFixture.Object;
            return loggerFixture;
        }

        #endregion
    }
}