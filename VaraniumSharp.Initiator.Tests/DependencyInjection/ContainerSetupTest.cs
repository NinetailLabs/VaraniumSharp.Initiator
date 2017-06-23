using System;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using VaraniumSharp.Attributes;
using VaraniumSharp.Enumerations;
using VaraniumSharp.Initiator.Attributes;
using VaraniumSharp.Initiator.DependencyInjection;

namespace VaraniumSharp.Initiator.Tests.DependencyInjection
{
    public class ContainerSetupTest
    {
        #region Public Methods

        [Test]
        public void ClassWithMultipleConstructorsIsRegisteredCorrectly()
        {
            // arrange
            var sut = new ContainerSetup();
            var act = new Action(() => sut.RetrieveClassesRequiringRegistration(true));

            // act
            // assert
            act.ShouldNotThrow<Exception>();
        }

        [Test]
        public void ConcretionClassesAreResolvedCorrectly()
        {
            // arrange
            var sut = new ContainerSetup();

            // act
            sut.RetrieveConcretionClassesRequiringRegistration(true);

            // assert
            var resolvedClass = sut.ResolveMany<BaseClassDummy>().ToList();
            resolvedClass.Count.Should().Be(1);
            resolvedClass.First().GetType().Should().Be(typeof(InheritorClassDummy));
        }

        [Test]
        public void ConcretionClassesCorrectlyApplyReuse()
        {
            // arrange
            var sut = new ContainerSetup();

            // act
            sut.RetrieveConcretionClassesRequiringRegistration(true);

            // assert
            var resolvedClasses = sut.ResolveMany<ITestInterfaceDummy>();
            var secondResolve = sut.ResolveMany<ITestInterfaceDummy>();
            resolvedClasses.ShouldAllBeEquivalentTo(secondResolve);
        }

        [Test]
        public void ConcretionClassesFromInterfaceAreCorrectlyResolved()
        {
            // arrange
            var sut = new ContainerSetup();

            // act
            sut.RetrieveConcretionClassesRequiringRegistration(true);

            // assert
            var resolvedClasses = sut.ResolveMany<ITestInterfaceDummy>().ToList();
            resolvedClasses.Count.Should().Be(2);
            resolvedClasses.Should().Contain(x => x.GetType() == typeof(ImplementationClassDummy));
            resolvedClasses.Should().Contain(x => x.GetType() == typeof(ImplmentationClassTooDummy));
        }

        [Test]
        public void ConcretionClassWithMultipleConstructorsIsRegisteredCorrectly()
        {
            // arrange
            var sut = new ContainerSetup();
            var act = new Action(() => sut.RetrieveConcretionClassesRequiringRegistration(true));

            // act
            // assert
            act.ShouldNotThrow<Exception>();
        }

        [Test]
        public void MultiTypeRegistrationSingletonsWorkCorrectly()
        {
            // arrange
            var sut = new ContainerSetup();

            // act
            sut.RetrieveConcretionClassesRequiringRegistration(true);

            // assert
            var resolvedClasses = sut.ResolveMany<ITestInterfaceDummy>().ToList();
            var interfaceResolvedClass =
                resolvedClasses.FirstOrDefault(t => t.GetType() == typeof(ImplementationClassDummy));
            var directlyResolvedClass = sut.Resolve<ImplementationClassDummy>();

            interfaceResolvedClass.Should().Be(directlyResolvedClass);
        }

        [Test]
        public void RegisteringAttributedDisposableTransientDoesNotThrowAnException()
        {
            // arrange
            var sut = new ContainerSetup();
            var act = new Action(() => sut.RetrieveClassesRequiringRegistration(true));

            // act
            // assert
            act.ShouldNotThrow<Exception>();
        }

        [Test]
        public void SetupContainter()
        {
            // arrange
            var sut = new ContainerSetup();

            // act
            sut.RetrieveClassesRequiringRegistration(true);

            // assert
            var resolvedClass = sut.Resolve<AutoRegistrationDummy>();
            resolvedClass.GetType().Should().Be<AutoRegistrationDummy>();
        }

        [Test]
        public void SingletonRegistrationsAreResolvedCorrectly()
        {
            // arrange
            var sut = new ContainerSetup();

            // act
            sut.RetrieveClassesRequiringRegistration(true);

            // assert
            var resolvedClass = sut.Resolve<SingletonDummy>();
            var secondResolve = sut.Resolve<SingletonDummy>();
            resolvedClass.Should().Be(secondResolve);
        }

        [Test]
        public void TransientRegistrationsAreResolvedCorrectly()
        {
            // arrange
            var sut = new ContainerSetup();

            // act
            sut.RetrieveClassesRequiringRegistration(true);

            // assert
            var resolvedClass = sut.Resolve<AutoRegistrationDummy>();
            var secondResolve = sut.Resolve<AutoRegistrationDummy>();
            resolvedClass.Should().NotBe(secondResolve);
        }

        #endregion

        // ReSharper disable once UnusedMember.Local - Used via DI
        [AutomaticContainerRegistration(typeof(DisposableDummy))]
        [DisposableTransient]
        private class DisposableDummy : IDisposable
        {
            #region Public Methods

            public void Dispose()
            {
            }

            #endregion
        }

        [AutomaticContainerRegistration(typeof(AutoRegistrationDummy))]
        private class AutoRegistrationDummy
        {
        }

        [AutomaticContainerRegistration(typeof(SingletonDummy), ServiceReuse.Singleton)]
        private class SingletonDummy
        {
        }

        [AutomaticConcretionContainerRegistration]
        private abstract class BaseClassDummy
        {
        }

        private class InheritorClassDummy : BaseClassDummy
        {
        }

        [AutomaticConcretionContainerRegistration(ServiceReuse.Singleton)]
        private interface ITestInterfaceDummy
        {
        }

        private class ImplementationClassDummy : ITestInterfaceDummy
        {
        }

        private class ImplmentationClassTooDummy : ITestInterfaceDummy
        {
        }

        [AutomaticContainerRegistration(typeof(MultiConstructorClass), ServiceReuse.Default, true)]
        // ReSharper disable once UnusedMember.Local - Used via DI
        private class MultiConstructorClass
        {
            #region Constructor

            // ReSharper disable once MemberCanBeProtected.Local - Needed to fully test injection with multiple constructors
            public MultiConstructorClass()
            {
            }

            // ReSharper disable once UnusedMember.Local - Needed to fully test injection with multiple constructors
            public MultiConstructorClass(AutoRegistrationDummy autoRegistrationDummy)
            {
                AutoRegistrationDummy = autoRegistrationDummy;
            }

            #endregion

            #region Properties

            // ReSharper disable once UnusedAutoPropertyAccessor.Local - Used for test purposes so we can get a valid second contructor
            private AutoRegistrationDummy AutoRegistrationDummy { get; }

            #endregion
        }

        [AutomaticConcretionContainerRegistration(ServiceReuse.Default, true)]
        // ReSharper disable once UnusedMember.Local - Used via DI
        private abstract class MultiConstructorConcretionClassDummy
        {
        }

        // ReSharper disable once UnusedMember.Local - Used via DI
        private class MultiConstructorConcretionInheritor : MultiConstructorConcretionClassDummy
        {
            #region Constructor

            // ReSharper disable once MemberCanBeProtected.Local - Needed to fully test injection with multiple constructors
            public MultiConstructorConcretionInheritor()
            {
            }

            // ReSharper disable once UnusedMember.Local - Needed to fully test injection with multiple constructors
            public MultiConstructorConcretionInheritor(AutoRegistrationDummy autoRegistrationDummy)
            {
                AutoRegistrationDummy = autoRegistrationDummy;
            }

            #endregion

            #region Properties

            // ReSharper disable once UnusedAutoPropertyAccessor.Local - Used for test purposes so we can get a valid second constructor
            private AutoRegistrationDummy AutoRegistrationDummy { get; }

            #endregion
        }
    }
}