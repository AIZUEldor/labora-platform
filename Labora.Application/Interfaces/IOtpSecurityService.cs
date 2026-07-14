using Labora.Domain.Enums;

namespace Labora.Application.Interfaces;

public interface IOtpSecurityService
{
    string GenerateOtpCode();
    string GenerateOperationToken();
    string HashOtpCode(Guid verificationId, OtpPurpose purpose, string normalizedPhone, string code);
    bool VerifyOtpCode(Guid verificationId, OtpPurpose purpose, string normalizedPhone, string code, string codeHash);
    string HashOperationToken(Guid verificationId, string rawOperationToken);
    bool VerifyOperationToken(Guid verificationId, string rawOperationToken, string operationTokenHash);
}
