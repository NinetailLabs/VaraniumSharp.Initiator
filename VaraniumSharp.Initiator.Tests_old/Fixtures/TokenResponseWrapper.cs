using Newtonsoft.Json;

namespace VaraniumSharp.Initiator.Tests.Fixtures
{
    public class TokenResponseWrapper
    {
        #region Constructor

        public TokenResponseWrapper(string accessToken, string refreshToken)
        {
            RefreshToken = refreshToken;
            AccessToken = accessToken;
        }

        #endregion

        #region Properties

        [JsonProperty("access_token")]
        public string AccessToken { get; }

        [JsonProperty("id_token")]
        public string IdentityToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; }

        #endregion
    }
}