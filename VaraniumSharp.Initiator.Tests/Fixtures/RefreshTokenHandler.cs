using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace VaraniumSharp.Initiator.Tests.Fixtures
{
    public class RefreshTokenHandler
    {
        #region Constructor

        public RefreshTokenHandler(string accessToken, string refreshToken, bool returnError = false)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _returnError = returnError;
        }

        #endregion

        #region Properties

        public string TokenPath => "/protocol/openid-connect/token";

        #endregion

        #region Public Methods

        public void Handle(HttpContext context)
        {
            if (context.Request.Method == "POST")
            {
                if (_returnError)
                {
                    context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                    _returnError = false;
                    return;
                }

                var tokenResponse =
                    JsonConvert.SerializeObject(new TokenResponseWrapper(_accessToken, _refreshToken));

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int) HttpStatusCode.OK;
                var memStream = new MemoryStream();
                var streamWrite = new StreamWriter(memStream);
                streamWrite.Write(tokenResponse);
                streamWrite.Flush();
                memStream.Position = 0;
                memStream.CopyTo(context.Response.Body);
            }
        }

        #endregion

        #region Variables

        private readonly string _accessToken;

        private readonly string _refreshToken;

        private bool _returnError;

        #endregion
    }
}