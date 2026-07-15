using System.ComponentModel.DataAnnotations;

namespace RestaurantPOS.Data.Entities;

public class Table
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TableNumber { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Available"; // Available, Occupied, Reserved

    public int Capacity { get; set; }
}
