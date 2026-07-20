using System.Collections.Generic;
using System.Threading.Tasks;
using KHAONPOS.Data.Entities;

namespace KHAONPOS.Services;

public interface IInventoryService
{
    Task<List<Category>> GetCategoriesAsync();
    Task<List<MenuItem>> GetMenuItemsAsync();
    Task<List<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId);
    Task UpdateStockAsync(int menuItemId, int quantityChange);
    Task<Category> AddCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task<(bool Success, string Message)> DeleteCategoryAsync(int categoryId);
}
