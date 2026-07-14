namespace Labora.Application.Interfaces;

public interface ISmsSender
{
    Task SendAsync(string phoneNumber, string message);
}
