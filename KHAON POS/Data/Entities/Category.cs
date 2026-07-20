using System.ComponentModel.DataAnnotations;

namespace KHAONPOS.Data.Entities;

public class Category
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string IconName { get; set; } = "Food"; // MaterialDesign Icon name
}
