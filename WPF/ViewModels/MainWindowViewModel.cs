using System.Collections.ObjectModel;
using BusinessObjects.Enums;
using WPF.Commands;
using WPF.Helpers;

namespace WPF.ViewModels;

public sealed class MainWindowViewModel : BaseViewModel
{
    private readonly NavigationService _navigationService;
    private readonly CurrentSession _currentSession;
    private readonly DialogService _dialogService;
    private readonly RelayCommand _goBackCommand;
    private readonly RelayCommand _clearSessionCommand;
    private readonly RelayCommand<NavigationItemViewModel> _navigateCommand;

    private BaseViewModel? _currentViewModel;
    private string _currentPageTitle = "Hotel Management System";
    private string _currentPageDescription = "Shared WPF shell";
    private string _sessionDisplayName = "No active session";
    private string _sessionRoleDisplay = "Public";
    private string _authenticationState = "Signed out";

    public MainWindowViewModel(
        NavigationService navigationService,
        CurrentSession currentSession,
        DialogService dialogService)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _currentSession = currentSession ?? throw new ArgumentNullException(nameof(currentSession));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        NavigationItems = new ObservableCollection<NavigationItemViewModel>(CreateNavigationItems());

        _navigateCommand = new RelayCommand<NavigationItemViewModel>(NavigateTo, item => item?.IsVisible == true);
        _goBackCommand = new RelayCommand(GoBack, () => _navigationService.CanGoBack);
        _clearSessionCommand = new RelayCommand(ClearSession, () => _currentSession.IsAuthenticated);

        NavigateCommand = _navigateCommand;
        GoBackCommand = _goBackCommand;
        ClearSessionCommand = _clearSessionCommand;

        _navigationService.NavigationChanged += OnNavigationChanged;
        _currentSession.SessionChanged += OnSessionChanged;

        RefreshNavigationVisibility();
        SyncShellState();
    }

    public string AppTitle => "Hotel Management System";

    public string AppSubtitle => "Shared MVVM foundation for the week 1 fullstack slices";

    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; }

    public RelayCommand<NavigationItemViewModel> NavigateCommand { get; }

    public RelayCommand GoBackCommand { get; }

    public RelayCommand ClearSessionCommand { get; }

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
                NavigationTargets.SessionSandbox,
                "Session Sandbox",
                "Validate CurrentSession and role-based visibility"),
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

    private void ClearSession()
    {
        if (!_currentSession.IsAuthenticated)
        {
            return;
        }

        if (_dialogService.Confirm("Clear the current demo session from the shell header?", "Clear Session"))
        {
            _currentSession.Clear();
        }
    }

    private void RefreshNavigationVisibility()
    {
        var roleName = _currentSession.User?.RoleName;

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
        SessionDisplayName = _currentSession.DisplayName;
        SessionRoleDisplay = _currentSession.RoleDisplay;
        AuthenticationState = _currentSession.IsAuthenticated ? "Demo session active" : "Foundation-only access";

        foreach (var item in NavigationItems)
        {
            item.IsSelected = string.Equals(item.Key, _navigationService.CurrentKey, StringComparison.OrdinalIgnoreCase);
        }

        _navigateCommand.RaiseCanExecuteChanged();
        _goBackCommand.RaiseCanExecuteChanged();
        _clearSessionCommand.RaiseCanExecuteChanged();
    }
}
