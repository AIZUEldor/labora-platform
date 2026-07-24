using Labora.Application.DTOs.Payments.Payme;

namespace Labora.Application.Interfaces;

public interface IPaymeCheckoutUrlBuilder
{
    string Build(PaymeCheckoutUrlRequest request);
}
