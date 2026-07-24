using Labora.Domain.Common;
using Labora.Domain.Enums;

namespace Labora.Domain.Entities;

public class PaymeTransaction : BaseEntity
{
    public Guid PaymentOrderId { get; set; }

    /// <summary>Payme's own transaction identifier (the "id" Payme sends/expects in params).</summary>
    public string PaymeTransactionId { get; set; } = string.Empty;

    /// <summary>The raw "account" value as received from Payme (e.g. account.order_id), kept
    /// separately from PaymentOrderId for audit/reconciliation even if it fails to resolve.</summary>
    public string AccountReference { get; set; } = string.Empty;

    /// <summary>Payme's own transaction-creation timestamp, from CreateTransaction request "time".
    /// Unit (ms vs s) is not confirmed by official docs - stored as received, unconverted.</summary>
    public long PaymeTransactionTime { get; set; }

    /// <summary>Amount as requested by Payme in CreateTransaction, in tiyin.</summary>
    public long RequestedAmountTiyin { get; set; }

    /// <summary>ALP Jobs' own merchant-side timestamps - what is returned as create_time/perform_time/
    /// cancel_time. Must stay stable across repeated calls (idempotency), never regenerated per call.</summary>
    public long? MerchantCreateTime { get; set; }
    public long? MerchantPerformTime { get; set; }
    public long? MerchantCancelTime { get; set; }

    public PaymeTransactionInternalStatus InternalStatus { get; set; } = PaymeTransactionInternalStatus.Created;

    /// <summary>The numeric "state" value returned to Payme. Exact code meanings are not confirmed
    /// by official documentation - nullable until first computed.</summary>
    public int? PaymeStateCode { get; set; }

    /// <summary>Cancellation reason as sent by Payme in CancelTransaction's "reason" field.</summary>
    public int? CancelReason { get; set; }

    /// <summary>PostgreSQL xmin optimistic concurrency token, same pattern as OtpVerification.Version.</summary>
    public uint Version { get; set; }

    // Navigation property
    public PaymentOrder PaymentOrder { get; set; } = null!;
}
