using System.Collections.ObjectModel;
using System.Net.Mail;
using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;

namespace WPF.ViewModels;

public sealed class UserDialogViewModel : BaseViewModel
{
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _fullName = string.Empty;
    private string _email = string.Empty;
    private string _phoneNumber = string.Empty;
    private string _errorMessage = string.Empty;
    private LookupItemDto? _selectedRole;
    private LookupItemDto? _selectedStatus;

    private UserDialogViewModel(
        bool isCreateMode,
        int userId,
        IEnumerable<LookupItemDto> roleOptions,
        IEnumerable<LookupItemDto> statusOptions)
    {
        IsCreateMode = isCreateMode;
        UserId = userId;
        RoleOptions = new ObservableCollection<LookupItemDto>(roleOptions);
        StatusOptions = new ObservableCollection<LookupItemDto>(statusOptions);
    }

    public int UserId { get; }

    public bool IsCreateMode { get; }

    public string DialogTitle => IsCreateMode ? "Add Staff Account" : "Edit Staff Account";

    public string DialogDescription => IsCreateMode
        ? "Create an internal staff login for Admin, Manager or Receptionist."
        : "Update profile, role and account status for this staff user.";

    public bool IsPasswordRequired => IsCreateMode;

    public ObservableCollection<LookupItemDto> RoleOptions { get; }

    public ObservableCollection<LookupItemDto> StatusOptions { get; }

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => SetProperty(ref _phoneNumber, value);
    }

    public LookupItemDto? SelectedRole
    {
        get => _selectedRole;
        set => SetProperty(ref _selectedRole, value);
    }

    public LookupItemDto? SelectedStatus
    {
        get => _selectedStatus;
        set => SetProperty(ref _selectedStatus, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public static UserDialogViewModel CreateForCreate(
        IEnumerable<LookupItemDto> roleOptions,
        IEnumerable<LookupItemDto> statusOptions)
    {
        var viewModel = new UserDialogViewModel(true, 0, roleOptions, statusOptions);
        viewModel.SelectedRole = viewModel.RoleOptions.FirstOrDefault(role =>
                string.Equals(role.Value, RoleName.Receptionist.ToString(), StringComparison.OrdinalIgnoreCase))
            ?? viewModel.RoleOptions.FirstOrDefault();
        viewModel.SelectedStatus = viewModel.StatusOptions.FirstOrDefault(status =>
                string.Equals(status.Value, UserStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase))
            ?? viewModel.StatusOptions.FirstOrDefault();

        return viewModel;
    }

    public static UserDialogViewModel CreateForEdit(
        UserListItemDto user,
        IEnumerable<LookupItemDto> roleOptions,
        IEnumerable<LookupItemDto> statusOptions)
    {
        ArgumentNullException.ThrowIfNull(user);

        var viewModel = new UserDialogViewModel(false, user.UserId, roleOptions, statusOptions)
        {
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber ?? string.Empty
        };

        viewModel.SelectedRole = viewModel.RoleOptions.FirstOrDefault(role => role.Id == user.RoleId)
            ?? viewModel.RoleOptions.FirstOrDefault();
        viewModel.SelectedStatus = viewModel.StatusOptions.FirstOrDefault(status =>
                string.Equals(status.Value, user.Status, StringComparison.OrdinalIgnoreCase))
            ?? viewModel.StatusOptions.FirstOrDefault();

        return viewModel;
    }

    public bool ValidateForSave()
    {
        var errors = new List<string>();
        var username = Username.Trim();
        var fullName = FullName.Trim();
        var email = Email.Trim();
        var phoneNumber = PhoneNumber.Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            errors.Add("Username is required.");
        }
        else if (username.Length is < ValidationRules.UsernameMinLength or > ValidationRules.UsernameMaxLength)
        {
            errors.Add($"Username must be {ValidationRules.UsernameMinLength}-{ValidationRules.UsernameMaxLength} characters.");
        }

        if (IsCreateMode)
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                errors.Add("Password is required.");
            }
            else if (Password.Length < ValidationRules.PasswordMinLength)
            {
                errors.Add($"Password must be at least {ValidationRules.PasswordMinLength} characters.");
            }
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            errors.Add("Full name is required.");
        }
        else if (fullName.Length > ValidationRules.FullNameMaxLength)
        {
            errors.Add($"Full name cannot exceed {ValidationRules.FullNameMaxLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add("Email is required.");
        }
        else if (email.Length > ValidationRules.EmailMaxLength || !IsValidEmail(email))
        {
            errors.Add("Email is not valid.");
        }

        if (!string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber.Length > ValidationRules.PhoneNumberMaxLength)
        {
            errors.Add($"Phone number cannot exceed {ValidationRules.PhoneNumberMaxLength} characters.");
        }

        if (SelectedRole?.Id is null or <= 0)
        {
            errors.Add("Role is required.");
        }

        if (!TryGetSelectedStatus(out _))
        {
            errors.Add("Status is required.");
        }

        ErrorMessage = string.Join(Environment.NewLine, errors);
        return errors.Count == 0;
    }

    public CreateUserRequestDto ToCreateRequest()
    {
        return new CreateUserRequestDto
        {
            RoleId = SelectedRole?.Id ?? 0,
            Username = Username.Trim(),
            Password = Password,
            FullName = FullName.Trim(),
            Email = Email.Trim(),
            PhoneNumber = NormalizeOptional(PhoneNumber),
            Status = GetSelectedStatusOrDefault()
        };
    }

    public UpdateUserRequestDto ToUpdateRequest()
    {
        return new UpdateUserRequestDto
        {
            UserId = UserId,
            RoleId = SelectedRole?.Id ?? 0,
            Username = Username.Trim(),
            FullName = FullName.Trim(),
            Email = Email.Trim(),
            PhoneNumber = NormalizeOptional(PhoneNumber),
            Status = GetSelectedStatusOrDefault()
        };
    }

    private UserStatus GetSelectedStatusOrDefault()
    {
        return TryGetSelectedStatus(out var status) ? status : UserStatus.Active;
    }

    private bool TryGetSelectedStatus(out UserStatus status)
    {
        return Enum.TryParse(SelectedStatus?.Value, ignoreCase: true, out status)
            && Enum.IsDefined(status);
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new MailAddress(email);
            return string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
