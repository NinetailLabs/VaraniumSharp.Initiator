using FluentAssertions;
using IdentityModel.OidcClient;
using Moq;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HttpMock.Net;
using VaraniumSharp.Initiator.Interfaces.Security;
using VaraniumSharp.Initiator.Security;
using VaraniumSharp.Initiator.Tests.Fixtures;
using VaraniumSharp.Interfaces.GenericHelpers;

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

            var oidcOptions = new OidcClientOptions();
            var serverDetails = new IdentityServerConnectionDetails(false, oidcOptions);
            var serverDetails2 = new IdentityServerConnectionDetails(false, oidcOptions);
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
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public async Task FailureDuringRefreshTokenRetrievalWillResultInFullTokenRetrieval()
        {
            // arrange
            var loggerMock = LoggerFixture.SetupLogCatcher();
            const string tokenName = "Test Token";
            var refreshToken = Guid.NewGuid().ToString();
            var fixture = new TokenManagerFixture();
            await fixture.SetupServerDataAsync(tokenName);
            var token = fixture.TokenGenerator.GenerateToken(DateTime.UtcNow.AddMinutes(-15),
                DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddMinutes(-17));
            var tokenDataDummy = new TokenData(token);
            var newAccessToken = fixture.TokenGenerator.GenerateToken(75);

            fixture.TokenStorageMock
                .Setup(t => t.RetrieveAccessTokenAsync(tokenName))
                .ReturnsAsync(tokenDataDummy);
            fixture.TokenStorageMock
                .Setup(t => t.RetrieveRefreshTokenAsync(tokenName))
                .ReturnsAsync(refreshToken);

            fixture.WellKnownSetup();
            fixture.SetupCertificates();
            fixture.AuthRefreshSetup(newAccessToken, refreshToken, true);
            fixture.AuthSetup(newAccessToken, refreshToken);
            fixture.UserInfoSetup();
            var sut = fixture.Instance;

            // act
            await sut.CheckSigninAsync(tokenName);

            // assert
            loggerMock.Verify(t =>
                t.Error("Error occured while trying to refresh Access Token. {Error}", It.IsAny<string>()));
            //result.Should().NotBeNull();
            loggerMock.Verify(t => t.Error("Error occured during user authentication. {Error}", It.IsAny<string>()),
                Times.Once);

            fixture.HttpMock.HttpMock.Dispose();
        }

        [Test]
        public async Task FailureToGetATokenWithTimerRefreshDoesNotBroadcastARefreshEvent()
        {
            // arrange
            var loggerDummy = LoggerFixture.SetupLogCatcher();
            var refreshTimeout = TimeSpan.FromSeconds(10);
            const string tokenName = "Test Token";
            var refreshToken = Guid.NewGuid().ToString();
            var fixture = new TokenManagerFixture();
            await fixture.SetupServerDataAsync(tokenName, true);
            var token = fixture.TokenGenerator.GenerateToken(DateTime.UtcNow.AddMinutes(-15),
                DateTime.UtcNow.AddSeconds(15), DateTime.UtcNow.AddMinutes(-17));
            var tokenDataDummy = new TokenData(token);
            var wasRefreshed = false;

            fixture.TokenStorageMock
                .Setup(t => t.RetrieveAccessTokenAsync(tokenName))
                .ReturnsAsync(tokenDataDummy);
            fixture.TokenStorageMock
                .Setup(t => t.RetrieveRefreshTokenAsync(tokenName))
                .ReturnsAsync(refreshToken);

            fixture.WellKnownSetup();
            fixture.SetupCertificates();
            fixture.AuthRefreshSetup(null, null);
            var sut = fixture.Instance;
            sut.SetupRefreshTimeSpan(refreshTimeout);
            sut.TokenRefreshed += (sender, pair) => { wasRefreshed = true; };

            // act
            await sut.CheckSigninAsync(tokenName);
            await Task.Delay(TimeSpan.FromSeconds(10));

            // assert
            wasRefreshed.Should().BeFalse();
            loggerDummy.Verify(
                t => t.Warning(
                    "Attempting to refresh access token failed. No further auto-refreshes will occur for {TokenName}",
                    tokenName), Times.Once);

            fixture.HttpMock.Dispose();
        }

        [Test]
        public async Task IfAccessTokenExistsAndHasNotExpiredYetItWillBeReturned()
        {
            // arrange
            const string tokenName = "Test Token";
            var fixture = new TokenManagerFixture();
            await fixture.SetupServerDataAsync(tokenName);
            var token = fixture.TokenGenerator.GenerateToken(75);
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
        public async Task IfRefreshIntervalIsChangedExistingTokenRefreshesAreAdjusted()
        {
            // arrange
            var refreshTimeout = TimeSpan.FromSeconds(100);
            const string tokenName = "Test Token";
            var refreshToken = Guid.NewGuid().ToString();
            var fixture = new TokenManagerFixture();
            await fixture.SetupServerDataAsync(tokenName, true);
            var token = fixture.TokenGenerator.GenerateToken(DateTime.UtcNow.AddMinutes(-15),
                DateTime.UtcNow.AddSeconds(110), DateTime.UtcNow.AddMinutes(-17));
            var tokenDataDummy = new TokenData(token);
            var newAccessToken = fixture.TokenGenerator.GenerateToken(75);
            var newRefreshToken = Guid.NewGuid().ToString();
            var raisingToken = string.Empty;
            TokenData refreshedToken = null;

            fixture.TokenStorageMock
                .Setup(t => t.RetrieveAccessTokenAsync(tokenName))
                .ReturnsAsync(tokenDataDummy);
            fixture.TokenStorageMock
                .Setup(t => t.RetrieveRefreshTokenAsync(tokenName))
                .ReturnsAsync(refreshToken);

            fixture.WellKnownSetup();
            fixture.SetupCertificates();
            fixture.AuthRefreshSetup(newAccessToken, newRefreshToken);
            var sut = fixture.Instance;
            sut.SetupRefreshTimeSpan(refreshTimeout);
            sut.TokenRefreshed += (sender, pair) =>
            {
                raisingToken = pair.Key;
                refreshedToken = pair.Value;
            };

            // act
            await sut.CheckSigninAsync(tokenName);
            sut.SetupRefreshTimeSpan(TimeSpan.FromSeconds(50));
            await Task.Delay(TimeSpan.FromSeconds(10));

            // assert
            raisingToken.Should().BeNullOrEmpty();
            refreshedToken.Should().BeNull();
            fixture.HttpMock.Dispose();
        }

        [Test]
        public async Task IfRefreshIntervalIsMadeLongerThanTokensRemainingTimeTheTokenIsRefreshed()
        {
            // arrange
            var refreshTimeout = TimeSpan.FromSeconds(100);
            const string tokenName = "Test Token";
            var refreshToken = Guid.NewGuid().ToString();
            var fixture = new TokenManagerFixture();
            await fixture.SetupServerDataAsync(tokenName, true);
            var token = fixture.TokenGenerator.GenerateToken(DateTime.UtcNow.AddMinutes(-15),
                DateTime.UtcNow.AddSeconds(150), DateTime.UtcNow.AddMinutes(-17));
            var tokenDataDummy = new TokenData(token);
            var newAccessToken = fixture.TokenGenerator.GenerateToken(75);
            var newRefreshToken = Guid.NewGuid().ToString();
            var raisingToken = string.Empty;
            TokenData refreshedToken = null;

            fixture.TokenStorageMock
                .Setup(t => t.RetrieveAccessTokenAsync(tokenName))
                .ReturnsAsync(tokenDataDummy);
            fixture.TokenStorageMock
                .Setup(t => t.RetrieveRefreshTokenAsync(tokenName))
                .ReturnsAsync(refreshToken);

            fixture.WellKnownSetup();
            fixture.SetupCertificates();
            fixture.AuthRefreshSetup(newAccessToken, newRefreshToken);
            var sut = fixture.Instance;
            sut.SetupRefreshTimeSpan(refreshTimeout);
            sut.TokenRefreshed += (sender, pair) =>
            {
                raisingToken = pair.Key;
                refreshedToken = pair.Value;
            };

            // act
            await sut.CheckSigninAsync(tokenName);
            sut.SetupRefreshTimeSpan(TimeSpan.FromSeconds(200));
            await Task.Delay(TimeSpan.FromSeconds(10));

            // assert
            raisingToken.Should().Be(tokenName);
            refreshedToken.Token.Should().Be(newAccessToken);
            fixture.HttpMock.Dispose();
        }

        [Test]
        public async Task IfThereAreNoTokensSigninIsExecutedToAcquireTokens()
        {
            // arrange
            const string tokenName = "Test Token";
            var refreshToken = Guid.NewGuid().ToString();
            var fixture = new TokenManagerFixture();
            await fixture.SetupServerDataAsync(tokenName, true);
            var token = fixture.TokenGenerator.GenerateToken(75);

            fixture.WellKnownSetup();
            fixture.SetupCertificates();
            fixture.AuthSetup(token, refreshToken);
            fixture.UserInfoSetup();
            fixture.TokenEndpointSetup(token, refreshToken);
            var sut = fixture.Instance;

            // act
            var result = await sut.CheckSigninAsync(tokenName);

            // assert
            result.Token.Should().Be(token);
            fixture.TokenStorageMock.Verify(t => t.StoreAccessTokenAsync(tokenName, token), Times.Once);
            fixture.TokenStorageMock.Verify(t => t.StoreRefreshTokenAsync(tokenName, refreshToken), Times.Once);

            fixture.HttpMock.Dispose();
        }

        [Test]
        public async Task IfTheSameTokenIsRetrievedAgainAndItIsStillValidItWillNotBeRetrievedFromStorageAgain()
        {
            // arrange
            const string tokenName = "Test Token";
            var fixture = new TokenManagerFixture();
            await fixture.SetupServerDataAsync(tokenName);
            var token = fixture.TokenGenerator.GenerateToken(75);
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
            var newAccessToken = fixture.TokenGenerator.GenerateToken(75);
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
            var newAccessToken = fixture.TokenGenerator.GenerateToken(75);
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
        public async Task IfTokenExpiresAfterRefreshIntervalItIsAutomaticallyRefreshed()
        {
            // arrange
            var refreshTimeout = TimeSpan.FromSeconds(10);
            const string tokenName = "Test Token";
            var refreshToken = Guid.NewGuid().ToString();
            var fixture = new TokenManagerFixture();
            await fixture.SetupServerDataAsync(tokenName, true);
            var token = fixture.TokenGenerator.GenerateToken(DateTime.UtcNow.AddMinutes(-15),
                DateTime.UtcNow.AddSeconds(15), DateTime.UtcNow.AddMinutes(-17));
            var tokenDataDummy = new TokenData(token);
            var newAccessToken = fixture.TokenGenerator.GenerateToken(75);
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
            var sut = fixture.Instance;
            sut.SetupRefreshTimeSpan(refreshTimeout);

            // act
            await sut.CheckSigninAsync(tokenName);
            await Task.Delay(TimeSpan.FromSeconds(10));

            // assert
            fixture.TokenStorageMock.Verify(t => t.StoreRefreshTokenAsync(tokenName, newRefreshToken), Times.Once);
            fixture.HttpMock.Dispose();
        }

        [Test]
        public async Task IfTokenExpiresWithinTheRefreshIntervalItIsRefreshed()
        {
            // arrange
            const string tokenName = "Test Token";
            var refreshToken = Guid.NewGuid().ToString();
            var fixture = new TokenManagerFixture();
            await fixture.SetupServerDataAsync(tokenName, true);
            var token = fixture.TokenGenerator.GenerateToken(30);
            var tokenDataDummy = new TokenData(token);
            var newAccessToken = fixture.TokenGenerator.GenerateToken(75);
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
            var sut = fixture.Instance;

            // act
            await sut.CheckSigninAsync(tokenName);

            // assert
            fixture.TokenStorageMock.Verify(t => t.StoreRefreshTokenAsync(tokenName, newRefreshToken), Times.Once);
            fixture.HttpMock.Dispose();
        }

        [Test]
        public async Task OnTokenRefreshTheTokenRefreshedEventIsRaised()
        {
            // arrange
            var refreshTimeout = TimeSpan.FromSeconds(10);
            const string tokenName = "Test Token";
            var refreshToken = Guid.NewGuid().ToString();
            var fixture = new TokenManagerFixture();
            await fixture.SetupServerDataAsync(tokenName, true);
            var token = fixture.TokenGenerator.GenerateToken(DateTime.UtcNow.AddMinutes(-15),
                DateTime.UtcNow.AddSeconds(15), DateTime.UtcNow.AddMinutes(-17));
            var tokenDataDummy = new TokenData(token);
            var newAccessToken = fixture.TokenGenerator.GenerateToken(75);
            var newRefreshToken = Guid.NewGuid().ToString();
            var raisingToken = string.Empty;
            TokenData refreshedToken = null;

            fixture.TokenStorageMock
                .Setup(t => t.RetrieveAccessTokenAsync(tokenName))
                .ReturnsAsync(tokenDataDummy);
            fixture.TokenStorageMock
                .Setup(t => t.RetrieveRefreshTokenAsync(tokenName))
                .ReturnsAsync(refreshToken);

            fixture.WellKnownSetup();
            fixture.SetupCertificates();
            fixture.AuthRefreshSetup(newAccessToken, newRefreshToken);
            var sut = fixture.Instance;
            sut.SetupRefreshTimeSpan(refreshTimeout);
            sut.TokenRefreshed += (sender, pair) =>
            {
                raisingToken = pair.Key;
                refreshedToken = pair.Value;
            };

            // act
            await sut.CheckSigninAsync(tokenName);
            await Task.Delay(TimeSpan.FromSeconds(10));

            // assert
            raisingToken.Should().Be(tokenName);
            refreshedToken.Token.Should().Be(newAccessToken);
            fixture.HttpMock.Dispose();
        }

        [Test]
        public async Task ServerDetailsAreAddedCorrectly()
        {
            // arrange
            const string tokenName = "Test Token";
            var oidcOptions = new OidcClientOptions();
            var serverDetails = new IdentityServerConnectionDetails(false, oidcOptions);
            var fixture = new TokenManagerFixture();

            var sut = fixture.Instance;

            // act
            await sut.AddServerDetails(tokenName, serverDetails);

            // assert
            sut.ServerDetailKeys.Should().Contain(tokenName);
        }

        [Test]
        public void SettingRefreshTimeSpanUpdatesTheRefreshTimeSpan()
        {
            // arrange
            var refreshTimeSpan = TimeSpan.FromHours(3);
            var fixture = new TokenManagerFixture();
            var sut = fixture.Instance;

            // act
            sut.SetupRefreshTimeSpan(refreshTimeSpan);

            // assert
            sut.RefreshTimeSpan.Should().Be(refreshTimeSpan);
        }

        [Test]
        public async Task SigninFailureReturnsNullAndLogsTheIssue()
        {
            // arrange
            var loggerMock = LoggerFixture.SetupLogCatcher();
            const string tokenName = "Test Token";
            var refreshToken = Guid.NewGuid().ToString();
            var fixture = new TokenManagerFixture();
            await fixture.SetupServerDataAsync(tokenName, true);
            var token = fixture.TokenGenerator.GenerateToken(75);

            fixture.WellKnownSetup();
            fixture.SetupCertificates();
            fixture.AuthSetup(token, refreshToken);
            var sut = fixture.Instance;

            // act
            var result = await sut.CheckSigninAsync(tokenName);

            // assert
            result.Should().BeNull();
            loggerMock.Verify(t => t.Error("Error occured during user authentication. {Error}", It.IsAny<string>()),
                Times.Once);

            fixture.HttpMock.HttpMock.Dispose();
        }

        #endregion

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local", Justification =
            "Test Fixture - Unit tests require access to Mocks")]
        private class TokenManagerFixture
        {
            #region Constructor

            public TokenManagerFixture()
            {
                Instance = new TokenManager(TokenStorage, StaticMethodWrapper);
                TokenGenerator = new TokenGenerator(Authority.TrimEnd('/'), "Tests");
            }

            #endregion

            #region Properties

            public HttpMockFixture HttpMock { get; private set; }

            public TokenManager Instance { get; }
            public IStaticMethodWrapper StaticMethodWrapper => StaticMethodWrapperMock.Object;

            public Mock<IStaticMethodWrapper> StaticMethodWrapperMock { get; } = new Mock<IStaticMethodWrapper>();
            public TokenGenerator TokenGenerator { get; }

            public ITokenStorage TokenStorage => TokenStorageMock.Object;
            public Mock<ITokenStorage> TokenStorageMock { get; } = new Mock<ITokenStorage>();

            #endregion

            #region Public Methods

            public void AuthRefreshSetup(string newAccessToken, string newRefreshToken, bool returnError = false)
            {
                StartServer();
                var handler = new RefreshTokenHandler(newAccessToken, newRefreshToken, returnError);
                HttpMock.HttpMock
                    .WhenPost(handler.TokenPath, context => true)
                    .Do(context => { handler.Handle(context); });
            }

            public void AuthSetup(string accessToken, string refreshToken)
            {
                StartServer();
                StaticMethodWrapperMock
                    .Setup(t => t.StartProcess(It.IsAny<string>()))
                    .Callback((string url) =>
                    {
                        //We need to invoke the appropriate method on the signin handler - Easiest way is to just make the call the browser would
                        Task.Run(() =>
                        {
                            using var client = new HttpClient();
                            client.GetAsync(url);
                            Thread.Sleep(TimeSpan.FromSeconds(10));
                        });
                    });

                var handler = new UserSigninHandler(RedirectUrl, accessToken, refreshToken, TokenGenerator);
                HttpMock.HttpMock
                    .WhenGet(handler.AuthPath)
                    .Do(context => { handler.Handle(context); });
                HttpMock.HttpMock
                    .WhenPost(handler.AuthPath, context => true)
                    .Do(context => { handler.Handle(context); });
            }

            public void SetupCertificates()
            {
                StartServer();
                HttpMock.HttpMock
                    .WhenGet(ServerCertificatePath)
                    .Respond(TokenGenerator.JsonWebKeyString);
            }

            public async Task SetupServerDataAsync(string tokenName, bool replaceRefresh = false)
            {
                var oidcOptions = new OidcClientOptions
                {
                    Authority = Authority,
                    ClientId = "Tests",
                    ClientSecret = Guid.NewGuid().ToString(),
                    RedirectUri = RedirectUrl,
                    Policy = new Policy
                    {
                        RequireAuthorizationCodeHash = false,
                        RequireAccessTokenHash = false
                    }
                };
                var serverDetails = new IdentityServerConnectionDetails(replaceRefresh, oidcOptions);
                await Instance.AddServerDetails(tokenName, serverDetails);
            }

            public void TokenEndpointSetup(string accessToken, string refreshToken)
            {
                StartServer();
                HttpMock.HttpMock
                    .WhenPost(TokenEndpoint, context => true)
                    .Do(context =>
                    {
                        var handler = new UserSigninHandler(RedirectUrl, accessToken, refreshToken, TokenGenerator);

                        handler.HandleTokenExchange(context);
                    });
            }

            public void UserInfoSetup()
            {
                StartServer();
                var handler = new UserInfoFixture();
                HttpMock.HttpMock
                    .WhenGet(handler.UserInfoPath)
                    .Do(context => { handler.Handle(context); });
            }

            public void WellKnownSetup()
            {
                var appPath = AppDomain.CurrentDomain.BaseDirectory;
                var data = File.ReadAllText(Path.Combine(appPath, "Resources", "OpenIdConfig.json"));
                StartServer();
                HttpMock.HttpMock
                    .WhenGet(WellKnownPath)
                    .Respond(data);
            }

            #endregion

            #region Private Methods

            private void StartServer()
            {
                if (HttpMock == null)
                {
                    HttpMock = new HttpMockFixture();
                    HttpMock.SetupServer(Port);
                }
            }

            #endregion

            #region Variables

            public const string WellKnownPath = "/.well-known/openid-configuration";
            public const string ServerCertificatePath = "/protocol/openid-connect/certs";
            public const string TokenEndpoint = "/protocol/openid-connect/token";
            public const string RedirectUrl = "http://localhost:9999/";
            public const int Port = 8888;
            public readonly string Authority = $"http://localhost:{Port}/";

            #endregion
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local", Justification =
            "Test Fixture - Unit tests require access to values")]
        private class HttpMockFixture : IDisposable
        {
            #region Properties

            public int CallCount { get; private set; }

            public HttpHandlerBuilder HttpMock { get; private set; }

            public bool PathWasCalled { get; private set; }

            #endregion

            #region Public Methods

            public void Dispose()
            {
                HttpMock.Dispose();
            }

            public void SetupServer(int port)
            {
                if (HttpMock == null)
                {
                    HttpMock = Server.Start(port);
                }
            }

            #endregion
        }
    }
}

// TODO - Add test that ensure new Signin is required if the provided access token isn't valid