using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VaraniumSharp.Initiator.Security;

namespace VaraniumSharp.Initiator.Interfaces.Security
{
    /// <summary>
    /// Manage Access Tokens
    /// </summary>
    public interface ITokenManager
    {
        #region Properties

        /// <summary>
        /// Get list of TokenNames that have Populated server details
        /// </summary>
        List<string> ServerDetailKeys { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Add connection details for an Identity Server that is associated with a specific token.
        /// If the same tokenName is passed again the IdentityServerConnectionDetails will be updated
        /// <remarks>
        /// This method must be called before attempting to validate tokens
        /// </remarks>
        /// </summary>
        /// <param name="tokenName">Name of the token that will be retrieved with the details</param>
        /// <param name="connectionDetails">Details that is used to identify the client to the Identity Server</param>
        Task AddServerDetails(string tokenName, IdentityServerConnectionDetails connectionDetails);

        /// <summary>
        /// Retrieve the user's Access Token.
        /// This method executes all the neccessary steps to retrieve the Access Token, validate it's expiry, handle refresh (if required) or all else failing guiding the user through login
        /// </summary>
        /// <param name="tokenName">Name of the token</param>
        /// <exception cref="ArgumentException">Thrown if the ServerDetails for the specific tokenName has not been populated</exception>
        /// <returns>TokenData if the user has an Access Token, otherwise null</returns>
        Task<TokenData> CheckSigninAsync(string tokenName);

        #endregion
    }
}