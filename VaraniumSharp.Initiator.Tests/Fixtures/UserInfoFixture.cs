using HttpMockSlim;
using IdentityModel.Client;
using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace VaraniumSharp.Initiator.Tests.Fixtures
{
    public class UserInfoFixture : IHttpHandlerMock
    {
        #region Public Methods

        public bool Handle(HttpListenerContext context)
        {
            if (context.Request.HttpMethod == "GET" && context.Request.Url.AbsolutePath.EndsWith(UserInfoPath))
            {
                //So this is a little weird, but it is apparently the way it expects the claims so that's how we do it
                var claimCollection = new UserInfoData
                {
                    Sub = "blah"
                };
                var userinfoJson = JsonConvert.SerializeObject(claimCollection);
                var userInfo = new UserInfoResponse(userinfoJson);

                var memStream = new MemoryStream();
                var streamWrite = new StreamWriter(memStream);
                streamWrite.Write(userInfo.Json);
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

        private const string UserInfoPath = "/protocol/openid-connect/userinfo";

        #endregion

        private class UserInfoData
        {
            #region Properties

            [JsonProperty("sub")]
            public string Sub { get; set; }

            #endregion
        }
    }
}