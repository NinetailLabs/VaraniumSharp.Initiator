using IdentityModel.Client;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace VaraniumSharp.Initiator.Tests.Fixtures
{
    public class UserInfoFixture
    {
        #region Properties

        public string UserInfoPath => "/protocol/openid-connect/userinfo";

        #endregion

        #region Public Methods

        public void Handle(HttpContext context)
        {
            if (context.Request.Method == "GET")
            {
                //So this is a little weird, but it is apparently the way it expects the claims so that's how we do it
                var claimCollection = new UserInfoData
                {
                    Sub = "blah"
                };
                var userinfoJson = JsonConvert.SerializeObject(claimCollection);
                var userInfo = new UserInfoResponse(userinfoJson);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                var memStream = new MemoryStream();
                var streamWrite = new StreamWriter(memStream);
                streamWrite.Write(userInfo.Json);
                streamWrite.Flush();
                memStream.Position = 0;
                memStream.CopyTo(context.Response.Body);
            }
        }

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