using Labora.Domain.Enums;

namespace Labora.Application.DTOs.Otp;

public class StartOtpRequestDto
{
    public string PhoneNumber { get; set; } = string.Empty;
    public OtpPurpose Purpose { get; set; }
}
