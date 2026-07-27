using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using BusinessObjects.DTOs;
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
    private IUserActivityService _userActivityService = null!;
    private RememberedLoginStore _rememberedLoginStore = null!;
    private DialogService _dialogService = null!;
    private NavigationService _navigationService = null!;
    private MainWindow? _shellWindow;
    private LoginWindow? _loginWindow;
    private DispatcherTimer? _sessionValidationTimer;
    private bool _isTransitioningWindows;
    private bool _isCheckingSession;

    protected override async void OnStartup(StartupEventArgs e)
    {
        ConfigureEnglishCulture();
        base.OnStartup(e);

        try
        {
            using var context = DbContextFactory.CreateDbContext();
            context.Database.ExecuteSqlRaw(@"
                IF OBJECT_ID(N'dbo.users', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.user_login_sessions', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.user_login_sessions
                    (
                        login_session_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        user_id INT NOT NULL,
                        login_at_utc DATETIME2 NOT NULL CONSTRAINT DF_user_login_sessions_login_at DEFAULT SYSUTCDATETIME(),
                        logout_at_utc DATETIME2 NULL,
                        last_seen_at_utc DATETIME2 NOT NULL CONSTRAINT DF_user_login_sessions_last_seen DEFAULT SYSUTCDATETIME(),
                        machine_name NVARCHAR(100) NOT NULL CONSTRAINT DF_user_login_sessions_machine DEFAULT (N'Unknown'),
                        windows_user NVARCHAR(100) NOT NULL CONSTRAINT DF_user_login_sessions_windows_user DEFAULT (N'Unknown'),
                        ip_address NVARCHAR(45) NOT NULL CONSTRAINT DF_user_login_sessions_ip DEFAULT (N'Unknown'),
                        os_version NVARCHAR(200) NOT NULL CONSTRAINT DF_user_login_sessions_os DEFAULT (N'Unknown'),
                        app_version NVARCHAR(50) NOT NULL CONSTRAINT DF_user_login_sessions_app DEFAULT (N'Unknown'),
                        device_type NVARCHAR(50) NOT NULL CONSTRAINT DF_user_login_sessions_device DEFAULT (N'Windows Desktop'),
                        status NVARCHAR(20) NOT NULL CONSTRAINT DF_user_login_sessions_status DEFAULT (N'Active')
                    );
                END

                IF OBJECT_ID(N'dbo.users', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.user_activity_logs', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.user_activity_logs
                    (
                        activity_log_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        login_session_id INT NULL,
                        actor_user_id INT NULL,
                        target_user_id INT NULL,
                        attempted_username NVARCHAR(50) NULL,
                        action_type NVARCHAR(50) NOT NULL,
                        entity_name NVARCHAR(100) NOT NULL,
                        entity_id NVARCHAR(100) NULL,
                        description NVARCHAR(1000) NOT NULL,
                        old_values_json NVARCHAR(MAX) NULL,
                        new_values_json NVARCHAR(MAX) NULL,
                        result NVARCHAR(30) NOT NULL CONSTRAINT DF_user_activity_logs_result DEFAULT (N'Success'),
                        error_message NVARCHAR(1000) NULL,
                        occurred_at_utc DATETIME2 NOT NULL CONSTRAINT DF_user_activity_logs_occurred DEFAULT SYSUTCDATETIME(),
                        machine_name NVARCHAR(100) NOT NULL CONSTRAINT DF_user_activity_logs_machine DEFAULT (N'Unknown'),
                        ip_address NVARCHAR(45) NOT NULL CONSTRAINT DF_user_activity_logs_ip DEFAULT (N'Unknown'),
                        device_type NVARCHAR(50) NOT NULL CONSTRAINT DF_user_activity_logs_device DEFAULT (N'Windows Desktop')
                    );
                END

                IF OBJECT_ID(N'dbo.user_login_sessions', N'U') IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_user_login_sessions_users')
                BEGIN
                    ALTER TABLE dbo.user_login_sessions
                        ADD CONSTRAINT FK_user_login_sessions_users FOREIGN KEY (user_id) REFERENCES dbo.users(user_id);
                END

                IF OBJECT_ID(N'dbo.user_activity_logs', N'U') IS NOT NULL
                    AND OBJECT_ID(N'dbo.user_login_sessions', N'U') IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_user_activity_logs_login_sessions')
                BEGIN
                    ALTER TABLE dbo.user_activity_logs
                        ADD CONSTRAINT FK_user_activity_logs_login_sessions
                        FOREIGN KEY (login_session_id) REFERENCES dbo.user_login_sessions(login_session_id);
                END

                IF OBJECT_ID(N'dbo.user_activity_logs', N'U') IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_user_activity_logs_actor_users')
                BEGIN
                    ALTER TABLE dbo.user_activity_logs
                        ADD CONSTRAINT FK_user_activity_logs_actor_users
                        FOREIGN KEY (actor_user_id) REFERENCES dbo.users(user_id);
                END

                IF OBJECT_ID(N'dbo.user_activity_logs', N'U') IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_user_activity_logs_target_users')
                BEGIN
                    ALTER TABLE dbo.user_activity_logs
                        ADD CONSTRAINT FK_user_activity_logs_target_users
                        FOREIGN KEY (target_user_id) REFERENCES dbo.users(user_id);
                END

                IF OBJECT_ID(N'dbo.user_login_sessions', N'U') IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_user_login_sessions_user_time' AND object_id = OBJECT_ID(N'dbo.user_login_sessions'))
                BEGIN
                    CREATE INDEX IX_user_login_sessions_user_time ON dbo.user_login_sessions(user_id, login_at_utc DESC);
                END

                IF OBJECT_ID(N'dbo.user_activity_logs', N'U') IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_user_activity_logs_time' AND object_id = OBJECT_ID(N'dbo.user_activity_logs'))
                BEGIN
                    CREATE INDEX IX_user_activity_logs_time ON dbo.user_activity_logs(occurred_at_utc DESC);
                END

                IF OBJECT_ID(N'dbo.user_activity_logs', N'U') IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_user_activity_logs_actor_time' AND object_id = OBJECT_ID(N'dbo.user_activity_logs'))
                BEGIN
                    CREATE INDEX IX_user_activity_logs_actor_time ON dbo.user_activity_logs(actor_user_id, occurred_at_utc DESC);
                END

                IF OBJECT_ID(N'dbo.user_activity_logs', N'U') IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_user_activity_logs_target_time' AND object_id = OBJECT_ID(N'dbo.user_activity_logs'))
                BEGIN
                    CREATE INDEX IX_user_activity_logs_target_time ON dbo.user_activity_logs(target_user_id, occurred_at_utc DESC);
                END

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
        _userActivityService = new UserActivityService();
        _authService = new AuthService(userActivityService: _userActivityService);
        _rememberedLoginStore = new RememberedLoginStore();
        _dialogService = new DialogService();
        _navigationService = new NavigationService();
        ConfigureSessionValidationTimer();
        _currentUserService.SessionChanged += OnSessionChanged;

        BuildShellWindow();

        if (!await TryRestoreRememberedSessionAsync())
        {
            ShowLoginWindow();
        }
    }

    private static void ConfigureEnglishCulture()
    {
        var culture = CultureInfo.GetCultureInfo("en-CA");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
    }

    private void BuildShellWindow()
    {
        var workspaceViewModel = new WorkspaceViewModel(
            _currentUserService,
            _userActivityService,
            new RevenueReportService(new Repositories.Implements.RevenueReportRepository()));
        var sessionViewModel = new SessionViewModel(_currentUserService, _userActivityService);
        var userManagementService = new UserManagementService(userActivityService: _userActivityService);
        var administrationViewModel = new UserManagementViewModel(
            userManagementService,
            _currentUserService,
            _userActivityService,
            _dialogService);
        var aiConfigurationService = new AiConfigurationService(userActivityService: _userActivityService);
        var aiSettingsViewModel = new AiSettingsViewModel(
            aiConfigurationService,
            _currentUserService);
        var aiServiceRecommendationService = new AiServiceRecommendationService();
        // Register Member 3 ViewModels and Services
        var checkInService = new CheckInService(userActivityService: _userActivityService);
        var checkoutService = new CheckoutService(userActivityService: _userActivityService);
        var serviceCatalogService = new ServiceCatalogService(userActivityService: _userActivityService);
        var serviceOrderService = new ServiceOrderService(userActivityService: _userActivityService);
        var invoiceService = new InvoiceService(userActivityService: _userActivityService);
        var paymentService = new PaymentService(userActivityService: _userActivityService);

        var checkInViewModel = new CheckInViewModel(checkInService, _currentUserService);
        var checkoutViewModel = new CheckoutViewModel(checkoutService, _currentUserService);
        var serviceManagementViewModel = new ServiceManagementViewModel(serviceCatalogService, _currentUserService);
        var serviceOrderViewModel = new ServiceOrderViewModel(serviceOrderService, serviceCatalogService, _currentUserService, checkoutService, aiServiceRecommendationService);
        var invoiceViewModel = new InvoiceViewModel(invoiceService, paymentService, _currentUserService, _dialogService);
        var billingViewModel = new BillingViewModel(invoiceViewModel);
        var customerManagementViewModel = new CustomerManagementViewModel(
            new CustomerService(userActivityService: _userActivityService),
            new RoomService(),
            new BookingService(userActivityService: _userActivityService),
            _currentUserService);
        var roomTypeManagementViewModel = new RoomTypeManagementViewModel(
            new RoomTypeService(userActivityService: _userActivityService),
            _currentUserService);
        var roomManagementViewModel = new RoomManagementViewModel(
            new RoomService(userActivityService: _userActivityService),
            new RoomTypeService(userActivityService: _userActivityService),
            _currentUserService);
        var roomMapViewModel = new RoomMapViewModel(
            new RoomService(userActivityService: _userActivityService),
            _currentUserService);
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
            _userActivityService,
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

        var loginViewModel = new LoginViewModel(
            _authService,
            _currentUserService,
            _rememberedLoginStore);
        _loginWindow = new LoginWindow
        {
            DataContext = loginViewModel
        };

        _loginWindow.Closed += OnLoginWindowClosed;
        MainWindow = _loginWindow;
        _loginWindow.Show();
    }

    private async Task<bool> TryRestoreRememberedSessionAsync()
    {
        if (!_rememberedLoginStore.TryLoad(out var rememberedLogin) || rememberedLogin is null)
        {
            return false;
        }

        var result = await _authService.RestoreRememberedSessionAsync(
            new RememberedLoginRequestDto
            {
                UserId = rememberedLogin.UserId,
                Username = rememberedLogin.Username,
                UserUpdatedAtTicks = rememberedLogin.UserUpdatedAtTicks,
                ExpiresAtUtc = rememberedLogin.ExpiresAtUtc,
                ClientEnvironment = ClientEnvironmentProvider.Capture()
            });

        if (result.IsSuccess && result.Data?.CurrentSession is not null)
        {
            _currentUserService.Set(result.Data.CurrentSession);
            return true;
        }

        _rememberedLoginStore.Clear();
        return false;
    }

    private void ConfigureSessionValidationTimer()
    {
        _sessionValidationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };

        _sessionValidationTimer.Tick += OnSessionValidationTimerTick;
    }

    private async void OnSessionValidationTimerTick(object? sender, EventArgs e)
    {
        await ValidateCurrentSessionAsync();
    }

    private async Task ValidateCurrentSessionAsync()
    {
        if (_isCheckingSession || _currentUserService.User?.IsAuthenticated != true)
        {
            return;
        }

        _isCheckingSession = true;

        try
        {
            var currentUser = _currentUserService.User;
            var validationResult = await _authService.ValidateSessionAsync(currentUser);

            if (validationResult.IsSuccess && validationResult.Data == true)
            {
                return;
            }

            _rememberedLoginStore.Clear();
            await _userActivityService.EndLoginSessionAsync(currentUser);
            _currentUserService.Clear();
        }
        finally
        {
            _isCheckingSession = false;
        }
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        if (_currentUserService.IsAuthenticated)
        {
            _sessionValidationTimer?.Start();
            ShowShellWindow();
            return;
        }

        _sessionValidationTimer?.Stop();
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
            _rememberedLoginStore.Clear();
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
                "Account Management keeps staff accounts and access status current.",
                "Admin Setup maintains rooms, room types and the service catalog.",
                "Operations brings together booking, stay service and billing work for reception staff.",
                "Reports show occupancy, revenue and service performance."
            ],
            [
                "Access follows the signed-in staff role.",
                "Billing status updates after successful payments.",
                "Reports use current operational data."
            ],
            ["Account Management", "Admin Setup", "Operations", "Reports"]);
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
