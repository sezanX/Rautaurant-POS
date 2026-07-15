namespace RestaurantPOS.Data.Models;

public class TopItemDTO
{
    public string Name { get; set; } = string.Empty;
    public string IconName { get; set; } = "Food";
    public int UnitsSold { get; set; }
}
