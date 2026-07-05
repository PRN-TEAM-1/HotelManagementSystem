using System.Windows;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using Services.Interfaces;
using WPF.Commands;
using WPF.Helpers;
using WPF.Views;

namespace WPF.ViewModels;

public sealed class UserManagementViewModel : BaseViewModel
{
    private readonly IUserManagementService _userManagementService;
    private readonly ICurrentUserService _currentUserService;
    private readonly DialogService _dialogService;
    private readonly List<LookupItemDto> _statusOptions;

    private List<LookupItemDto> _roleOptions = new();
    private List<UserListItemDto> _users = new();
    private UserListItemDto? _selectedUser;
    private string _searchTerm = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;
    private bool _isInitialized;

    public UserManagementViewModel(
        IUserManagementService userManagementService,
        ICurrentUserService currentUserService,
        DialogService dialogService)
    {
        _userManagementService = userManagementService ?? throw new ArgumentNullException(nameof(userManagementService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _statusOptions = CreateStatusOptions();

        LoadUsersCommand = new AsyncRelayCommand(LoadUsersAsync, CanExecuteWhenReady);
        SearchUsersCommand = new AsyncRelayCommand(SearchUsersAsync, CanExecuteWhenReady);
        ClearSearchCommand = new AsyncRelayCommand(ClearSearchAsync, CanClearSearch);
        AddUserCommand = new AsyncRelayCommand(AddUserAsync, CanExecuteWhenReady);
        EditUserCommand = new AsyncRelayCommand(EditUserAsync, CanEditSelectedUser);
        ActivateUserCommand = new AsyncRelayCommand(
            () => ChangeSelectedStatusAsync(UserStatus.Active, "activate"),
            CanActivateSelectedUser);
        InactivateUserCommand = new AsyncRelayCommand(
            () => ChangeSelectedStatusAsync(UserStatus.Inactive, "inactivate"),
            CanInactivateSelectedUser);
        LockUserCommand = new AsyncRelayCommand(
            () => ChangeSelectedStatusAsync(UserStatus.Locked, "lock"),
            CanLockSelectedUser);
        ClearMessagesCommand = new RelayCommand(ClearMessages);
    }

    public override string Title => "User Management";

    public override string Description => "Create, update and control staff login access";

    public List<UserListItemDto> Users
    {
        get => _users;
        private set
        {
            if (SetProperty(ref _users, value))
            {
                OnPropertyChanged(nameof(EmptyStateMessage));
            }
        }
    }

    public UserListItemDto? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (SetProperty(ref _selectedUser, value))
            {
                RaiseSelectionCommandStates();
            }
        }
    }

    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (SetProperty(ref _searchTerm, value))
            {
                SearchUsersCommand.RaiseCanExecuteChanged();
                ClearSearchCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string SuccessMessage
    {
        get => _successMessage;
        private set => SetProperty(ref _successMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(CanEdit));
                OnPropertyChanged(nameof(EmptyStateMessage));
                RaiseAllCommandStates();
            }
        }
    }

    public bool CanEdit => !IsBusy;

    public string EmptyStateMessage
    {
        get
        {
            if (IsBusy || Users.Count > 0)
            {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace(SearchTerm)
                ? "No staff accounts found."
                : "No staff accounts match the current search.";
        }
    }

    public AsyncRelayCommand LoadUsersCommand { get; }

    public AsyncRelayCommand SearchUsersCommand { get; }

    public AsyncRelayCommand ClearSearchCommand { get; }

    public AsyncRelayCommand AddUserCommand { get; }

    public AsyncRelayCommand EditUserCommand { get; }

    public AsyncRelayCommand ActivateUserCommand { get; }

    public AsyncRelayCommand InactivateUserCommand { get; }

    public AsyncRelayCommand LockUserCommand { get; }

    public RelayCommand ClearMessagesCommand { get; }

    public override async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        await EnsureRoleOptionsAsync();
        await LoadUsersAsync();
        _isInitialized = true;
    }

    private bool CanExecuteWhenReady()
    {
        return !IsBusy;
    }

    private bool CanClearSearch()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(SearchTerm);
    }

    private bool CanEditSelectedUser()
    {
        return !IsBusy && SelectedUser is not null;
    }

    private bool CanActivateSelectedUser()
    {
        return !IsBusy && SelectedUser is not null && !HasSelectedStatus(UserStatus.Active);
    }

    private bool CanInactivateSelectedUser()
    {
        return !IsBusy && SelectedUser is not null && !HasSelectedStatus(UserStatus.Inactive);
    }

    private bool CanLockSelectedUser()
    {
        return !IsBusy && SelectedUser is not null && !HasSelectedStatus(UserStatus.Locked);
    }

    private async Task LoadUsersAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var result = await _userManagementService.GetUsersAsync(
                _currentUserService.User,
                SearchTerm);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                Users = new();
                SelectedUser = null;
                return;
            }

            var selectedUserId = SelectedUser?.UserId;
            Users = result.Data ?? new();
            SelectedUser = selectedUserId.HasValue
                ? Users.FirstOrDefault(user => user.UserId == selectedUserId.Value)
                : null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading users: {ex.Message}";
            Users = new();
            SelectedUser = null;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SearchUsersAsync()
    {
        await LoadUsersAsync();
    }

    private async Task ClearSearchAsync()
    {
        SearchTerm = string.Empty;
        await LoadUsersAsync();
    }

    private async Task AddUserAsync()
    {
        if (!await EnsureRoleOptionsAsync())
        {
            return;
        }

        var dialogViewModel = UserDialogViewModel.CreateForCreate(_roleOptions, _statusOptions);

        if (!ShowUserDialog(dialogViewModel))
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var result = await _userManagementService.CreateUserAsync(
                dialogViewModel.ToCreateRequest(),
                _currentUserService.User);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                return;
            }

            SuccessMessage = $"Account '{result.Data?.Username}' created successfully.";
            await LoadUsersAsync();
            SelectedUser = result.Data is null
                ? null
                : Users.FirstOrDefault(user => user.UserId == result.Data.UserId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error creating user: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task EditUserAsync()
    {
        if (SelectedUser is null)
        {
            ErrorMessage = "Please select a user to edit.";
            return;
        }

        if (!await EnsureRoleOptionsAsync())
        {
            return;
        }

        var dialogViewModel = UserDialogViewModel.CreateForEdit(
            SelectedUser,
            _roleOptions,
            _statusOptions);

        if (!ShowUserDialog(dialogViewModel))
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var result = await _userManagementService.UpdateUserAsync(
                dialogViewModel.ToUpdateRequest(),
                _currentUserService.User);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                return;
            }

            SuccessMessage = $"Account '{result.Data?.Username}' updated successfully.";
            await LoadUsersAsync();
            SelectedUser = result.Data is null
                ? null
                : Users.FirstOrDefault(user => user.UserId == result.Data.UserId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error updating user: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ChangeSelectedStatusAsync(UserStatus status, string actionName)
    {
        if (SelectedUser is null)
        {
            ErrorMessage = "Please select a user first.";
            return;
        }

        var message = $"Are you sure you want to {actionName} account '{SelectedUser.Username}'?";
        if (!_dialogService.Confirm(message, "Confirm account status"))
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var result = await _userManagementService.ChangeStatusAsync(
                new ChangeUserStatusRequestDto
                {
                    UserId = SelectedUser.UserId,
                    Status = status
                },
                _currentUserService.User);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                return;
            }

            SuccessMessage = $"Account '{result.Data?.Username}' is now {status}.";
            await LoadUsersAsync();
            SelectedUser = result.Data is null
                ? null
                : Users.FirstOrDefault(user => user.UserId == result.Data.UserId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error changing status: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<bool> EnsureRoleOptionsAsync()
    {
        if (_roleOptions.Count > 0)
        {
            return true;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _userManagementService.GetRoleLookupsAsync(_currentUserService.User);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                return false;
            }

            _roleOptions = result.Data ?? new();
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading roles: {ex.Message}";
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ShowUserDialog(UserDialogViewModel dialogViewModel)
    {
        var owner = Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(window => window.IsActive)
            ?? Application.Current.MainWindow;

        var dialog = new UserDialog
        {
            DataContext = dialogViewModel,
            Owner = owner
        };

        return dialog.ShowDialog() == true;
    }

    private bool HasSelectedStatus(UserStatus status)
    {
        return Enum.TryParse<UserStatus>(SelectedUser?.Status, ignoreCase: true, out var selectedStatus)
            && selectedStatus == status;
    }

    private void ClearMessages()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    private void RaiseAllCommandStates()
    {
        LoadUsersCommand.RaiseCanExecuteChanged();
        SearchUsersCommand.RaiseCanExecuteChanged();
        ClearSearchCommand.RaiseCanExecuteChanged();
        AddUserCommand.RaiseCanExecuteChanged();
        RaiseSelectionCommandStates();
    }

    private void RaiseSelectionCommandStates()
    {
        EditUserCommand.RaiseCanExecuteChanged();
        ActivateUserCommand.RaiseCanExecuteChanged();
        InactivateUserCommand.RaiseCanExecuteChanged();
        LockUserCommand.RaiseCanExecuteChanged();
    }

    private static List<LookupItemDto> CreateStatusOptions()
    {
        return Enum.GetValues<UserStatus>()
            .Select(status => new LookupItemDto
            {
                Id = (int)status,
                Value = status.ToString(),
                DisplayName = status.ToString()
            })
            .ToList();
    }
}
