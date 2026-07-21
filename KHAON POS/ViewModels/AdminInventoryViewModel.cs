using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using KHAONPOS.Data;
using KHAONPOS.Data.Entities;

namespace KHAONPOS.ViewModels;

public class AdminInventoryViewModel : BaseViewModel
{
    private readonly AppDbContext _context;

    public ObservableCollection<MenuItem> MenuItems { get; } = [];
    public ObservableCollection<Category> Categories { get; } = [];

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
                    NewImageData = value.ImageData;
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
        set
        {
            if (SetProperty(ref _newImagePath, value))
            {
                OnPropertyChanged(nameof(NewDisplayImage));
            }
        }
    }

    private byte[]? _newImageData;
    public byte[]? NewImageData
    {
        get => _newImageData;
        set
        {
            if (SetProperty(ref _newImageData, value))
            {
                OnPropertyChanged(nameof(NewDisplayImage));
            }
        }
    }
    
    public object? NewDisplayImage => NewImageData != null ? NewImageData : (string.IsNullOrWhiteSpace(NewImagePath) ? null : NewImagePath);

    public ICommand SaveItemCommand { get; }
    public ICommand DeleteItemCommand { get; }
    public ICommand UploadPhotoCommand { get; }
    public ICommand ClearFormCommand { get; }

    public AdminInventoryViewModel(AppDbContext context)
    {
        _context = context;

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

        if (openFileDialog.ShowDialog() is true)
        {
            try
            {
                var sourcePath = openFileDialog.FileName;
                
                // Read the image file into a byte array for database storage
                NewImageData = File.ReadAllBytes(sourcePath);
                
                // Clear the ImagePath to force the UI to use the new byte array
                NewImagePath = "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to upload photo: {ex.Message}");
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
                ImageData = NewImageData,
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
            SelectedItem.ImageData = NewImageData;
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
        NewImageData = null;
    }
}
