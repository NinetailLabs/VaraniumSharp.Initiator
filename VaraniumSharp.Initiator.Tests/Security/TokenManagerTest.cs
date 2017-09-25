using FluentAssertions;
using HttpMockSlim;
using IdentityModel.OidcClient;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using VaraniumSharp.Initiator.Interfaces.Security;
using VaraniumSharp.Initiator.Security;
using VaraniumSharp.Initiator.Tests.Fixtures;

namespace VaraniumSharp.Initiator.Tests.Security
{
    public class TokenManagerTest
    {
        #region Public Methods

        [Test]
        public async Task AddingDifferentServerDetailsForTheSameTokenNameUpdatesTheValues()
        {
            // arrange
            const string tokenName = "Test Token";
            const string authority = "Authority";
            const string authority2 = "Authority";
            var oidcOptions = new OidcClientOptions();
            var serverDetails = new IdentityServerConnectionDetails(authority, false, oidcOptions);
            var serverDetails2 = new IdentityServerConnectionDetails(authority2, false, oidcOptions);
            var fixture = new TokenManagerFixture();

            var sut = fixture.Instance;
            await sut.AddServerDetails(tokenName, serverDetails);

            // act
            await sut.AddServerDetails(tokenName, serverDetails2);

            // assert
            sut.ServerDetailKeys.Count.Should().Be(1);
        }

        [Test]
        public void AttemptingToSignInWhenCredentialsAreNotSetThrowsAnException()
        {
            // arrange
            const string tokenName = "Test Token";
            var fixture = new TokenManagerFixture();
            var sut = fixture.Instance;
            var act = new Action(() => sut.CheckSigninAsync(tokenName).Wait());

            // act
            // assert
            act.ShouldThrow<ArgumentException>();
        }

        [Test]
        public async Task IfAccecssTokenExistsAndHasNotExpiredYetItWillBeReturned()
        {
            // arrange
            const string tokenName = "Test Token";
            var fixture = new TokenManagerFixture();
            await fixture.SetupServerDataAsync(tokenName);
            var token = fixture.TokenGenerator.GenerateToken();
            var tokenDataDummy = new TokenData(token);

            fixture.TokenStorageMock
                .Setup(t => t.RetrieveAccessTokenAsync(tokenName))
                .ReturnsAsync(tokenDataDummy);

            var sut = fixture.Instance;

            // act
            var result = await sut.CheckSigninAsync(tokenName);

            // assert
            result.Should().Be(tokenDataDummy);
        }

        [Test]
        public async Task IfTheSameTokenIsRetrievedAgainAndItIsStillValidItWillNotBeRetrievedFromStorageAgain()
        {
            // arrange
            const string tokenName = "Test Token";
            var fixture = new TokenManagerFixture();
            await fixture.SetupServerDataAsync(tokenName);
            var token = fixture.TokenGenerator.GenerateToken();
            var tokenDataDummy = new TokenData(token);

            fixture.TokenStorageMock
                .Setup(t => t.RetrieveAccessTokenAsync(tokenName))
                .ReturnsAsync(tokenDataDummy);

            var sut = fixture.Instance;
            await sut.CheckSigninAsync(tokenName);

            // act
            await sut.CheckSigninAsync(tokenName);

            // assert
            fixture.TokenStorageMock.Verify(t => t.RetrieveAccessTokenAsync(tokenName), Times.Once);
        }

        [Test]
        public async Task IfTokenExpiredAndRefreshTokenIsReplacedOnRefreshItIsCorrectlyUpdatedInStorage()
        {
            // arrange
            const string tokenName = "Test Token";
            var refreshToken = Guid.NewGuid().ToString();
            var fixture = new TokenManagerFixture();
            await fixture.SetupServerDataAsync(tokenName, true);
            var token = fixture.TokenGenerator.GenerateToken(DateTime.UtcNow.AddMinutes(-15),
                DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddMinutes(-17));
            var tokenDataDummy = new TokenData(token);
            var newAccessToken = fixture.TokenGenerator.GenerateToken();
            var newRefreshToken = Guid.NewGuid().ToString();

            fixture.TokenStorageMock
                .Setup(t => t.RetrieveAccessTokenAsync(tokenName))
                .ReturnsAsync(tokenDataDummy);
            fixture.TokenStorageMock
                .Setup(t => t.RetrieveRefreshTokenAsync(tokenName))
                .ReturnsAsync(refreshToken);

            fixture.WellKnownSetup();
            fixture.SetupCertificates();
            fixture.AuthRefreshSetup(newAccessToken, newRefreshToken);
            fixture.AuthSetup();
            var sut = fixture.Instance;

            // act
            await sut.CheckSigninAsync(tokenName);

            // assert
            fixture.TokenStorageMock.Verify(t => t.StoreRefreshTokenAsync(tokenName, newRefreshToken), Times.Once);
            fixture.HttpMock.Dispose();
        }

        [Test]
        public async Task IfTokenExpiredRefreshTokenIsRetrievedAndUsedToRefreshTheAccessToken()
        {
            // arrange
            const string tokenName = "Test Token";
            var refreshToken = Guid.NewGuid().ToString();
            var fixture = new TokenManagerFixture();
            await fixture.SetupServerDataAsync(tokenName);
            var token = fixture.TokenGenerator.GenerateToken(DateTime.UtcNow.AddMinutes(-15),
                DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddMinutes(-17));
            var tokenDataDummy = new TokenData(token);
            var newAccessToken = fixture.TokenGenerator.GenerateToken();
            var newRefreshToken = Guid.NewGuid().ToString();

            fixture.TokenStorageMock
                .Setup(t => t.RetrieveAccessTokenAsync(tokenName))
                .ReturnsAsync(tokenDataDummy);
            fixture.TokenStorageMock
                .Setup(t => t.RetrieveRefreshTokenAsync(tokenName))
                .ReturnsAsync(refreshToken);

            fixture.WellKnownSetup();
            fixture.SetupCertificates();
            fixture.AuthRefreshSetup(newAccessToken, newRefreshToken);
            fixture.AuthSetup();
            var sut = fixture.Instance;

            // act
            var result = await sut.CheckSigninAsync(tokenName);

            // assert
            result.Token.Should().Be(newAccessToken);
            fixture.TokenStorageMock.Verify(t => t.StoreAccessTokenAsync(tokenName, newAccessToken), Times.Once);
            fixture.TokenStorageMock.Verify(t => t.StoreRefreshTokenAsync(tokenName, newRefreshToken), Times.Never);
            fixture.HttpMock.Dispose();
        }

        [Test]
        public async Task ServerDetailsAreAddedCorrectly()
        {
            // arrange
            const string tokenName = "Test Token";
            const string authority = "Authority";
            var oidcOptions = new OidcClientOptions();
            var serverDetails = new IdentityServerConnectionDetails(authority, false, oidcOptions);
            var fixture = new TokenManagerFixture();

            var sut = fixture.Instance;

            // act
            await sut.AddServerDetails(tokenName, serverDetails);

            // assert
            sut.ServerDetailKeys.Should().Contain(tokenName);
        }

        #endregion

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local", Justification =
            "Test Fixture - Unit tests require access to Mocks")]
        private class TokenManagerFixture
        {
            #region Constructor

            public TokenManagerFixture()
            {
                Instance = new TokenManager(TokenStorage);
                TokenGenerator = new TokenGenerator(Authority, "Tests");
            }

            #endregion

            #region Properties

            public HttpMockFixture HttpMock { get; private set; }

            public TokenManager Instance { get; }
            public TokenGenerator TokenGenerator { get; }

            public ITokenStorage TokenStorage => TokenStorageMock.Object;
            public Mock<ITokenStorage> TokenStorageMock { get; } = new Mock<ITokenStorage>();

            #endregion

            #region Public Methods

            public void AuthRefreshSetup(string newAccessToken, string newRefreshToken)
            {
                StartServer();
                HttpMock.HttpMock.Add(new RefreshTokenHandler(newAccessToken, newRefreshToken));
            }

            public void AuthSetup()
            {
                StartServer();
                HttpMock.HttpMock.Add("GET", AuthPath,
                    (request, response) => { response.StatusCode = (int)HttpStatusCode.OK; });
            }

            public void SetupCertificates()
            {
                StartServer();
                HttpMock.HttpMock.Add("GET", ServerCertificatePath, (request, response) =>
                {
                    response.SetBody(TokenGenerator.JsonWebKeyString);
                    response.StatusCode = (int)HttpStatusCode.OK;
                });
            }

            public async Task SetupServerDataAsync(string tokenName, bool replaceRefresh = false)
            {
                var oidcOptions = new OidcClientOptions
                {
                    Authority = Authority,
                    ClientId = "TestClient",
                    ClientSecret = Guid.NewGuid().ToString(),
                    RedirectUri = "http://localhost:9999/"
                };
                var serverDetails = new IdentityServerConnectionDetails(Authority, replaceRefresh, oidcOptions);
                await Instance.AddServerDetails(tokenName, serverDetails);
            }

            public void WellKnownSetup()
            {
                StartServer();
                HttpMock.HttpMock.Add("GET", WellKnownPath, (request, response) =>
                {
                    var appPath = AppDomain.CurrentDomain.BaseDirectory;
                    var data = File.ReadAllText(Path.Combine(appPath, "Resources", "OpenIdConfig.json"));
                    response.SetBody(data);
                    response.StatusCode = (int)HttpStatusCode.OK;
                });
            }

            #endregion

            #region Private Methods

            private void StartServer()
            {
                if (HttpMock == null)
                {
                    HttpMock = new HttpMockFixture();
                    HttpMock.SetupServer(Authority);
                }
            }

            #endregion

            #region Variables

            public const string WellKnownPath = "/.well-known/openid-configuration";
            public const string ServerCertificatePath = "/protocol/openid-connect/certs";
            public const string AuthPath = "/protocol/openid-connect/auth";
            public const string Authority = "http://localhost:8888/";

            #endregion
        }

        private class HttpMockFixture : IDisposable
        {
            #region Properties

            public int CallCount { get; private set; }

            public HttpMock HttpMock { get; private set; }

            public bool PathWasCalled { get; private set; }

            #endregion

            #region Public Methods

            public void Dispose()
            {
                HttpMock.Stop();
            }

            public void SetupServer(string baseUrl)
            {
                if (HttpMock == null)
                {
                    HttpMock = new HttpMock();
                    HttpMock.Start(baseUrl);
                }
            }

            #endregion
        }

        private class RefreshTokenHandler : IHttpHandlerMock
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

        private class TokenResponseWrapper
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

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; }

            #endregion
        }
    }
}

// TODO - Add test that ensure new Signin is required if the provided access token isn't valid