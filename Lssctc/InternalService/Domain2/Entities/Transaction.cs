using System;
using System.Collections.Generic;

namespace InternalService.Domain2.Entities;

public partial class Transaction
{
    public int Id { get; set; }

    public string TransactionCode { get; set; } = null!;

    public string Message { get; set; } = null!;

    public string PayerName { get; set; } = null!;

    public string ReceiverName { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? Currency { get; set; }

    public string? TransactionType { get; set; }

    public string? Description { get; set; }

    public int Status { get; set; }

    public DateTime IssuedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public int? PayerId { get; set; }

    public int? ReceiverId { get; set; }

    public string? Note { get; set; }

    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    public virtual ICollection<TransactionProgram> TransactionPrograms { get; set; } = new List<TransactionProgram>();
}
