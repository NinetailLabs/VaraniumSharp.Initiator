using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using DryIoc;
using VaraniumSharp.Interfaces.Caching;

namespace VaraniumSharp.Initiator.Caching
{
    /// <summary>
    /// Factory for creating <see cref="IMemoryCacheWrapper{T}"/> instances
    /// </summary>
    public class MemoryCacheWrapperFactory<T> : IMemoryCacheWrapperFactory<T> where T : class
    {
        #region Constructor

        /// <summary>
        /// DI Constructor
        /// </summary>
        /// <param name="container">DryIoC instance</param>
        public MemoryCacheWrapperFactory(IContainer container)
        {
            _container = container;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a new instance of the MemoryCacheWrapper and initialize it
        /// </summary>
        /// <typeparam name="T">Type of items that will be stored in the cache</typeparam>
        /// <param name="policy">Cache eviction policy</param>
        /// <param name="dataRetrievalFunc">Function used to retrieve items if they are not in the cache</param>
        /// <returns>Initialized instance of MemoryCacheWrapper</returns>
        public IMemoryCacheWrapper<T> Create(CacheItemPolicy policy, Func<string, Task<T>> dataRetrievalFunc)
        {
            var instance = _container.Resolve<IMemoryCacheWrapper<T>>();
            instance.CachePolicy = policy;
            instance.DataRetrievalFunc = dataRetrievalFunc;
            return instance;
        }

        /// <summary>
        /// Creates a new instance of the MemoryCacheWrapper and initialize it with a default cache policy.
        /// The default policy uses SlidingExpiration with a duration 5 minute
        /// </summary>
        /// <typeparam name="T">Type of items that will be stored in the cache</typeparam>
        /// <param name="dataRetrievalFunc">Function used to retrieve items if they are not in the cache</param>
        /// <returns>Initialized instance of MemoryCacheWrapper</returns>
        public IMemoryCacheWrapper<T> CreateWithDefaultSlidingPolicy(Func<string, Task<T>> dataRetrievalFunc)
        {
            var defaultPolicy = new CacheItemPolicy

            {
                SlidingExpiration = TimeSpan.FromMinutes(DefaultTimeoutInMinutes)
            };
            return Create(defaultPolicy, dataRetrievalFunc);
        }

        #endregion

        #region Variables

        private const int DefaultTimeoutInMinutes = 5;

        private readonly IContainer _container;

        #endregion
    }
}