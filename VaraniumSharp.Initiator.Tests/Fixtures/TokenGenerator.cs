using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace VaraniumSharp.Initiator.Tests.Fixtures
{
    public class TokenGenerator
    {
        #region Constructor

        public TokenGenerator(string issuer, string audience)
        {
            _issuer = issuer;
            _audience = audience;
            SetupKeys();
        }

        #endregion

        #region Properties

        public JsonWebKey JsonWebKey { get; private set; }

        public string JsonWebKeyString { get; private set; }

        #endregion

        #region Public Methods

        public string GenerateToken()
        {
            return GenerateToken(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(10), DateTime.UtcNow,
                new ClaimsIdentity());
        }

        public string GenerateToken(ClaimsIdentity claimsIdentity)
        {
            return GenerateToken(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(10), DateTime.UtcNow,
                claimsIdentity);
        }

        public string GenerateToken(DateTime? notBefore, DateTime? expires, DateTime? issuedAt)
        {
            return GenerateToken(notBefore, expires, issuedAt, new ClaimsIdentity());
        }

        public string GenerateToken(DateTime? notBefore, DateTime? expires, DateTime? issuedAt,
            ClaimsIdentity claimsIdentity)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.CreateEncodedJwt(
                _issuer,
                _audience,
                claimsIdentity,
                notBefore,
                expires,
                issuedAt,
                _signingCredentials);
        }

        #endregion

        #region Private Methods

        private void SetupKeys()
        {
            _rsa = new RSACryptoServiceProvider(2048);
            _rsaSecurityKey = new RsaSecurityKey(_rsa);
            _signingCredentials = new SigningCredentials(_rsaSecurityKey, "RS256");

            var webkey = new WebKey
            {
                Kid = _rsaSecurityKey.KeyId ?? "AB",
                Kty = "RSA",
                Alg = "RSA256",
                Use = "sig",
                N = Convert.ToBase64String(_rsa.ExportParameters(false).Modulus),
                E = Convert.ToBase64String(_rsa.ExportParameters(false).Exponent)
            };

            JsonWebKeyString = JsonConvert.SerializeObject(new KeyWrapper { Keys = new List<WebKey> { webkey } });

            JsonWebKey = new JsonWebKey(JsonWebKeyString);
        }

        #endregion

        #region Variables

        private readonly string _audience;

        private readonly string _issuer;

        private RSACryptoServiceProvider _rsa;

        private RsaSecurityKey _rsaSecurityKey;

        private SigningCredentials _signingCredentials;

        #endregion

        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification =
            "Test class - All properties are required even if not used")]
        private class WebKey
        {
            #region Properties

            public string Alg { get; set; }
            public string E { get; set; }
            public string Kid { get; set; }
            public string Kty { get; set; }
            public string N { get; set; }
            public string Use { get; set; }

            #endregion
        }

        private class KeyWrapper
        {
            #region Properties

            public List<WebKey> Keys { get; set; }

            #endregion
        }
    }
}