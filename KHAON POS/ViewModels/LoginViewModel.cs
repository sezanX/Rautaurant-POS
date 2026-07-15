using System;
using System.Linq;
using System.Windows.Input;
using RestaurantPOS.Data;

namespace RestaurantPOS.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly AppDbContext _context;

    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    private bool _isRoleSelected;
    public bool IsRoleSelected
    {
        get => _isRoleSelected;
        set => SetProperty(ref _isRoleSelected, value);
    }

    private string _selectedRole = string.Empty;
    public string SelectedRole
    {
        get => _selectedRole;
        set => SetProperty(ref _selectedRole, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand SelectRoleCommand { get; }
    public ICommand BackCommand { get; }

    public static event EventHandler<string>? LoginSuccessful;

    public LoginViewModel(AppDbContext context)
    {
        _context = context;
        LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
        SelectRoleCommand = new RelayCommand<string>(ExecuteSelectRole);
        BackCommand = new RelayCommand(ExecuteBack);
    }

    private void ExecuteSelectRole(string? role)
    {
        if (!string.IsNullOrEmpty(role))
        {
            SelectedRole = role;
            IsRoleSelected = true;
            ErrorMessage = string.Empty;
        }
    }

    private void ExecuteBack(object? parameter)
    {
        IsRoleSelected = false;
        SelectedRole = string.Empty;
        Username = string.Empty;
        Password = string.Empty;
        ErrorMessage = string.Empty;
    }

    private bool CanExecuteLogin(object? parameter)
    {
        return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
    }

    private void ExecuteLogin(object? parameter)
    {
        var trimmedUsername = Username?.Trim();
        var trimmedPassword = Password?.Trim();

        var user = _context.Users.FirstOrDefault(u => u.Username == trimmedUsername && u.PasswordHash == trimmedPassword && u.Role == SelectedRole);
        if (user != null)
        {
            ErrorMessage = string.Empty;
            LoginSuccessful?.Invoke(this, user.Role);
            
            // Clear fields after login
            ExecuteBack(null);
        }
        else
        {
            ErrorMessage = "Invalid username or password for this role";
        }
    }
}
