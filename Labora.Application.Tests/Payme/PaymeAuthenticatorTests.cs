using System.Text;
using Labora.Application.Options;
using Labora.Application.Services;

namespace Labora.Application.Tests.Payme;

public class PaymeAuthenticatorTests
{
    private const string Login = "Paycom";
    private const string Password = "test-secret-key";

    private static PaymeAuthenticator CreateAuthenticator(string login = Login, string password = Password)
    {
        return new PaymeAuthenticator(Microsoft.Extensions.Options.Options.Create(new PaymeMerchantOptions
        {
            MerchantId = "merchant-1",
            Login = login,
            Password = password
        }));
    }

    private static string BasicHeader(string login, string password, string scheme = "Basic")
        => $"{scheme} " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{login}:{password}"));

    [Fact]
    public void Validate_CorrectCredentials_ReturnsTrue()
    {
        PaymeAuthenticator authenticator = CreateAuthenticator();

        Assert.True(authenticator.Validate(BasicHeader(Login, Password)));
    }

    [Fact]
    public void Validate_WrongPassword_ReturnsFalse()
    {
        PaymeAuthenticator authenticator = CreateAuthenticator();

        Assert.False(authenticator.Validate(BasicHeader(Login, "wrong-password")));
    }

    [Fact]
    public void Validate_WrongLogin_ReturnsFalse()
    {
        PaymeAuthenticator authenticator = CreateAuthenticator();

        Assert.False(authenticator.Validate(BasicHeader("SomeoneElse", Password)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_MissingOrBlankHeader_ReturnsFalse(string? header)
    {
        PaymeAuthenticator authenticator = CreateAuthenticator();

        Assert.False(authenticator.Validate(header));
    }

    [Fact]
    public void Validate_NonBasicScheme_ReturnsFalse()
    {
        PaymeAuthenticator authenticator = CreateAuthenticator();

        Assert.False(authenticator.Validate(BasicHeader(Login, Password, scheme: "Bearer")));
    }

    [Fact]
    public void Validate_MalformedBase64_ReturnsFalse()
    {
        PaymeAuthenticator authenticator = CreateAuthenticator();

        Assert.False(authenticator.Validate("Basic not-valid-base64!!!"));
    }

    [Fact]
    public void Validate_LowercaseBasicScheme_StillMatches()
    {
        PaymeAuthenticator authenticator = CreateAuthenticator();

        Assert.True(authenticator.Validate(BasicHeader(Login, Password, scheme: "basic")));
    }

    [Fact]
    public void Validate_EmptyDecodedCredentials_ReturnsFalse()
    {
        PaymeAuthenticator authenticator = CreateAuthenticator();

        Assert.False(authenticator.Validate("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Empty))));
    }

    [Fact]
    public void Validate_CorrectLoginWrongPassword_DoesNotShortCircuitAsTrue()
    {
        // Guards against a naive "compare login separately from password" implementation that might
        // accidentally treat a right-login/wrong-password pair as valid.
        PaymeAuthenticator authenticator = CreateAuthenticator();

        Assert.False(authenticator.Validate(BasicHeader(Login, Password + "x")));
    }
}
