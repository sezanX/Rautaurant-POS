using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using RestaurantPOS.Data;
using RestaurantPOS.Data.Entities;
using RestaurantPOS.Services;

namespace RestaurantPOS.ViewModels;

public class AdminInventoryViewModel : BaseViewModel
{
    private readonly AppDbContext _context;
    private readonly IInventoryService _inventoryService;

    public ObservableCollection<MenuItem> MenuItems { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();

    private MenuItem? _selectedItem;
    public MenuItem? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                if (value != null)
                {
                    NewName = value.Name;
                    NewDescription = value.Description ?? "";
                    NewPrice = value.Price;
                    NewCategoryId = value.CategoryId;
                    NewPrepTime = value.PreparationTimeMinutes;
                    NewImagePath = value.ImagePath ?? "";
                }
                else
                {
                    ClearForm();
                }
            }
        }
    }

    private string _newName = "";
    public string NewName
    {
        get => _newName;
        set => SetProperty(ref _newName, value);
    }

    private string _newDescription = "";
    public string NewDescription
    {
        get => _newDescription;
        set => SetProperty(ref _newDescription, value);
    }

    private decimal _newPrice;
    public decimal NewPrice
    {
        get => _newPrice;
        set => SetProperty(ref _newPrice, value);
    }

    private int _newCategoryId;
    public int NewCategoryId
    {
        get => _newCategoryId;
        set => SetProperty(ref _newCategoryId, value);
    }

    private int _newPrepTime;
    public int NewPrepTime
    {
        get => _newPrepTime;
        set => SetProperty(ref _newPrepTime, value);
    }

    private string _newImagePath = "";
    public string NewImagePath
    {
        get => _newImagePath;
        set => SetProperty(ref _newImagePath, value);
    }

    public ICommand SaveItemCommand { get; }
    public ICommand DeleteItemCommand { get; }
    public ICommand UploadPhotoCommand { get; }
    public ICommand ClearFormCommand { get; }

    public AdminInventoryViewModel(AppDbContext context, IInventoryService inventoryService)
    {
        _context = context;
        _inventoryService = inventoryService;

        SaveItemCommand = new RelayCommand(async _ => await SaveItemAsync());
        DeleteItemCommand = new RelayCommand(async _ => await DeleteSelectedItemAsync(), _ => SelectedItem != null);
        UploadPhotoCommand = new RelayCommand(_ => UploadPhoto());
        ClearFormCommand = new RelayCommand(_ => ClearForm());

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var categories = await _context.Categories.ToListAsync();
        Categories.Clear();
        foreach (var c in categories) Categories.Add(c);

        if (Categories.Any())
        {
            NewCategoryId = Categories.First().Id;
        }

        var items = await _context.MenuItems.Include(m => m.Category).ToListAsync();
        MenuItems.Clear();
        foreach (var i in items) MenuItems.Add(i);
    }

    private void UploadPhoto()
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select Product Photo",
            Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                var sourcePath = openFileDialog.FileName;
                var imagesFolder = Path.Combine(Environment.CurrentDirectory, "Assets", "Images");
                
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(sourcePath);
                var destinationPath = Path.Combine(imagesFolder, fileName);

                File.Copy(sourcePath, destinationPath, true);

                NewImagePath = destinationPath;
            }
            catch (Exception ex)
            {
                // Ignore for now
            }
        }
    }

    private async Task SaveItemAsync()
    {
        if (string.IsNullOrWhiteSpace(NewName)) return;

        if (SelectedItem == null)
        {
            // Add new
            var item = new MenuItem
            {
                Name = NewName,
                Description = NewDescription,
                Price = NewPrice,
                CategoryId = NewCategoryId,
                PreparationTimeMinutes = NewPrepTime,
                ImagePath = NewImagePath,
                StockQuantity = 100 // default
            };
            _context.MenuItems.Add(item);
        }
        else
        {
            // Update existing
            SelectedItem.Name = NewName;
            SelectedItem.Description = NewDescription;
            SelectedItem.Price = NewPrice;
            SelectedItem.CategoryId = NewCategoryId;
            SelectedItem.PreparationTimeMinutes = NewPrepTime;
            SelectedItem.ImagePath = NewImagePath;
            _context.MenuItems.Update(SelectedItem);
        }

        await _context.SaveChangesAsync();
        ClearForm();
        await LoadDataAsync();
    }

    private async Task DeleteSelectedItemAsync()
    {
        if (SelectedItem == null) return;

        _context.MenuItems.Remove(SelectedItem);
        await _context.SaveChangesAsync();
        ClearForm();
        await LoadDataAsync();
    }

    private void ClearForm()
    {
        SelectedItem = null;
        NewName = "";
        NewDescription = "";
        NewPrice = 0;
        if (Categories.Any()) NewCategoryId = Categories.First().Id;
        NewPrepTime = 0;
        NewImagePath = "";
    }
}
