using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KHAONPOS.Data.Entities;

public class Payment
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    [Column(TypeName = "numeric(18,2)")]
    public decimal AmountPaid { get; set; }

    [Required]
    [MaxLength(20)]
    public string PaymentMethod { get; set; } = "Cash"; // Cash, Card

    public DateTime PaymentDate { get; set; } = DateTime.Now;
}
