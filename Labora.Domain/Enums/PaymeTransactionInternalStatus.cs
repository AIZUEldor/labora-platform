namespace Labora.Domain.Enums;

/// <summary>
/// ALP Jobs' own lifecycle understanding of a Payme transaction, independent of Payme's numeric
/// "state" wire encoding (which official documentation does not fully confirm - see PaymeTransaction).
/// </summary>
public enum PaymeTransactionInternalStatus
{
    Created = 1,
    Performed = 2,
    Cancelled = 3
}
