﻿using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using VaraniumSharp.Initiator.Caching;
using VaraniumSharp.Initiator.Tests.Fixtures;
using VaraniumSharp.Interfaces.Caching;

namespace VaraniumSharp.Initiator.Tests.Caching
{
    public class MemoryCacheWrapperFactoryTests
    {
        #region Public Methods

        [Test]
        public void CreateInstanceWithCustomPolicy()
        {
            // arrange
            var func = new Func<string, Task<ITestHelper>>(DataRetrievalFunc);
            var containerDummy = new ContainerFixture();
            var cachedDummy = new Mock<IMemoryCacheWrapper<ITestHelper>>();
            var policy = new CacheItemPolicy();
            var sut = new MemoryCacheWrapperFactory<ITestHelper>(containerDummy);

            cachedDummy.SetupAllProperties();
            containerDummy.SetupItemToResolve(cachedDummy.Object);

            // act
            var cache = sut.Create(policy, func);

            // assert
            cache.Should().NotBeNull();
            cache.CachePolicy.Should().Be(policy);
            cache.DataRetrievalFunc.Should().Be(func);
        }

        [Test]
        public void CreateInstanceWithDefaultPolicy()
        {
            // arrange
            var func = new Func<string, Task<ITestHelper>>(DataRetrievalFunc);
            var containerDummy = new ContainerFixture();
            var cachedDummy = new Mock<IMemoryCacheWrapper<ITestHelper>>();
            var sut = new MemoryCacheWrapperFactory<ITestHelper>(containerDummy);

            containerDummy.SetupItemToResolve(cachedDummy.Object);
            cachedDummy.SetupAllProperties();

            // act
            var result = sut.CreateWithDefaultSlidingPolicy(func);

            // assert
            result.Should().NotBeNull();
            result.DataRetrievalFunc.Should().Be(func);
            result.CachePolicy.SlidingExpiration.Minutes.Should().Be(DefaultTimeoutInMinutes);
        }

        #endregion

        #region Private Methods

        private static async Task<ITestHelper> DataRetrievalFunc(string s)
        {
            await Task.Delay(10);
            return (new Mock<ITestHelper>()).Object;
        }

        #endregion

        #region Variables

        private const int DefaultTimeoutInMinutes = 5;

        #endregion
    }
}