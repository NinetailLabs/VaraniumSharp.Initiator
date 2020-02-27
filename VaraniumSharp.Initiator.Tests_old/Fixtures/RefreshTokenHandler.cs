using System.IO;
using System.Net;
using HttpMockSlim;
using Newtonsoft.Json;

namespace VaraniumSharp.Initiator.Tests.Fixtures
{
    public class RefreshTokenHandler : IHttpHandlerMock
    {
        #region Constructor

        public RefreshTokenHandler(string accessToken, string refreshToken, bool returnError = false)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _returnError = returnError;
        }

        #endregion

        #region Public Methods

        public bool Handle(HttpListenerContext context)
        {
            if (context.Request.HttpMethod == "POST"
                && context.Request.Url.AbsolutePath.EndsWith(TokenPath))
            {
                if (_returnError)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.Close();
                    _returnError = false;
                    return true;
                }

                var tokenResponse =
                    JsonConvert.SerializeObject(new TokenResponseWrapper(_accessToken, _refreshToken));

                var memStream = new MemoryStream();
                var streamWrite = new StreamWriter(memStream);
                streamWrite.Write(tokenResponse);
                streamWrite.Flush();
                memStream.Position = 0;
                memStream.CopyTo(context.Response.OutputStream);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.OK;

                context.Response.Close();
            }
            return false;
        }

        #endregion

        #region Variables

        private const string TokenPath = "/protocol/openid-connect/token";

        private readonly string _accessToken;

        private readonly string _refreshToken;

        private bool _returnError;

        #endregion
    }
}