using System;
using System.Linq;
using System.Windows.Input;
using KHAONPOS.Data;
using KHAONPOS.Data.Entities;

namespace KHAONPOS.ViewModels;

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
    public ICommand ForgotPasswordCommand { get; }

    public static event EventHandler<string>? LoginSuccessful;
    public static User? CurrentUser { get; private set; }

    public LoginViewModel(AppDbContext context)
    {
        _context = context;
        LoginCommand = new RelayCommand(ExecuteLogin, CanExecuteLogin);
        SelectRoleCommand = new RelayCommand<string>(ExecuteSelectRole);
        BackCommand = new RelayCommand(ExecuteBack);
        ForgotPasswordCommand = new RelayCommand(_ => ExecuteForgotPassword());

        // Default role on login page load is Admin
        ExecuteSelectRole("Admin");
    }

    private void ExecuteSelectRole(string? role)
    {
        if (!string.IsNullOrEmpty(role))
        {
            SelectedRole = role;
            IsRoleSelected = true;
            ErrorMessage = string.Empty;

            switch (role)
            {
                case "Admin":
                    Username = "admin";
                    break;
                case "Cashier":
                    Username = "cashier";
                    break;
                case "Kitchen":
                    Username = "kitchen";
                    break;
            }
        }
    }

    private void ExecuteBack(object? parameter)
    {
        ExecuteSelectRole("Admin");
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

        var user = _context.Users.FirstOrDefault(u =>
            u.Role == SelectedRole &&
            u.PasswordHash == trimmedPassword &&
            (u.Username == trimmedUsername ||
             (u.Username == "cashier1" && trimmedUsername == "cashier") ||
             (u.Username == "kitchen1" && trimmedUsername == "kitchen") ||
             (u.Username == "cashier" && trimmedUsername == "cashier1") ||
             (u.Username == "kitchen" && trimmedUsername == "kitchen1")));

        if (user != null)
        {
            CurrentUser = user;
            ErrorMessage = string.Empty;
            LoginSuccessful?.Invoke(this, user.Role);

            // Clear fields and reset default role after login
            ExecuteBack(null);
        }
        else
        {
            ErrorMessage = "Invalid username or password for this role";
        }
    }

    private void ExecuteForgotPassword()
    {
        ErrorMessage = "Please contact your admin/manager";
    }
}
