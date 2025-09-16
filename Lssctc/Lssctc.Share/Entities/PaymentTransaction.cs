using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class PaymentTransaction
{
    public int Id { get; set; }

    public int PaymentId { get; set; }

    public int TransactionId { get; set; }

    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Payment Payment { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
