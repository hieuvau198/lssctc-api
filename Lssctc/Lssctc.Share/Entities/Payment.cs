using System;
using System.Collections.Generic;

namespace Lssctc.Share.Entities;

public partial class Payment
{
    public int Id { get; set; }

    public int TraineeId { get; set; }

    public decimal Amount { get; set; }

    public string? Currency { get; set; }

    public string? PaymentMethod { get; set; }

    public DateTime PaymentDate { get; set; }

    public int Status { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    public virtual Trainee Trainee { get; set; } = null!;
}
