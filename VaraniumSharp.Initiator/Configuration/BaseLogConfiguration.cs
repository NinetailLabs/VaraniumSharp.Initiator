using System;
using Serilog;

namespace VaraniumSharp.Initiator.Configuration
{
    public abstract class BaseLogConfiguration
    {
        #region Properties

        public bool IsActive { get; protected set; }
        public bool WasApplied { get; private set; }

        #endregion

        #region Public Methods

        public void Apply(LoggerConfiguration serilogConfiguration)
        {
            lock (_applyLock)
            {
                if (WasApplied)
                {
                    throw new InvalidOperationException($"Cannot reapply configuration again, it has already been applied");
                }

                LogSetup(serilogConfiguration);

                WasApplied = true;
            }
        }

        #endregion

        #region Private Methods

        protected abstract void LogSetup(LoggerConfiguration serilogConfiguration);

        #endregion

        #region Variables

        private readonly object _applyLock = new object();

        #endregion
    }
}