using Labora.Domain.Enums;

namespace Labora.Domain.Exceptions;

public class OtpBlockedException : Exception
{
    public OtpBlockType BlockType { get; }
    public OtpBlockReason BlockReason { get; }
    public DateTime ExpiresAtUtc { get; }

    public OtpBlockedException(OtpBlockType blockType, OtpBlockReason blockReason, DateTime expiresAtUtc)
        : base($"OTP requests are blocked for {blockType} scope until {expiresAtUtc:O} (reason: {blockReason}).")
    {
        BlockType = blockType;
        BlockReason = blockReason;
        ExpiresAtUtc = expiresAtUtc;
    }
}
