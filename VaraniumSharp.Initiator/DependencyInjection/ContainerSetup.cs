using DryIoc;
using System;
using System.Collections.Generic;
using System.Reflection;
using VaraniumSharp.Attributes;
using VaraniumSharp.DependencyInjection;
using VaraniumSharp.Initiator.Attributes;

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
        public override TService Resolve<TService>()
        {
            return _container.Resolve<TService>();
        }

        /// <summary>
        /// Resolve Services from the container via a shared interface of parent class
        /// </summary>
        /// <typeparam name="TService">Interface or parent class that children are registered under</typeparam>
        /// <returns>Collection of children classes that inherit from the parent or implement the interface</returns>
        public override IEnumerable<TService> ResolveMany<TService>()
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

                var transientDisposalAttribute =
                    (DisposableTransientAttribute)
                    @class.GetCustomAttribute(typeof(DisposableTransientAttribute));

                if (transientDisposalAttribute != null)
                {
                    RegisterDisposableTransient(@class, registrationAttribute);
                }
                else if (registrationAttribute.MultipleConstructors)
                {
                    _container.Register(registrationAttribute.ServiceType, @class,
                        registrationAttribute.Reuse.ConvertFromVaraniumReuse(),
                        FactoryMethod.ConstructorWithResolvableArguments);
                }
                else
                {
                    _container.Register(registrationAttribute.ServiceType, @class,
                        registrationAttribute.Reuse.ConvertFromVaraniumReuse());
                }
            }
        }

        /// <summary>
        /// Register disposable transient
        /// </summary>
        /// <param name="class">The class to register</param>
        /// <param name="registrationAttribute">The registration attribute details for the class</param>
        private void RegisterDisposableTransient(Type @class,
            AutomaticContainerRegistrationAttribute registrationAttribute)
        {
            _container.Register(registrationAttribute.ServiceType, @class,
                registrationAttribute.Reuse.ConvertFromVaraniumReuse(),
                FactoryMethod.ConstructorWithResolvableArguments, Setup.With(allowDisposableTransient: true));
        }

        /// <inheritdoc />
        protected override void RegisterConcretionClasses()
        {
            foreach (var @class in ConcretionClassesToRegister)
            {
                var registrationAttribute =
                    (AutomaticConcretionContainerRegistrationAttribute)
                    @class.Key.GetCustomAttribute(typeof(AutomaticConcretionContainerRegistrationAttribute));
                @class.Value.ForEach(x =>
                {
                    if (registrationAttribute.MultipleConstructors)
                    {
                        _container.RegisterMany(new[] { @class.Key, x }, x,
                            registrationAttribute.Reuse.ConvertFromVaraniumReuse(),
                            FactoryMethod.ConstructorWithResolvableArguments);
                    }
                    else
                    {
                        _container.RegisterMany(new[] { @class.Key, x }, x,
                            registrationAttribute.Reuse.ConvertFromVaraniumReuse());
                    }
                });
            }
        }

        #endregion Protected Methods
    }
}