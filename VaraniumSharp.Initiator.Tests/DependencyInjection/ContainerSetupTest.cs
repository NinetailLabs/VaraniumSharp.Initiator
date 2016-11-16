using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using VaraniumSharp.Attributes;
using VaraniumSharp.Enumerations;
using VaraniumSharp.Initiator.DependencyInjection;

namespace VaraniumSharp.Initiator.Tests.DependencyInjection
{
    public class ContainerSetupTest
    {
        #region Public Methods

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

        [AutomaticContainerRegistration(typeof(AutoRegistrationDummy))]
        private class AutoRegistrationDummy
        {}

        [AutomaticContainerRegistration(typeof(SingletonDummy), ServiceReuse.Singleton)]
        private class SingletonDummy
        {}

        [AutomaticConcretionContainerRegistration]
        private abstract class BaseClassDummy
        {}

        private class InheritorClassDummy : BaseClassDummy
        {}

        [AutomaticConcretionContainerRegistration(ServiceReuse.Singleton)]
        private interface ITestInterfaceDummy
        {}

        private class ImplementationClassDummy : ITestInterfaceDummy
        {}

        private class ImplmentationClassTooDummy : ITestInterfaceDummy
        {}
    }
}