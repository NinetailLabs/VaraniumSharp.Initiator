using DryIoc;
using System.Reflection;
using VaraniumSharp.Attributes;
using VaraniumSharp.DependencyInjection;

namespace VaraniumSharp.Initiator.DependencyInjection
{
    /// <summary>
    /// Set up the DryIoC container and register all classes that implement the AutomaticContainerRegistrationAttribute
    /// </summary>
    public class ContainerSetup : AutomaticContainerRegistration
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public ContainerSetup()
        {
            _container = new Container();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resolve a Service from the Container
        /// </summary>
        /// <typeparam name="TService">Service to resolve</typeparam>
        /// <returns>Resolved service</returns>
        public TService Resolve<TService>()
        {
            return _container.Resolve<TService>();
        }

        #endregion

        #region Private Methods

        #region Protected Methods

        /// <summary>
        /// Automatically register classes with Container
        /// </summary>
        protected override void RegisterClasses()
        {
            foreach (var @class in ClassesToRegister)
            {
                var registrationAttribute =
                    (AutomaticContainerRegistrationAttribute)
                    @class.GetCustomAttribute(typeof(AutomaticContainerRegistrationAttribute));
                _container.Register(registrationAttribute.ServiceType, @class, registrationAttribute.Reuse.ConvertFromVaraniumReuse());
            }
        }

        #endregion Protected Methods

        #endregion

        #region Variables

        private readonly IContainer _container;

        #endregion
    }
}