using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using KHAONPOS.Data.Entities;
using KHAONPOS.Services;

namespace KHAONPOS.ViewModels;

public class AdminCategoryViewModel : BaseViewModel
{
    private readonly IInventoryService _inventoryService;

    public ObservableCollection<Category> Categories { get; } = [];

    private Category? _selectedCategory;
    public Category? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
            {
                if (value != null)
                {
                    CategoryName = value.Name;
                    CategoryIconName = value.IconName ?? "Food";
                    CategoryErrorMessage = "";
                }
                else
                {
                    ClearForm();
                }
            }
        }
    }

    private string _categoryName = "";
    public string CategoryName
    {
        get => _categoryName;
        set => SetProperty(ref _categoryName, value);
    }

    private string _categoryIconName = "Food";
    public string CategoryIconName
    {
        get => _categoryIconName;
        set => SetProperty(ref _categoryIconName, value);
    }

    private string _categoryErrorMessage = "";
    public string CategoryErrorMessage
    {
        get => _categoryErrorMessage;
        set => SetProperty(ref _categoryErrorMessage, value);
    }

    public ObservableCollection<string> AvailableIcons { get; } =
    [
        "Food", "Hamburger", "Pizza", "CupWater", "IceCream",
        "Silverware", "Coffee", "Cake", "Noodles", "Fish",
        "GlassCocktail", "FruitCherries", "BreadSlice"
    ];

    public ICommand SaveCategoryCommand { get; }
    public ICommand DeleteCategoryCommand { get; }
    public ICommand ClearFormCommand { get; }

    public AdminCategoryViewModel(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;

        SaveCategoryCommand = new RelayCommand(async _ => await SaveCategoryAsync());
        DeleteCategoryCommand = new RelayCommand(async _ => await DeleteSelectedCategoryAsync(), _ => SelectedCategory != null);
        ClearFormCommand = new RelayCommand(_ => ClearForm());

        _ = LoadCategoriesAsync();
    }

    public async Task LoadCategoriesAsync()
    {
        var list = await _inventoryService.GetCategoriesAsync();
        Categories.Clear();
        foreach (var c in list)
        {
            Categories.Add(c);
        }
    }

    private async Task SaveCategoryAsync()
    {
        CategoryErrorMessage = "";
        if (string.IsNullOrWhiteSpace(CategoryName))
        {
            CategoryErrorMessage = "Category name is required.";
            return;
        }

        var icon = string.IsNullOrWhiteSpace(CategoryIconName) ? "Food" : CategoryIconName.Trim();
        var name = CategoryName.Trim();

        if (SelectedCategory == null)
        {
            var newCategory = new Category
            {
                Name = name,
                IconName = icon
            };
            await _inventoryService.AddCategoryAsync(newCategory);
        }
        else
        {
            SelectedCategory.Name = name;
            SelectedCategory.IconName = icon;
            await _inventoryService.UpdateCategoryAsync(SelectedCategory);
        }

        ClearForm();
        await LoadCategoriesAsync();
    }

    private async Task DeleteSelectedCategoryAsync()
    {
        CategoryErrorMessage = "";
        if (SelectedCategory == null) return;

        var (success, message) = await _inventoryService.DeleteCategoryAsync(SelectedCategory.Id);
        if (!success)
        {
            CategoryErrorMessage = message;
            return;
        }

        ClearForm();
        await LoadCategoriesAsync();
    }

    private void ClearForm()
    {
        _selectedCategory = null;
        OnPropertyChanged(nameof(SelectedCategory));
        CategoryName = "";
        CategoryIconName = "Food";
        CategoryErrorMessage = "";
    }
}
