using System;
using VaraniumSharp.Attributes;

namespace VaraniumSharp.Initiator.Attributes
{
    /// <summary>
    /// Allows for the registration of disposable transients with the DryIoc container alongside VaraniumSharp's automatic container registration attributes
    /// Using this attribute without either <see cref="AutomaticContainerRegistrationAttribute"/> or <see cref="AutomaticConcretionContainerRegistrationAttribute"/> will result
    /// in the class not being registered with the container. The attribute is supplementary only
    /// <see>
    ///     <cref>https://bitbucket.org/dadhi/dryioc/wiki/ReuseAndScopes#markdown-header-disposable-transient</cref>
    /// </see>
    /// <remarks>
    /// This attribute does not support container disposal of the transient, delegating the responsibility to the user
    /// </remarks>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
    public class DisposableTransientAttribute : Attribute
    {
    }
}