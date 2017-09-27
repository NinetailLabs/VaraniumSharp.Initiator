using System.Threading.Tasks;
using VaraniumSharp.Initiator.Security;

namespace VaraniumSharp.Initiator.Interfaces.Security
{
    /// <summary>
    /// Used to store and retrieve Access and Refresh tokens to local storage for later use.
    /// <remark>
    /// Tokens should be stored securily, a leaked token will grant access to user account as if it is the user. Suggested to use account based encryption for storing on local machine
    /// </remark>
    /// </summary>
    public interface ITokenStorage
    {
        #region Public Methods

        /// <summary>
        /// Retrieve access token from storage
        /// </summary>
        /// <param name="tokenName">Name to store the token under - This is used to differentiate among different tokens</param>
        /// <returns>Access token wrapped in TokenData. If token does not exist, null</returns>
        Task<TokenData> RetrieveAccessTokenAsync(string tokenName);

        /// <summary>
        /// Retrieve refresh token from storage
        /// </summary>
        /// <param name="tokenName">Name to store the token under - This is used to differentiate among different tokens</param>
        /// <returns>Refresh token that was retrieved. If the token does not exists returns <see cref="string.Empty"/></returns>
        Task<string> RetrieveRefreshTokenAsync(string tokenName);

        /// <summary>
        /// Store Access token
        /// </summary>
        /// <param name="tokenName">Name to store the token under - This is used to differentiate among different tokens</param>
        /// <param name="accessToken">The access token to store</param>
        Task StoreAccessTokenAsync(string tokenName, string accessToken);

        /// <summary>
        /// Store a Refresh token
        /// </summary>
        /// <param name="tokenName">Name to store the token under - This is used to differentiate among different tokens</param>
        /// <param name="refreshToken">Refresh token to store</param>
        Task StoreRefreshTokenAsync(string tokenName, string refreshToken);

        #endregion
    }
}