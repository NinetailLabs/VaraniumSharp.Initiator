using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using HttpMockSlim;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace VaraniumSharp.Initiator.Tests.Fixtures
{
    public class UserSigninHandler : IHttpHandlerMock
    {
        #region Constructor

        public UserSigninHandler(string callbackUrl, string accessToken, string refreshToken, TokenGenerator generator)
        {
            _callbackUrl = callbackUrl;
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _generator = generator;
        }

        #endregion

        #region Public Methods

        public bool Handle(HttpListenerContext context)
        {
            if (context.Request.HttpMethod == "GET"
                && context.Request.Url.AbsolutePath.EndsWith(AuthPath))
            {
                var state = context.Request.QueryString["State"];
                _nonce = context.Request.QueryString["Nonce"];

                _idToken = _generator.GenerateToken(new ClaimsIdentity(new List<Claim>
                {
                    new Claim("sub", "blah"),
                    new Claim("nonce", _nonce)
                }));

                using (var client = new HttpClient())
                {
                    client.PostAsync(_callbackUrl, new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("access_token", _accessToken),
                        new KeyValuePair<string, string>("refresh_token", _refreshToken),
                        new KeyValuePair<string, string>("code", Guid.NewGuid().ToString()),
                        new KeyValuePair<string, string>("state", state),
                        new KeyValuePair<string, string>("id_token", _idToken),
                        new KeyValuePair<string, string>("nonce", _nonce)
                    })).Wait();
                }
            }

            if (context.Request.HttpMethod == "POST")
            {
                var responseData = JsonConvert.SerializeObject(new TokenResponseWrapper(_accessToken, _refreshToken)
                {
                    IdentityToken = _idToken
                });

                var memStream = new MemoryStream();
                var streamWrite = new StreamWriter(memStream);
                streamWrite.Write(responseData);
                streamWrite.Flush();
                memStream.Position = 0;
                memStream.CopyTo(context.Response.OutputStream);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.OK;

                context.Response.Close();

                return true;
            }

            return false;
        }

        #endregion

        #region Variables

        private const string AuthPath = "/protocol/openid-connect/auth";

        private readonly string _accessToken;

        private readonly string _callbackUrl;

        private readonly TokenGenerator _generator;

        private readonly string _refreshToken;

        private string _idToken;

        private string _nonce;

        #endregion
    }
}