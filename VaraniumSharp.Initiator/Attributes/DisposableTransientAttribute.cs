using System;

namespace VaraniumSharp.Initiator.Attributes
{
    /// <summary>
    /// Attribute to indicate to container setup that it should mark a class as a Disposable Transient that the user will dispose of
    /// <see>
    ///     <cref>https://bitbucket.org/dadhi/dryioc/wiki/ReuseAndScopes#markdown-header-disposable-transient</cref>
    /// </see>
    /// <remarks>
    /// User is responsible for disposing of the class, the container is not set up to track it
    /// </remarks>
    /// </summary>
    public class DisposableTransientAttribute : Attribute
    {
    }
}