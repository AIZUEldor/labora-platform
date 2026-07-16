namespace Labora.Application.Interfaces;

public interface IIdentifierHasher
{
    string HashIp(string rawIp);
    string HashDevice(string rawDeviceId);
}
