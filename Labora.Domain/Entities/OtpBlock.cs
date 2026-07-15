using Labora.Domain.Common;
using Labora.Domain.Enums;

namespace Labora.Domain.Entities;

public class OtpBlock : BaseEntity
{
    public OtpBlockType BlockType { get; set; }
    public string ScopeKey { get; set; } = string.Empty;
    public OtpBlockReason BlockReason { get; set; }
    public int ViolationCount { get; set; } = 0;
    public DateTime ExpiresAt { get; set; }
    public DateTime? LastViolationAt { get; set; }
    public uint Version { get; set; }
}
