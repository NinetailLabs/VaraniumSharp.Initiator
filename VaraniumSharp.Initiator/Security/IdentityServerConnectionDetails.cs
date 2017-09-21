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
        /// <param name="authority">Url where the Identity Server can be reached</param>
        /// <param name="replaceRefreshToken">Does the server replace the Refresh Token when the refresh endpoint is called</param>
        /// <param name="clientOptions">Options used to authenticate the client to the Identity Server</param>
        public IdentityServerConnectionDetails(string authority, bool replaceRefreshToken,
            OidcClientOptions clientOptions)
        {
            Authority = authority;
            ReplaceRefreshToken = replaceRefreshToken;
            OidcOptions = clientOptions;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Url where the Identity Server can be reached
        /// </summary>
        public string Authority { get; }

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