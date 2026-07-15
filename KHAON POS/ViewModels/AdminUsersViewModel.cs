using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using RestaurantPOS.Data;
using RestaurantPOS.Data.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace RestaurantPOS.ViewModels;

public class AdminUsersViewModel : BaseViewModel
{
    private readonly AppDbContext _context;

    public ObservableCollection<User> Users { get; } = new();

    private User? _selectedUser;
    public User? SelectedUser
    {
        get => _selectedUser;
        set => SetProperty(ref _selectedUser, value);
    }

    private string _newUsername = "";
    public string NewUsername
    {
        get => _newUsername;
        set => SetProperty(ref _newUsername, value);
    }

    private string _newPassword = "";
    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    private string _newRole = "Cashier";
    public string NewRole
    {
        get => _newRole;
        set => SetProperty(ref _newRole, value);
    }

    public ObservableCollection<string> AvailableRoles { get; } = new() { "Admin", "Cashier", "Kitchen" };

    public ICommand AddUserCommand { get; }
    public ICommand DeleteUserCommand { get; }
    public ICommand SaveChangesCommand { get; }
    public ICommand RefreshCommand { get; }

    public AdminUsersViewModel(AppDbContext context)
    {
        _context = context;

        AddUserCommand = new RelayCommand(async _ => await AddUserAsync());
        DeleteUserCommand = new RelayCommand(async _ => await DeleteSelectedUserAsync(), _ => SelectedUser != null);
        SaveChangesCommand = new RelayCommand(async _ => await SaveChangesAsync());
        RefreshCommand = new RelayCommand(async _ => await LoadUsersAsync());

        _ = LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        Users.Clear();
        var users = await _context.Users.ToListAsync();
        foreach (var user in users)
        {
            Users.Add(user);
        }
    }

    private async Task AddUserAsync()
    {
        if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(NewRole))
            return;

        var user = new User
        {
            Username = NewUsername,
            PasswordHash = NewPassword, // In a real app, hash this properly
            Role = NewRole
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        NewUsername = "";
        NewPassword = "";
        NewRole = "Cashier";
        
        await LoadUsersAsync();
    }

    private async Task DeleteSelectedUserAsync()
    {
        if (SelectedUser == null) return;

        _context.Users.Remove(SelectedUser);
        await _context.SaveChangesAsync();
        await LoadUsersAsync();
    }

    private async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
        await LoadUsersAsync();
    }
}
