using System.IO;
using System.Net;
using HttpMockSlim;
using Newtonsoft.Json;

namespace VaraniumSharp.Initiator.Tests.Fixtures
{
    public class RefreshTokenHandler : IHttpHandlerMock
    {
        #region Constructor

        public RefreshTokenHandler(string accessToken, string refreshToken)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
        }

        #endregion

        #region Public Methods

        public bool Handle(HttpListenerContext context)
        {
            if (context.Request.HttpMethod == "POST"
                && context.Request.Url.AbsolutePath.EndsWith(TokenPath))
            {
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

                return true;
            }
            return false;
        }

        #endregion

        #region Variables

        private const string TokenPath = "/protocol/openid-connect/token";

        private readonly string _accessToken;

        private readonly string _refreshToken;

        #endregion
    }
}