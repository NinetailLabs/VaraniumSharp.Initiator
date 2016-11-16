using DryIoc;
using System.Collections.Generic;
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

        /// <summary>
        /// Resolve Services from the container via a shared interface of parent class
        /// </summary>
        /// <typeparam name="TService">Interface or parent class that children are registered under</typeparam>
        /// <returns>Collection of children classes that inherit from the parent or implement the interface</returns>
        public IEnumerable<TService> ResolveMany<TService>()
        {
            return _container.ResolveMany<TService>();
        }

        #endregion

        #region Variables

        private readonly IContainer _container;

        #endregion

        #region Protected Methods

        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override void RegisterConcretionClasses()
        {
            foreach (var @class in ConcretionClassesToRegister)
            {
                var registrationAttribute =
                    (AutomaticConcretionContainerRegistrationAttribute)
                    @class.Key.GetCustomAttribute(typeof(AutomaticConcretionContainerRegistrationAttribute));
                @class.Value.ForEach(x => _container.Register(@class.Key, x, registrationAttribute.Reuse.ConvertFromVaraniumReuse()));
            }
        }

        #endregion Protected Methods
    }
}