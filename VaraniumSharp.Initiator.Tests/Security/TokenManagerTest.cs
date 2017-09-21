using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using HttpMockSlim;
using IdentityModel.OidcClient;
using Moq;
using NUnit.Framework;
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

        //TODO - This does not work yet, not quite sure why
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

            fixture.TokenStorageMock
                .Setup(t => t.RetrieveAccessTokenAsync(tokenName))
                .ReturnsAsync(tokenDataDummy);
            fixture.TokenStorageMock
                .Setup(t => t.RetrieveRefreshTokenAsync(tokenName))
                .ReturnsAsync(refreshToken);

            using (var httpMock = fixture.HttpMock)
            {
                fixture.WellKnownSetup();
                fixture.SetupCertificates();
                var sut = fixture.Instance;

                // act
                var result = await sut.CheckSigninAsync(tokenName);

                // assert
            }
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

            public void SetupCertificates()
            {
                StartServer();
                HttpMock.HttpMock.Add("GET", ServerCertificatePath, (request, response) =>
                {
                    response.SetBody(TokenGenerator.JsonWebKeyString);
                    response.StatusCode = (int)HttpStatusCode.OK;
                });
            }

            public async Task SetupServerDataAsync(string tokenName)
            {
                var oidcOptions = new OidcClientOptions
                {
                    Authority = Authority,
                    ClientId = "TestClient",
                    ClientSecret = Guid.NewGuid().ToString(),
                    RedirectUri = "http://localhost:9999/"
                };
                var serverDetails = new IdentityServerConnectionDetails(Authority, false, oidcOptions);
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
    }
}