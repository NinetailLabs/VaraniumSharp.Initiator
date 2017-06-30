using System;
using VaraniumSharp.Attributes;
using VaraniumSharp.Enumerations;
using VaraniumSharp.Initiator.Attributes;

// ReSharper disable UnusedAutoPropertyAccessor.Global - Test fixtures - Required for testing even when not used
// ReSharper disable MemberCanBePrivate.Global - Test fixtures - Access required so they function correctly
// ReSharper disable UnusedMember.Global - Test fixtures used via AutomaticContainerRegistration

namespace VaraniumSharp.Initiator.Tests.Fixtures
{
    [AutomaticConcretionContainerRegistration]
    [DisposableTransient]
    public interface IDisposableConcretionBase : IDisposable
    {
    }

    public class DisposableConcretionImplementation : IDisposableConcretionBase
    {
        #region Public Methods

        public void Dispose()
        {
        }

        #endregion
    }

    public class SecondaryDisposableConcretionImplementation : IDisposableConcretionBase
    {
        #region Public Methods

        public void Dispose()
        {
        }

        #endregion
    }

    [AutomaticContainerRegistration(typeof(DisposableDummy))]
    [DisposableTransient]
    public class DisposableDummy : IDisposable
    {
        #region Public Methods

        public void Dispose()
        {
        }

        #endregion
    }

    [AutomaticContainerRegistration(typeof(AutoRegistrationDummy))]
    public class AutoRegistrationDummy
    {
    }

    [AutomaticContainerRegistration(typeof(SingletonDummy), ServiceReuse.Singleton)]
    public class SingletonDummy
    {
    }

    [AutomaticConcretionContainerRegistration]
    public abstract class BaseClassDummy
    {
    }

    public class InheritorClassDummy : BaseClassDummy
    {
    }

    [AutomaticConcretionContainerRegistration(ServiceReuse.Singleton)]
    public interface ITestInterfaceDummy
    {
    }

    public class ImplementationClassDummy : ITestInterfaceDummy
    {
    }

    public class ImplmentationClassTooDummy : ITestInterfaceDummy
    {
    }

    [AutomaticContainerRegistration(typeof(MultiConstructorClass), ServiceReuse.Default, true)]
    public class MultiConstructorClass
    {
        #region Constructor

        public MultiConstructorClass()
        {
        }

        public MultiConstructorClass(AutoRegistrationDummy autoRegistrationDummy)
        {
            AutoRegistrationDummy = autoRegistrationDummy;
        }

        #endregion

        #region Properties

        public AutoRegistrationDummy AutoRegistrationDummy { get; }

        #endregion
    }

    [AutomaticConcretionContainerRegistration(ServiceReuse.Default, true)]
    public abstract class MultiConstructorConcretionClassDummy
    {
    }

    public class MultiConstructorConcretionInheritor : MultiConstructorConcretionClassDummy
    {
        #region Constructor

        public MultiConstructorConcretionInheritor()
        {
        }

        public MultiConstructorConcretionInheritor(AutoRegistrationDummy autoRegistrationDummy)
        {
            AutoRegistrationDummy = autoRegistrationDummy;
        }

        #endregion

        #region Properties

        public AutoRegistrationDummy AutoRegistrationDummy { get; }

        #endregion
    }
}