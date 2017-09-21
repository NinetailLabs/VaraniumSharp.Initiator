using IdentityModel.OidcClient;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VaraniumSharp.Initiator.Interfaces.Security;

namespace VaraniumSharp.Initiator.Security
{
    /// <summary>
    /// Manage Access Tokens
    /// </summary>
    public class TokenManager
    {
        #region Constructor

        /// <summary>
        /// DI Constructor
        /// </summary>
        /// <param name="tokenStorage">TokenStorage implementation</param>
        public TokenManager(ITokenStorage tokenStorage)
        {
            _tokenStorage = tokenStorage;
            _tokenDictionary = new Dictionary<string, TokenData>();
            _refreshDictionary = new Dictionary<string, string>();
            _tokenLocks = new ConcurrentDictionary<string, SemaphoreSlim>();
            _serverDetails = new Dictionary<string, IdentityServerConnectionDetails>();
            _log = Log.Logger.ForContext("Module", nameof(TokenManager));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Get list of TokenNames that have Populated server details
        /// </summary>
        public List<string> ServerDetailKeys => _serverDetails.Keys.ToList();

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
        public async Task AddServerDetails(string tokenName, IdentityServerConnectionDetails connectionDetails)
        {
            var semaphore = _tokenLocks.GetOrAdd(tokenName, new SemaphoreSlim(1));
            try
            {
                await semaphore.WaitAsync();

                if (_serverDetails.ContainsKey(tokenName))
                {
                    _serverDetails[tokenName] = connectionDetails;
                }
                else
                {
                    _serverDetails.Add(tokenName, connectionDetails);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Retrieve the user's Access Token.
        /// This method executes all the neccessary steps to retrieve the Access Token, validate it's expiry, handle refresh (if required) or all else failing guiding the user through login
        /// </summary>
        /// <param name="tokenName">Name of the token</param>
        /// <exception cref="ArgumentException">Thrown if the ServerDetails for the specific tokenName has not been populated</exception>
        /// <returns>TokenData if the user has an Access Token, otherwise null</returns>
        public async Task<TokenData> CheckSigninAsync(string tokenName)
        {
            var semaphore = _tokenLocks.GetOrAdd(tokenName, new SemaphoreSlim(1));
            try
            {
                await semaphore.WaitAsync();
                if (!_serverDetails.ContainsKey(tokenName))
                {
                    throw new ArgumentException(
                        $"{tokenName} does not have server data yet, please call AddServerDetails with the same token name before attempting to sign in");
                }

                var tokenData = (await RetrieveAccessToken(tokenName)
                                 ?? await RefreshToken(tokenName))
                                ?? await AuthenticateClient(tokenName);

                return tokenData;
            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Guide the user through the sign-in process in order to acquire an Access Token
        /// </summary>
        /// <param name="tokenName">Name of the token</param>
        /// <returns>Access Token if successful, otherwise null</returns>
        private async Task<TokenData> AuthenticateClient(string tokenName)
        {
            TokenData token;

            var options = _serverDetails[tokenName];
            // create an HttpListener to listen for requests on that redirect URI.
            var http = new HttpListener();
            http.Prefixes.Add(options.OidcOptions.RedirectUri);
            http.Start();

            var client = new OidcClient(options.OidcOptions);
            var state = await client.PrepareLoginAsync();
            Process.Start(state.StartUrl);

            var context = await http.GetContextAsync();

            var formData = GetRequestPostData(context.Request);
            var response = context.Response;
            var responseString = "<html><body>Please return to the app.</body></html>";
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            await responseOutput.WriteAsync(buffer, 0, buffer.Length);
            responseOutput.Close();

            var result = await client.ProcessResponseAsync(formData, state);

            if (result.IsError)
            {
                _log.Error("Error occured during user authentication. {Error}", result.Error);
                return null;
            }
            else
            {
                await UpdateRefreshTokenStorage(tokenName, result.RefreshToken);
                token = await UpdateAccessTokenStorage(tokenName, result.AccessToken);
            }

            http.Stop();

            return token;
        }

        /// <summary>
        /// Execute an Access token refresh, updating the data stores.
        /// <remarks>
        /// This method does not lock the tokenName semaphore, it is the responsibility of callers to ensure the semaphore is locked.
        /// </remarks>
        /// </summary>
        /// <param name="tokenName">Name of the token</param>
        /// <param name="refreshToken">The refresh token to use</param>
        /// <param name="replaceRefreshToken">Should the refresh token be replaced. This is required in case the server gives a new refresh token when the current one is used</param>
        /// <param name="options">Options for connecting to the Identity Server</param>
        /// <returns>New token unless refresh could not be carried out, then null</returns>
        private async Task<TokenData> ExecuteTokenRefreshAsync(string tokenName, string refreshToken,
            bool replaceRefreshToken, OidcClientOptions options)
        {
            var client = new OidcClient(options);
            var result = await client.RefreshTokenAsync(refreshToken);
            if (result.IsError)
            {
                _log.Error("Error occured while trying to refresh Access Token. {Error}", result.Error);
                return null;
            }

            // Save our tokens to the datastore
            if (replaceRefreshToken)
            {
                await UpdateRefreshTokenStorage(tokenName, result.RefreshToken);
            }

            return await UpdateAccessTokenStorage(tokenName, result.AccessToken);
        }

        private static string GetRequestPostData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                return null;
            }

            using (var body = request.InputStream)
            {
                using (var reader = new System.IO.StreamReader(body, request.ContentEncoding))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Retrieve refresh token from storage and use it to attempt to refresh the Access Token
        /// </summary>
        /// <param name="tokenName">Name of the token</param>
        /// <returns>Fresh Access Token unless refresh failed in which case null</returns>
        private async Task<TokenData> RefreshToken(string tokenName)
        {
            string rToken;
            _refreshDictionary.TryGetValue(tokenName, out rToken);

            if (string.IsNullOrEmpty(rToken))
            {
                rToken = await _tokenStorage.RetrieveRefreshTokenAsync(tokenName);
            }

            // We do not have a refresh token
            if (string.IsNullOrEmpty(rToken))
            {
                return null;
            }

            // We need to call out to have our Access Token refreshed
            var connectionDetails = _serverDetails[tokenName];
            return await ExecuteTokenRefreshAsync(tokenName, rToken, connectionDetails.ReplaceRefreshToken,
                connectionDetails.OidcOptions);
        }

        /// <summary>
        /// Retrieve token from storage and validate that it has not expired yet
        /// </summary>
        /// <param name="tokenName">Name of the token to retrieve</param>
        /// <returns>TokenData unless the token does not exist or has expired in which case null is returned</returns>
        private async Task<TokenData> RetrieveAccessToken(string tokenName)
        {
            TokenData dToken;
            _tokenDictionary.TryGetValue(tokenName, out dToken);

            if (dToken == null)
            {
                dToken = await _tokenStorage.RetrieveAccessTokenAsync(tokenName);
            }

            _tokenDictionary[tokenName] = dToken;
            return dToken.TokenExpired ? null : dToken;
        }

        /// <summary>
        /// Store the Access token in the datastore and add/update the <see cref="_tokenDictionary"/>
        /// </summary>
        /// <param name="tokenName">Name of the token</param>
        /// <param name="newToken">Access token string to store</param>
        /// <returns>Populated TokenData from the access token</returns>
        private async Task<TokenData> UpdateAccessTokenStorage(string tokenName, string newToken)
        {
            await _tokenStorage.StoreAccessTokenAsync(tokenName, newToken);
            var tokenData = new TokenData(newToken);
            if (_tokenDictionary.ContainsKey(tokenName))
            {
                _tokenDictionary[tokenName] = tokenData;
            }
            else
            {
                _tokenDictionary.Add(tokenName, tokenData);
            }
            return tokenData;
        }

        /// <summary>
        /// Store refresh token in the datastore and add/update the <see cref="_refreshDictionary"/>
        /// </summary>
        /// <param name="tokenName">Name of the token</param>
        /// <param name="newRefreshToken">Refresh token that should be stored</param>
        private async Task UpdateRefreshTokenStorage(string tokenName, string newRefreshToken)
        {
            await _tokenStorage.StoreRefreshTokenAsync(tokenName, newRefreshToken);
            if (_refreshDictionary.ContainsKey(tokenName))
            {
                _refreshDictionary[tokenName] = newRefreshToken;
            }
            else
            {
                _refreshDictionary.Add(tokenName, newRefreshToken);
            }
        }

        #endregion

        #region Variables

        /// <summary>
        /// Serilog Logger instance
        /// </summary>
        private readonly ILogger _log;

        /// <summary>
        /// Dictionary to store refresh tokens
        /// </summary>
        private readonly Dictionary<string, string> _refreshDictionary;

        /// <summary>
        /// Dictionary to store server connection details
        /// </summary>
        private readonly Dictionary<string, IdentityServerConnectionDetails> _serverDetails;

        /// <summary>
        /// Dictionary to store token data
        /// </summary>
        private readonly Dictionary<string, TokenData> _tokenDictionary;

        /// <summary>
        /// Semaphores used to lock token access
        /// </summary>
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _tokenLocks;

        /// <summary>
        /// Token storage instance
        /// </summary>
        private readonly ITokenStorage _tokenStorage;

        #endregion
    }
}