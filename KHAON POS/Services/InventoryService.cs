using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KHAONPOS.Data;
using KHAONPOS.Data.Entities;

namespace KHAONPOS.Services;

public class InventoryService(AppDbContext context) : IInventoryService
{
    private readonly AppDbContext _context = context;

    public Task<List<Category>> GetCategoriesAsync()
    {
        return _context.Categories.ToListAsync();
    }

    public Task<List<MenuItem>> GetMenuItemsAsync()
    {
        return _context.MenuItems.Include(m => m.Category).ToListAsync();
    }

    public Task<List<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId)
    {
        return _context.MenuItems
            .Include(m => m.Category)
            .Where(m => m.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task UpdateStockAsync(int menuItemId, int quantityChange)
    {
        var item = await _context.MenuItems.FindAsync(menuItemId);
        if (item != null)
        {
            item.StockQuantity += quantityChange;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Category> AddCategoryAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Success, string Message)> DeleteCategoryAsync(int categoryId)
    {
        var category = await _context.Categories.FindAsync(categoryId);
        if (category == null)
            return (false, "Category not found.");

        var hasMenuItems = await _context.MenuItems.AnyAsync(m => m.CategoryId == categoryId);
        if (hasMenuItems)
            return (false, "Cannot delete category because menu items are associated with it.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return (true, "Category deleted successfully.");
    }
}
