using System.Windows;
using Services.Implements;
using Services.Interfaces;
using WPF.Helpers;
using WPF.ViewModels;
using WPF.Views;

namespace WPF;

public partial class App : Application
{
    private ICurrentUserService _currentUserService = null!;
    private IAuthService _authService = null!;
    private DialogService _dialogService = null!;
    private NavigationService _navigationService = null!;
    private MainWindow? _shellWindow;
    private LoginWindow? _loginWindow;
    private bool _isTransitioningWindows;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _currentUserService = new CurrentUserService();
        _authService = new AuthService();
        _dialogService = new DialogService();
        _navigationService = new NavigationService();
        _currentUserService.SessionChanged += OnSessionChanged;

        BuildShellWindow();
        ShowLoginWindow();
    }

    private void BuildShellWindow()
    {
        var workspaceViewModel = CreateWorkspaceViewModel();
        var sessionViewModel = new SessionViewModel(_currentUserService);
        var userManagementService = new UserManagementService();
        var administrationViewModel = new UserManagementViewModel(
            userManagementService,
            _currentUserService,
            _dialogService);
        var roomTypeManagementViewModel = new RoomTypeManagementViewModel(new RoomTypeService());

        // Register Member 3 ViewModels and Services
        var checkInService = new CheckInService();
        var checkoutService = new CheckoutService();
        var serviceCatalogService = new ServiceCatalogService();
        var serviceOrderService = new ServiceOrderService();
        var invoiceService = new InvoiceService();
        var paymentService = new PaymentService();

        var checkInViewModel = new CheckInViewModel(checkInService, _currentUserService);
        var checkoutViewModel = new CheckoutViewModel(checkoutService, _currentUserService);
        var serviceManagementViewModel = new ServiceManagementViewModel(serviceCatalogService);
        var serviceOrderViewModel = new ServiceOrderViewModel(serviceOrderService, serviceCatalogService, _currentUserService);
        var invoiceViewModel = new InvoiceViewModel(invoiceService, paymentService, _currentUserService, _dialogService);
        var customerManagementViewModel = new CustomerManagementViewModel(
            new CustomerService(),
            new RoomService(),
            new BookingService(),
            _currentUserService);
        var roomTypeManagementViewModel = new RoomTypeManagementViewModel(new RoomTypeService());
        var roomManagementViewModel = new RoomManagementViewModel(new RoomService(), new RoomTypeService());

        var operationsViewModel = new OperationsViewModel(
            checkInViewModel,
            checkoutViewModel,
            serviceManagementViewModel,
            serviceOrderViewModel,
            invoiceViewModel,
            customerManagementViewModel,
            roomTypeManagementViewModel,
            roomManagementViewModel);

        var dashboardRepository = new Repositories.Implements.DashboardRepository();
        var dashboardService = new DashboardService(dashboardRepository);
        var dashboardViewModel = new DashboardViewModel(dashboardService);
        var reportsViewModel = new ReportsViewModel(dashboardViewModel);
        var styleGuideViewModel = new StyleGuideViewModel(_dialogService);

        _navigationService.Register(NavigationTargets.Workspace, () => workspaceViewModel);
        _navigationService.Register(NavigationTargets.Session, () => sessionViewModel);
        _navigationService.Register(NavigationTargets.Administration, () => administrationViewModel);
        _navigationService.Register(NavigationTargets.Operations, () => operationsViewModel);
        _navigationService.Register(NavigationTargets.Reports, () => reportsViewModel);
        _navigationService.Register(NavigationTargets.StyleGuide, () => styleGuideViewModel);

        _navigationService.Navigate(NavigationTargets.Workspace, addToHistory: false);

        var mainWindowViewModel = new MainWindowViewModel(
            _navigationService,
            _currentUserService,
            _dialogService);

        _shellWindow = new MainWindow
        {
            DataContext = mainWindowViewModel
        };

        _shellWindow.Closed += OnShellWindowClosed;
    }

    private void ShowLoginWindow()
    {
        if (_loginWindow is not null)
        {
            if (!_loginWindow.IsVisible)
            {
                _loginWindow.Show();
            }

            _loginWindow.Activate();
            MainWindow = _loginWindow;
            return;
        }

        var loginViewModel = new LoginViewModel(_authService, _currentUserService);
        _loginWindow = new LoginWindow
        {
            DataContext = loginViewModel
        };

        _loginWindow.Closed += OnLoginWindowClosed;
        MainWindow = _loginWindow;
        _loginWindow.Show();
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        if (_currentUserService.IsAuthenticated)
        {
            ShowShellWindow();
            return;
        }

        ShowLoginAfterLogout();
    }

    private void ShowShellWindow()
    {
        _isTransitioningWindows = true;

        try
        {
            if (_loginWindow is not null)
            {
                _loginWindow.Closed -= OnLoginWindowClosed;
                _loginWindow.Close();
                _loginWindow = null;
            }

            _navigationService.Navigate(NavigationTargets.Workspace, addToHistory: false);

            if (_shellWindow is not null && !_shellWindow.IsVisible)
            {
                _shellWindow.Show();
            }

            MainWindow = _shellWindow;
            _shellWindow?.Activate();
        }
        finally
        {
            _isTransitioningWindows = false;
        }
    }

    private void ShowLoginAfterLogout()
    {
        _isTransitioningWindows = true;

        try
        {
            _shellWindow?.Hide();
            ShowLoginWindow();
        }
        finally
        {
            _isTransitioningWindows = false;
        }
    }

    private void OnLoginWindowClosed(object? sender, EventArgs e)
    {
        if (_isTransitioningWindows || _currentUserService.IsAuthenticated)
        {
            return;
        }

        Shutdown();
    }

    private void OnShellWindowClosed(object? sender, EventArgs e)
    {
        if (_isTransitioningWindows)
        {
            return;
        }

        Shutdown();
    }

    private static SectionViewModel CreateWorkspaceViewModel()
    {
        return new SectionViewModel(
            "Core Workspace",
            "Authenticated entry point for every upcoming vertical slice after the new login flow is completed.",
            "CORE-004 + M1-AUTH-001",
            [
                "`WPF View -> ViewModel -> Service -> Repository -> DAO` remains the only allowed call chain.",
                "`DbContextFactory` centralizes `appsettings.json` discovery, so UI code stays configuration-only.",
                "Auth now reaches SQL Server through EF Core and populates `CurrentUserService` with the logged-in staff account."
            ],
            [
                "Add a view model, add a view, then register that pair once in `App.xaml.cs` to join the shell.",
                "Keep repositories and DAOs out of `WPF`; feature view models should only depend on service interfaces.",
                "Reuse the shared button, input and table styles so forms from different members still feel like one product."
            ],
            ["MVVM", "Shell", "Navigation", "Auth", "Styles"]);
    }

    private static SectionViewModel CreateOperationsViewModel()
    {
        return new SectionViewModel(
            "Operations",
            "Target area for booking, check-in, check-out, service order, invoice and payment flows used by reception staff.",
            "Reception Flow",
            [
                "This section matches the end-to-end business flow documented in `README.md` and `Database-Ver2.0.md`.",
                "Navigation supports role-gated access for Receptionist and Admin accounts after real login.",
                "The shared shell is ready for task slices like booking creation, room map and billing screens."
            ],
            [
                "Keep long-running actions inside services and return `ServiceResult<T>` back to the UI.",
                "Use dialogs for confirm/cancel flows, and bind validation messages rather than throwing from the view.",
                "Replace these cards with actual feature views as soon as the backend slices are ready."
            ],
            ["Booking", "Check-in", "Check-out", "Billing"]);
    }

    private static SectionViewModel CreateReportsViewModel()
    {
        return new SectionViewModel(
            "Reports",
            "Home for dashboard metrics, occupancy reports, revenue summaries and service analytics aimed at Admin and Manager roles.",
            "Insights",
            [
                "Role-based navigation exposes this zone only to Manager/Admin sessions.",
                "The shared `DataGrid` styling is ready for KPI tables and export preview screens.",
                "Future charts or summary cards can live inside the same shell without changing the navigation infrastructure."
            ],
            [
                "Report view models should stay read-only and call dedicated reporting services.",
                "Export actions can attach to the same dialog patterns for confirmation and completion messages.",
                "Dashboard cards can reuse the same color system to keep reports visually aligned with operations screens."
            ],
            ["Dashboard", "Occupancy", "Revenue", "Service Usage"]);
    }
}
