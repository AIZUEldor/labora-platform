using Labora.Domain.Enums;

namespace Labora.Application.Common;

public sealed record OtpBlockVerdict(OtpBlockType BlockType, OtpBlockReason BlockReason, DateTime ExpiresAtUtc)
{
    public static implicit operator OtpBlockVerdict((OtpBlockType BlockType, OtpBlockReason BlockReason, DateTime ExpiresAt) tuple) =>
        new(tuple.BlockType, tuple.BlockReason, tuple.ExpiresAt);
}
