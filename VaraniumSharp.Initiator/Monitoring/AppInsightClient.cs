using Microsoft.ApplicationInsights;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace VaraniumSharp.Initiator.Monitoring
{
    /// <summary>
    /// Wraps the Application Insight telemetry client in a static instance so it can be easily used from anywhere
    /// </summary>
    public static class AppInsightClient
    {
        #region Constructor

        /// <summary>
        /// Static Constructor
        /// </summary>
        static AppInsightClient()
        {
            LogInstance = Log.Logger.ForContext("Module", nameof(AppInsightClient));
            TrackTelemetry = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get the device Manufacturer and Model that TelemetryClient will report
        /// </summary>
        public static string DeviceDetails =>
            $"{_telemetryClient?.Context.Device.OemName} {_telemetryClient?.Context.Device.Model}";

        /// <summary>
        /// Instrumentatin Key value assigned to the TelemetryClient
        /// </summary>
        public static string InstrumentationKey => _telemetryClient?.InstrumentationKey;

        /// <summary>
        /// Indicate if the AppInsightClient has already been initialized
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// The OS that the TelemetryClient will report
        /// </summary>
        public static string OperatingSystem => _telemetryClient?.Context.Device.OperatingSystem;

        /// <summary>
        /// Session key value that has been created for this session
        /// </summary>
        public static string SessionKey => _telemetryClient?.Context.Session.Id;

        /// <summary>
        /// Enable telemetry tracking
        /// </summary>
        public static bool TrackTelemetry { get; set; }

        /// <summary>
        /// User key value assigned to the TelemetryClient
        /// </summary>
        public static string UserKey => _telemetryClient?.Context.User.AuthenticatedUserId;

        #endregion

        #region Public Methods

        /// <summary>
        /// Flushes the in-memory buffer
        /// </summary>
        public static void Flush()
        {
            _telemetryClient.Flush();
        }

        /// <summary>
        /// Initialize the Telemetry client with appropriate settings
        /// </summary>
        /// <param name="instrumentationKey">Application Insight instrumentation key</param>
        /// <param name="userKey">Key that uniquely identify the user</param>
        public static async Task InitializeAsync(string instrumentationKey, string userKey)
        {
            try
            {
                await StartupLock.WaitAsync();
                if (IsInitialized)
                {
                    LogInstance.Warning("Client can only be initialized once");
                    return;
                }
                _telemetryClient = new TelemetryClient
                {
                    InstrumentationKey = instrumentationKey
                };

                _telemetryClient.Context.User.AuthenticatedUserId = userKey;
                _telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
                _telemetryClient.Context.Device.OperatingSystem = GetWindowsFriendlyName();
                _telemetryClient.Context.Device.Model = GetDeviceModel();
                _telemetryClient.Context.Device.OemName = GetDeviceManufacturer();
                _telemetryClient.Context.Component.Version = GetComponentVersion();
                IsInitialized = true;
            }
            finally
            {
                StartupLock.Release();
            }
        }

        /// <summary>
        /// Logging the duration and frequency of calls to external components that your app depends on.
        /// </summary>
        /// <param name="dependencyName">Name of the external dependency</param>
        /// <param name="commandName">Dependency call command name</param>
        /// <param name="startTime">Time when dependency was called</param>
        /// <param name="duration">Time taken by dependency to handle request</param>
        /// <param name="success">Was the call handled successfully</param>
        public static void TrackDependency(string dependencyName, string commandName, DateTimeOffset startTime,
            TimeSpan duration, bool success)
        {
            if (TelemetryCanBePosted())
            {
                _telemetryClient.TrackDependency(dependencyName, commandName, startTime, duration, success);
            }
        }

        /// <summary>
        /// User actions and other events. Used to track user behavior or to monitor performance.
        /// </summary>
        /// <param name="name">Name of the event</param>
        /// <param name="properties">Dictionary of event properties</param>
        /// <param name="metrics">Dictionary of event metrics</param>
        public static void TrackEvent(string name, IDictionary<string, string> properties = null,
            IDictionary<string, double> metrics = null)
        {
            if (TelemetryCanBePosted())
            {
                _telemetryClient.TrackEvent(name, properties, metrics);
            }
        }

        /// <summary>
        /// Logging exceptions for diagnosis. Trace where they occur in relation to other events and examine stack traces.
        /// </summary>
        /// <param name="exception">Exception that occured</param>
        /// <param name="properties">Named string values that can be used to search for exception</param>
        /// <param name="metrics">Additional values associated with exception</param>
        public static void TrackException(Exception exception, IDictionary<string, string> properties = null,
            IDictionary<string, double> metrics = null)
        {
            if (TelemetryCanBePosted())
            {
                _telemetryClient.TrackException(exception, properties, metrics);
            }
        }

        /// <summary>
        /// Performance measurements such as queue lengths not related to specific events.
        /// </summary>
        /// <param name="name">Metric name</param>
        /// <param name="value">Metric value</param>
        public static void TrackMetric(string name, double value)
        {
            if (TelemetryCanBePosted())
            {
                _telemetryClient.TrackMetric(name, value);
            }
        }

        /// <summary>
        /// Pages, screens, blades, or forms.
        /// </summary>
        /// <param name="name">Name of the page</param>
        public static void TrackPageView(string name)
        {
            if (TelemetryCanBePosted())
            {
                _telemetryClient.TrackPageView(name);
            }
        }

        /// <summary>
        /// Logging the frequency and duration of server requests for performance analysis.
        /// </summary>
        /// <param name="name">Request name</param>
        /// <param name="startTime">Time when request was started</param>
        /// <param name="duration">Time taken by application to handle request</param>
        /// <param name="responseCode">Response status code</param>
        /// <param name="success">True if request was handled successfully</param>
        public static void TrackRequest(string name, DateTimeOffset startTime, TimeSpan duration, string responseCode,
            bool success)
        {
            if (TelemetryCanBePosted())
            {
                _telemetryClient.TrackRequest(name, startTime, duration, responseCode, success);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get the version of the Assembly
        /// </summary>
        /// <returns>Assembly version number</returns>
        private static string GetComponentVersion()
        {
            return Assembly
                       .GetEntryAssembly()
                       ?.GetName()
                       .Version
                       .ToString()
                   ?? "0.0.0.0";
        }

        /// <summary>
        /// Get the device manufacturer from Management Information
        /// </summary>
        /// <returns>Device manufacturer if it can be found</returns>
        private static string GetDeviceManufacturer()
        {
            const string manufacturer = "Manufacturer";
            return RetrieveValueFromManagementInformation(manufacturer, ComputerSystem, UnknownValue);
        }

        /// <summary>
        /// Get the device Model from Management Information
        /// </summary>
        /// <returns>Device manufacturer if it can be found</returns>
        private static string GetDeviceModel()
        {
            const string model = "Model";
            return RetrieveValueFromManagementInformation(model, ComputerSystem, UnknownValue);
        }

        /// <summary>
        /// Retrieve the Windows friendly name instead of just a version
        /// </summary>
        /// <returns></returns>
        private static string GetWindowsFriendlyName()
        {
            const string caption = "Caption";
            const string component = "Win32_OperatingSystem";
            var fallback = Environment.OSVersion.ToString();

            return RetrieveValueFromManagementInformation(caption, component, fallback);
        }

        /// <summary>
        /// Retrieve an entry from ManagementInformation
        /// </summary>
        /// <param name="propertyToRetrieve">The property to retrieve</param>
        /// <param name="componentFromWhichToRetrieve">The component from which the value should be retrieved</param>
        /// <param name="fallbackValue">Value to return if property cannot be found</param>
        /// <returns>Result from the ManagementInformation query</returns>
        private static string RetrieveValueFromManagementInformation(string propertyToRetrieve,
            string componentFromWhichToRetrieve, string fallbackValue)
        {
            return new ManagementObjectSearcher($"SELECT {propertyToRetrieve} FROM {componentFromWhichToRetrieve}")
                       .Get()
                       .OfType<ManagementObject>()
                       .Select(x => x.GetPropertyValue(propertyToRetrieve))
                       .FirstOrDefault()
                       ?.ToString()
                   ?? fallbackValue;
        }

        /// <summary>
        /// Check if we can post Telemetry data.
        /// This method checks if the client has been initialized and if posting of telemetry data is allowed
        /// </summary>
        /// <returns>True - Telemetry data can be posted</returns>
        private static bool TelemetryCanBePosted()
        {
            var canPost = true;
            if (!IsInitialized)
            {
                LogInstance.Warning("Cannot track telemetry - Client has not been initialized");
                canPost = false;
            }
            if (!TrackTelemetry)
            {
                LogInstance.Verbose("Telemetry data posting is disabled");
                canPost = false;
            }
            return canPost;
        }

        #endregion

        #region Variables

        /// <summary>
        /// Win32_ComputerSystem string
        /// </summary>
        private const string ComputerSystem = "Win32_ComputerSystem";

        /// <summary>
        /// Constant value "Unknown"
        /// </summary>
        private const string UnknownValue = "Unknown";

        /// <summary>
        /// Semaphore used to lock initialization
        /// </summary>
        private static readonly SemaphoreSlim StartupLock = new SemaphoreSlim(1);

        /// <summary>
        /// Application Insight TelemetryClient
        /// </summary>
        private static TelemetryClient _telemetryClient;

        /// <summary>
        /// Log instance for the class
        /// </summary>
        private static readonly ILogger LogInstance;

        #endregion
    }
}