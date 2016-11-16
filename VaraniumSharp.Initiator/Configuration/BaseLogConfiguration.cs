using Serilog;
using System;
using VaraniumSharp.Attributes;
using VaraniumSharp.Enumerations;

namespace VaraniumSharp.Initiator.Configuration
{
    /// <summary>
    /// Base class for logging configuration
    /// </summary>
    [AutomaticConcretionContainerRegistration(ServiceReuse.Singleton)]
    public abstract class BaseLogConfiguration
    {
        #region Properties

        /// <summary>
        /// Is the sink used
        /// </summary>
        public bool IsActive { get; protected set; }

        /// <summary>
        /// Indicate if the configuration has already been applied
        /// </summary>
        public bool WasApplied { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Apply the configuration.
        /// A configuration can only be applied once
        /// </summary>
        /// <param name="serilogConfiguration">Serilog Configuration on which the logger should be activated</param>
        public void Apply(LoggerConfiguration serilogConfiguration)
        {
            lock (_applyLock)
            {
                if (WasApplied)
                {
                    throw new InvalidOperationException("Cannot reapply configuration again, it has already been applied");
                }

                LogSetup(serilogConfiguration);

                WasApplied = true;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This method is called during the apply method.
        /// It should be overwritten and the appropriate configuration applied
        /// </summary>
        /// <param name="serilogConfiguration"></param>
        protected abstract void LogSetup(LoggerConfiguration serilogConfiguration);

        #endregion

        #region Variables

        private readonly object _applyLock = new object();

        #endregion
    }
}