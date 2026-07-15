using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantPOS.Data.Entities;

namespace RestaurantPOS.Services;

public interface IInventoryService
{
    Task<List<Category>> GetCategoriesAsync();
    Task<List<MenuItem>> GetMenuItemsAsync();
    Task<List<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId);
    Task<MenuItem?> GetMenuItemByBarcodeAsync(string barcode);
    Task UpdateStockAsync(int menuItemId, int quantityChange);
}
