using System;
using System.IdentityModel.Tokens.Jwt;

namespace VaraniumSharp.Initiator.Security
{
    /// <summary>
    /// Storage for Token information
    /// </summary>
    public class TokenData
    {
        #region Constructor

        /// <summary>
        /// Construct from a Jwt token string
        /// </summary>
        /// <param name="token">Token string to construct the storage from</param>
        public TokenData(string token)
        {
            DecodedToken = new JwtSecurityToken(token);
            ExpirationDate = DecodedToken.ValidTo;
            Token = token;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The decoded token
        /// </summary>
        public JwtSecurityToken DecodedToken { get; }

        /// <summary>
        /// UTC date when the token expires
        /// </summary>
        public DateTime ExpirationDate { get; }

        /// <summary>
        /// Jwt token string
        /// </summary>
        public string Token { get; }

        /// <summary>
        /// Indicate if the token has expired
        /// </summary>
        public bool TokenExpired => DateTime.UtcNow > ExpirationDate;

        #endregion
    }
}