using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Data;
using RestaurantPOS.Data.Entities;

namespace RestaurantPOS.Services;

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _context;

    public InventoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories.ToListAsync();
    }

    public async Task<List<MenuItem>> GetMenuItemsAsync()
    {
        return await _context.MenuItems.Include(m => m.Category).ToListAsync();
    }

    public async Task<List<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId)
    {
        return await _context.MenuItems
            .Include(m => m.Category)
            .Where(m => m.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<MenuItem?> GetMenuItemByBarcodeAsync(string barcode)
    {
        return await _context.MenuItems
            .Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Barcode == barcode);
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
}
