using DryIoc;
using FluentAssertions;
using NUnit.Framework;
using VaraniumSharp.Enumerations;
using VaraniumSharp.Initiator.DependencyInjection;

namespace VaraniumSharp.Initiator.Tests.DependencyInjection
{
    public class ServiceReuseExtensions
    {
        [Test]
        public void ConvertDefaultVaraniumReuseToDryIocReuse()
        {
            // arrange
            const ServiceReuse varaniumReuse = ServiceReuse.Default;
            var dryIocReuse = Reuse.Transient;


            // act
            var result = varaniumReuse.ConvertFromVaraniumReuse();

            // assert
            result.Should().Be(dryIocReuse);
        }

        [Test]
        public void ConvertSingletonVaraniumReuseToDryIocReuse()
        {
            // arrange
            const ServiceReuse varaniumReuse = ServiceReuse.Singleton;
            var dryIocReuse = Reuse.Singleton;


            // act
            var result = varaniumReuse.ConvertFromVaraniumReuse();

            // assert
            result.Should().Be(dryIocReuse);
        }

    }
}