using System;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using VaraniumSharp.Initiator.DependencyInjection;
using VaraniumSharp.Initiator.Tests.Fixtures;

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
            act.Should().NotThrow<Exception>();
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
            var resolvedClasses = sut.ResolveMany<ITestInterfaceDummy>().ToList();
            var secondResolve = sut.ResolveMany<ITestInterfaceDummy>().ToList();
            resolvedClasses.Should().BeEquivalentTo(secondResolve);
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
            act.Should().NotThrow<Exception>();
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
            act.Should().NotThrow<Exception>();
        }

        [Test]
        public void RegistrationOfDisposableConcretionClassDoesNotThrowAnException()
        {
            // arrange
            var sut = new ContainerSetup();
            var act = new Action(() => sut.RetrieveConcretionClassesRequiringRegistration(true));

            // act
            // assert
            act.Should().NotThrow<Exception>();
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
    }
}