using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantPOS.Data.Entities;

public class MenuItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [MaxLength(100)]
    public string? Barcode { get; set; }

    public int StockQuantity { get; set; }

    public int PreparationTimeMinutes { get; set; }

    [MaxLength(500)]
    public string? ImagePath { get; set; }
}
