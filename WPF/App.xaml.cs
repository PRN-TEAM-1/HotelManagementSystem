using System.Windows;
using WPF.Helpers;
using WPF.ViewModels;
using WPF.Views;

namespace WPF;

public partial class App : Application
{
    private CurrentSession _currentSession = null!;
    private DialogService _dialogService = null!;
    private NavigationService _navigationService = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _currentSession = new CurrentSession();
        _dialogService = new DialogService();
        _navigationService = new NavigationService();

        var workspaceViewModel = CreateWorkspaceViewModel();
        var sessionViewModel = new SessionViewModel(_currentSession, _dialogService);
        var administrationViewModel = CreateAdministrationViewModel();
        var operationsViewModel = CreateOperationsViewModel();
        var reportsViewModel = CreateReportsViewModel();
        var styleGuideViewModel = new StyleGuideViewModel(_dialogService);

        _navigationService.Register(NavigationTargets.Workspace, () => workspaceViewModel);
        _navigationService.Register(NavigationTargets.SessionSandbox, () => sessionViewModel);
        _navigationService.Register(NavigationTargets.Administration, () => administrationViewModel);
        _navigationService.Register(NavigationTargets.Operations, () => operationsViewModel);
        _navigationService.Register(NavigationTargets.Reports, () => reportsViewModel);
        _navigationService.Register(NavigationTargets.StyleGuide, () => styleGuideViewModel);

        var mainWindowViewModel = new MainWindowViewModel(
            _navigationService,
            _currentSession,
            _dialogService);

        _navigationService.Navigate(NavigationTargets.Workspace, addToHistory: false);

        MainWindow = new MainWindow
        {
            DataContext = mainWindowViewModel
        };

        MainWindow.Show();
    }

    private static SectionViewModel CreateWorkspaceViewModel()
    {
        return new SectionViewModel(
            "Core Workspace",
            "Shared MVVM shell for the Hotel Management System. This is the hand-off point for every feature team before wiring real service-backed screens.",
            "CORE-004",
            [
                "`WPF View -> ViewModel -> Service -> Repository -> DAO` remains the only allowed call chain.",
                "`DbContextFactory` already centralizes `appsettings.json` discovery, so UI code stays configuration-only.",
                "`CurrentSession`, `NavigationService` and `RelayCommand` are ready for the upcoming auth and role-based menu flow."
            ],
            [
                "Add a view model, add a view, then register that pair once in `App.xaml.cs` to join the shell.",
                "Keep repositories and DAOs out of `WPF`; feature view models should only depend on service interfaces.",
                "Reuse the shared button, input and table styles so forms from different members still feel like one product."
            ],
            ["MVVM", "Shell", "Navigation", "Session", "Styles"]);
    }

    private static SectionViewModel CreateAdministrationViewModel()
    {
        return new SectionViewModel(
            "Administration",
            "Reserved for admin-focused slices such as user management, room catalog, room types and service catalog maintenance.",
            "Admin Only",
            [
                "User management will plug into this area after `M1-USERMGMT-001`.",
                "Room type, room and service catalog pages can share the same form and table styles from `CORE-004`.",
                "Role-aware visibility is already prepared so this area stays hidden outside Admin sessions."
            ],
            [
                "Feature teams can replace the placeholder with a real dashboard or nested navigation later.",
                "Any command in this area should continue to delegate business logic to `Services` only.",
                "Dialog confirmations here can reuse `DialogService` for lock/unlock, status change and delete flows."
            ],
            ["Users", "Rooms", "Room Types", "Services"]);
    }

    private static SectionViewModel CreateOperationsViewModel()
    {
        return new SectionViewModel(
            "Operations",
            "Target area for booking, check-in, check-out, service order, invoice and payment flows used by reception staff.",
            "Reception Flow",
            [
                "This section matches the end-to-end business flow documented in `README.md` and `Database-Ver2.0.md`.",
                "Navigation already supports role-gated access for Receptionist and Admin demo sessions.",
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
                "Role-based navigation already exposes this zone only to Manager/Admin sessions.",
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
