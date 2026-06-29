using System.Collections.ObjectModel;
using BusinessObjects.Enums;
using Services.Interfaces;
using WPF.Commands;
using WPF.Helpers;

namespace WPF.ViewModels;

public sealed class MainWindowViewModel : BaseViewModel
{
    private readonly NavigationService _navigationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly DialogService _dialogService;
    private readonly RelayCommand _goBackCommand;
    private readonly RelayCommand _logoutCommand;
    private readonly RelayCommand<NavigationItemViewModel> _navigateCommand;

    private BaseViewModel? _currentViewModel;
    private string _currentPageTitle = "Hotel Management System";
    private string _currentPageDescription = "Role-aware operations shell";
    private string _sessionDisplayName = "No active session";
    private string _sessionRoleDisplay = "Public";
    private string _authenticationState = "Signed out";

    public MainWindowViewModel(
        NavigationService navigationService,
        ICurrentUserService currentUserService,
        DialogService dialogService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        NavigationItems = new ObservableCollection<NavigationItemViewModel>(CreateNavigationItems());

        _navigateCommand = new RelayCommand<NavigationItemViewModel>(NavigateTo, item => item?.IsVisible == true);
        _goBackCommand = new RelayCommand(GoBack, () => _navigationService.CanGoBack);
        _logoutCommand = new RelayCommand(Logout, () => _currentUserService.IsAuthenticated);

        NavigateCommand = _navigateCommand;
        GoBackCommand = _goBackCommand;
        LogoutCommand = _logoutCommand;

        _navigationService.NavigationChanged += OnNavigationChanged;
        _currentUserService.SessionChanged += OnSessionChanged;

        RefreshNavigationVisibility();
        SyncShellState();
    }

    public string AppTitle => "Hotel Management System";

    public string AppSubtitle => "Role-aware operations shell powered by EF Core auth";

    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; }

    public RelayCommand<NavigationItemViewModel> NavigateCommand { get; }

    public RelayCommand GoBackCommand { get; }

    public RelayCommand LogoutCommand { get; }

    public BaseViewModel? CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public string CurrentPageTitle
    {
        get => _currentPageTitle;
        private set => SetProperty(ref _currentPageTitle, value);
    }

    public string CurrentPageDescription
    {
        get => _currentPageDescription;
        private set => SetProperty(ref _currentPageDescription, value);
    }

    public string SessionDisplayName
    {
        get => _sessionDisplayName;
        private set => SetProperty(ref _sessionDisplayName, value);
    }

    public string SessionRoleDisplay
    {
        get => _sessionRoleDisplay;
        private set => SetProperty(ref _sessionRoleDisplay, value);
    }

    public string AuthenticationState
    {
        get => _authenticationState;
        private set => SetProperty(ref _authenticationState, value);
    }

    private void OnNavigationChanged(object? sender, EventArgs e)
    {
        SyncShellState();
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        RefreshNavigationVisibility();
        SyncShellState();
    }

    private IEnumerable<NavigationItemViewModel> CreateNavigationItems()
    {
        return
        [
            new NavigationItemViewModel(
                NavigationTargets.Workspace,
                "Core Workspace",
                "Architecture handoff and integration notes"),
            new NavigationItemViewModel(
                NavigationTargets.Session,
                "Current Session",
                "Authenticated user profile and access summary"),
            new NavigationItemViewModel(
                NavigationTargets.Administration,
                "Administration",
                "Users, rooms, room types and service catalog",
                [RoleName.Admin]),
            new NavigationItemViewModel(
                NavigationTargets.Operations,
                "Operations",
                "Booking, check-in, check-out and billing",
                [RoleName.Admin, RoleName.Receptionist]),
            new NavigationItemViewModel(
                NavigationTargets.Reports,
                "Reports",
                "Dashboard, occupancy and revenue insights",
                [RoleName.Admin, RoleName.Manager]),
            new NavigationItemViewModel(
                NavigationTargets.StyleGuide,
                "Style Guide",
                "Preview buttons, text inputs and data tables")
        ];
    }

    private void NavigateTo(NavigationItemViewModel? item)
    {
        if (item is null || !item.IsVisible)
        {
            return;
        }

        _navigationService.Navigate(item.Key);
    }

    private void GoBack()
    {
        while (_navigationService.CanGoBack)
        {
            if (!_navigationService.GoBack())
            {
                break;
            }

            if (IsCurrentViewVisible())
            {
                return;
            }
        }

        if (!IsCurrentViewVisible())
        {
            _navigationService.Navigate(NavigationTargets.Workspace, addToHistory: false);
        }
    }

    private void Logout()
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return;
        }

        if (_dialogService.Confirm("Sign out of the current account and return to the login window?", "Logout"))
        {
            _currentUserService.Clear();
        }
    }

    private void RefreshNavigationVisibility()
    {
        var roleName = _currentUserService.User?.RoleName;

        foreach (var item in NavigationItems)
        {
            item.IsVisible = item.IsAllowedFor(roleName);
        }

        if (!IsCurrentViewVisible() && _navigationService.CurrentKey is not null)
        {
            _navigationService.Navigate(NavigationTargets.Workspace, addToHistory: false);
        }
    }

    private bool IsCurrentViewVisible()
    {
        if (_navigationService.CurrentKey is null)
        {
            return true;
        }

        var currentItem = NavigationItems.FirstOrDefault(item =>
            string.Equals(item.Key, _navigationService.CurrentKey, StringComparison.OrdinalIgnoreCase));

        return currentItem?.IsVisible ?? true;
    }

    private void SyncShellState()
    {
        CurrentViewModel = _navigationService.CurrentViewModel;
        CurrentPageTitle = CurrentViewModel?.Title ?? AppTitle;
        CurrentPageDescription = CurrentViewModel?.Description ?? AppSubtitle;
        SessionDisplayName = _currentUserService.DisplayName;
        SessionRoleDisplay = _currentUserService.RoleDisplay;
        AuthenticationState = _currentUserService.IsAuthenticated ? "Signed in" : "Signed out";

        foreach (var item in NavigationItems)
        {
            item.IsSelected = string.Equals(item.Key, _navigationService.CurrentKey, StringComparison.OrdinalIgnoreCase);
        }

        _navigateCommand.RaiseCanExecuteChanged();
        _goBackCommand.RaiseCanExecuteChanged();
        _logoutCommand.RaiseCanExecuteChanged();
    }
}
