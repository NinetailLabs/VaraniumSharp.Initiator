using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace VaraniumSharp.Initiator.Tests.Fixtures
{
    public class UserSigninHandler
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

        #region Properties

        public string AuthPath => "/protocol/openid-connect/auth";

        #endregion

        #region Public Methods

        public void Handle(HttpContext context)
        {
            if (context.Request.Method == "GET")
            {
                var state = context.Request.Query["State"];
                _nonce = context.Request.Query["Nonce"];

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

                return;
            }

            if (context.Request.Method == "POST")
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
                memStream.CopyTo(context.Response.Body);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int) HttpStatusCode.OK;
            }
        }

        #endregion

        #region Variables

        private readonly string _accessToken;

        private readonly string _callbackUrl;

        private readonly TokenGenerator _generator;

        private readonly string _refreshToken;

        private string _idToken;

        private string _nonce;

        #endregion
    }
}