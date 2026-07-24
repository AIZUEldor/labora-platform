namespace Labora.Application.Interfaces;

public interface IPaymeAuthenticator
{
    bool Validate(string? authorizationHeaderValue);
}
