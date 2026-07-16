using Labora.Application.Common;

namespace Labora.Application.Interfaces;

public interface IOtpRequestContextProvider
{
    OtpRequestContext GetContext();
}
