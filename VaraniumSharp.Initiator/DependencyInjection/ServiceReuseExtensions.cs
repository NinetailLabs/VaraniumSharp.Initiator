using DryIoc;
using VaraniumSharp.Enumerations;

namespace VaraniumSharp.Initiator.DependencyInjection
{
    /// <summary>
    /// Extension method on top of ServiceReuse enumeration
    /// </summary>
    public static class ServiceReuseExtensions
    {
        #region Public Methods

        /// <summary>
        /// Convert a VaraniumSharp ServiceReuse enum value into a IReuse value used by DryIoc
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IReuse ConvertFromVaraniumReuse(this ServiceReuse self)
        {
            IReuse result;
            switch (self)
            {
                case ServiceReuse.Singleton:
                    result = Reuse.Singleton;
                    break;

                default:
                    result = Reuse.Transient;
                    break;
            }
            return result;
        }

        #endregion
    }
}