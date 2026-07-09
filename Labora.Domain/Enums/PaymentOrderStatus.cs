namespace Labora.Domain.Enums;

public enum PaymentOrderStatus
{
    Pending = 1,      // To'lov kutilmoqda
    Paid = 2,         // To'landi
    Failed = 3,       // Muvaffaqiyatsiz
    Cancelled = 4,    // Bekor qilindi
    Expired = 5       // Muddati o'tgan
}
