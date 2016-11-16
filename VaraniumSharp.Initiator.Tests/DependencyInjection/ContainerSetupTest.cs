using FluentAssertions;
using NUnit.Framework;
using VaraniumSharp.Attributes;
using VaraniumSharp.Enumerations;
using VaraniumSharp.Initiator.DependencyInjection;

namespace VaraniumSharp.Initiator.Tests.DependencyInjection
{
    public class ContainerSetupTest
    {
        #region Public Methods

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
        { }

        [AutomaticContainerRegistration(typeof(SingletonDummy), Reuse = ServiceReuse.Singleton)]
        private class SingletonDummy
        { }
    }
}