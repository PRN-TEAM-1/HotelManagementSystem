using System.Windows;
using Microsoft.EntityFrameworkCore;
using DataAccessObjects;
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

        try
        {
            using var context = DbContextFactory.CreateDbContext();
            context.Database.ExecuteSqlRaw(@"
                IF OBJECT_ID('dbo.CK_rooms_status', 'C') IS NOT NULL
                BEGIN
                    ALTER TABLE dbo.rooms DROP CONSTRAINT CK_rooms_status;
                END
                ALTER TABLE dbo.rooms ADD CONSTRAINT CK_rooms_status CHECK (status IN (N'Available', N'Cleaning', N'Maintenance', N'Inactive', N'Reserved', N'Occupied'));

                IF OBJECT_ID(N'dbo.ai_provider_settings', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.ai_provider_settings
                    (
                        ai_provider_setting_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        provider_name NVARCHAR(30) NOT NULL,
                        model_name NVARCHAR(100) NOT NULL,
                        encrypted_api_key NVARCHAR(4000) NOT NULL CONSTRAINT DF_ai_provider_settings_api_key DEFAULT (N''),
                        endpoint_url NVARCHAR(500) NULL,
                        temperature DECIMAL(5,2) NOT NULL CONSTRAINT DF_ai_provider_settings_temperature DEFAULT (0.20),
                        max_output_tokens INT NOT NULL CONSTRAINT DF_ai_provider_settings_max_output_tokens DEFAULT (900),
                        timeout_seconds INT NOT NULL CONSTRAINT DF_ai_provider_settings_timeout_seconds DEFAULT (45),
                        is_active BIT NOT NULL CONSTRAINT DF_ai_provider_settings_is_active DEFAULT (0),
                        last_tested_at DATETIME2 NULL,
                        last_test_status NVARCHAR(500) NULL,
                        created_at DATETIME2 NOT NULL CONSTRAINT DF_ai_provider_settings_created_at DEFAULT SYSUTCDATETIME(),
                        updated_at DATETIME2 NOT NULL CONSTRAINT DF_ai_provider_settings_updated_at DEFAULT SYSUTCDATETIME()
                    );
                END

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ai_provider_settings_provider_name' AND object_id = OBJECT_ID(N'dbo.ai_provider_settings'))
                BEGIN
                    CREATE UNIQUE INDEX UX_ai_provider_settings_provider_name ON dbo.ai_provider_settings(provider_name);
                END

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ai_provider_settings_active' AND object_id = OBJECT_ID(N'dbo.ai_provider_settings'))
                BEGIN
                    CREATE UNIQUE INDEX UX_ai_provider_settings_active ON dbo.ai_provider_settings(is_active) WHERE is_active = 1;
                END

                IF OBJECT_ID(N'dbo.CK_ai_provider_settings_provider', N'C') IS NULL
                BEGIN
                    ALTER TABLE dbo.ai_provider_settings
                        ADD CONSTRAINT CK_ai_provider_settings_provider CHECK (provider_name IN (N'OpenAI', N'Gemini'));
                END

                IF OBJECT_ID(N'dbo.CK_ai_provider_settings_values', N'C') IS NULL
                BEGIN
                    ALTER TABLE dbo.ai_provider_settings
                        ADD CONSTRAINT CK_ai_provider_settings_values CHECK
                        (
                            temperature >= 0
                            AND temperature <= 2
                            AND max_output_tokens BETWEEN 100 AND 4000
                            AND timeout_seconds BETWEEN 5 AND 180
                        );
                END
            ");
        }
        catch
        {
            // Ignore if database is not initialized yet
        }

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
        var aiConfigurationService = new AiConfigurationService();
        var aiSettingsViewModel = new AiSettingsViewModel(
            aiConfigurationService,
            _currentUserService);
        var aiServiceRecommendationService = new AiServiceRecommendationService();
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
        var serviceOrderViewModel = new ServiceOrderViewModel(serviceOrderService, serviceCatalogService, _currentUserService, checkoutService, aiServiceRecommendationService);
        var invoiceViewModel = new InvoiceViewModel(invoiceService, paymentService, _currentUserService, _dialogService);
        var billingViewModel = new BillingViewModel(invoiceViewModel);
        var customerManagementViewModel = new CustomerManagementViewModel(
            new CustomerService(),
            new RoomService(),
            new BookingService(),
            _currentUserService);
        var roomTypeManagementViewModel = new RoomTypeManagementViewModel(new RoomTypeService());
        var roomManagementViewModel = new RoomManagementViewModel(new RoomService(), new RoomTypeService());
        var roomMapViewModel = new RoomMapViewModel(new RoomService());
        var adminSetupViewModel = new AdminSetupViewModel(
            roomTypeManagementViewModel,
            roomManagementViewModel,
            serviceManagementViewModel);

        var operationsViewModel = new OperationsViewModel(
            checkInViewModel,
            checkoutViewModel,
            serviceOrderViewModel,
            billingViewModel,
            customerManagementViewModel,
            roomMapViewModel);


        var dashboardRepository = new Repositories.Implements.DashboardRepository();
        var dashboardService = new DashboardService(dashboardRepository);
        var dashboardViewModel = new DashboardViewModel(dashboardService);
        var occupancyReportRepository = new Repositories.Implements.OccupancyReportRepository();
        var occupancyReportService = new OccupancyReportService(occupancyReportRepository);
        var occupancyReportViewModel = new OccupancyReportViewModel(occupancyReportService);
        var revenueReportViewModel = new RevenueReportViewModel();
        var serviceUsageReportViewModel = new ServiceUsageReportViewModel();
        var reportsViewModel = new ReportsViewModel(
            dashboardViewModel,
            occupancyReportViewModel,
            revenueReportViewModel,
            serviceUsageReportViewModel);
        var styleGuideViewModel = new StyleGuideViewModel(_dialogService);

        _navigationService.Register(NavigationTargets.Workspace, () => workspaceViewModel);
        _navigationService.Register(NavigationTargets.Session, () => sessionViewModel);
        _navigationService.Register(NavigationTargets.Administration, () => administrationViewModel);
        _navigationService.Register(NavigationTargets.AdminSetup, () => adminSetupViewModel);
        _navigationService.Register(NavigationTargets.AiSettings, () => aiSettingsViewModel);
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
            "Workspace",
            "Staff operations overview for the hotel management system.",
            "Overview",
            [
                "Administration keeps staff accounts and access status current.",
                "Admin Setup maintains rooms, room types and the service catalog.",
                "Operations brings together booking, stay service and billing work for reception staff.",
                "Reports show occupancy, revenue and service performance."
            ],
            [
                "Access follows the signed-in staff role.",
                "Billing status updates after successful payments.",
                "Reports use current operational data."
            ],
            ["Accounts", "Admin Setup", "Operations", "Reports"]);
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
