using IdentityModel.OidcClient;

namespace VaraniumSharp.Initiator.Security
{
    /// <summary>
    /// Stores options required to connect to the Identity Server and authenticate the client
    /// </summary>
    public class IdentityServerConnectionDetails
    {
        #region Constructor

        /// <summary>
        /// Construct with details
        /// </summary>
        /// <param name="replaceRefreshToken">Does the server replace the Refresh Token when the refresh endpoint is called</param>
        /// <param name="clientOptions">Options used to authenticate the client to the Identity Server</param>
        public IdentityServerConnectionDetails(bool replaceRefreshToken,
            OidcClientOptions clientOptions)
        {
            ReplaceRefreshToken = replaceRefreshToken;
            OidcOptions = clientOptions;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Options used to authenticate the client to the Identity Server
        /// </summary>
        public OidcClientOptions OidcOptions { get; }

        /// <summary>
        /// Does the server replace the Refresh Token when the refresh endpoint is called
        /// </summary>
        public bool ReplaceRefreshToken { get; }

        #endregion
    }
}