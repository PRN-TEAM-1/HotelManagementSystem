using System.Collections.ObjectModel;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class ReportsViewModel : BaseViewModel
{
    private readonly DashboardViewModel _dashboardViewModel;
    private readonly OccupancyReportViewModel _occupancyReportViewModel;
    private readonly RevenueReportViewModel _revenueReportViewModel;
    private readonly ServiceUsageReportViewModel _serviceUsageReportViewModel;
    private readonly HashSet<string> _initializedModuleKeys = new(StringComparer.OrdinalIgnoreCase);

    private BaseViewModel _currentViewModel;
    private ReportModuleViewModel? _selectedModule;
    private string _reportMessage = string.Empty;
    private bool _isBusy;

    public ReportsViewModel(
        DashboardViewModel dashboardViewModel,
        OccupancyReportViewModel occupancyReportViewModel,
        RevenueReportViewModel revenueReportViewModel,
        ServiceUsageReportViewModel serviceUsageReportViewModel)
    {
        _dashboardViewModel = dashboardViewModel ?? throw new ArgumentNullException(nameof(dashboardViewModel));
        _occupancyReportViewModel = occupancyReportViewModel ?? throw new ArgumentNullException(nameof(occupancyReportViewModel));
        _revenueReportViewModel = revenueReportViewModel ?? throw new ArgumentNullException(nameof(revenueReportViewModel));
        _serviceUsageReportViewModel = serviceUsageReportViewModel ?? throw new ArgumentNullException(nameof(serviceUsageReportViewModel));

        _currentViewModel = dashboardViewModel;
        Modules = new ObservableCollection<ReportModuleViewModel>(CreateModules());
        SelectModuleCommand = new RelayCommand<ReportModuleViewModel>(SelectModule);

        if (Modules.FirstOrDefault() is { } firstModule)
        {
            SetSelectedModule(firstModule);
        }
    }

    public override string Title => "Reports";

    public override string Description => "Dashboard, occupancy, revenue and service analytics";

    public ObservableCollection<ReportModuleViewModel> Modules { get; }

    public RelayCommand<ReportModuleViewModel> SelectModuleCommand { get; }

    public DashboardViewModel DashboardViewModel => _dashboardViewModel;

    public OccupancyReportViewModel OccupancyReportViewModel => _occupancyReportViewModel;

    public RevenueReportViewModel RevenueReportViewModel => _revenueReportViewModel;

    public ServiceUsageReportViewModel ServiceUsageReportViewModel => _serviceUsageReportViewModel;

    public BaseViewModel CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    public ReportModuleViewModel? SelectedModule
    {
        get => _selectedModule;
        private set
        {
            if (SetProperty(ref _selectedModule, value))
            {
                OnPropertiesChanged(nameof(CurrentModuleTitle), nameof(CurrentModuleDescription));
            }
        }
    }

    public string CurrentModuleTitle => SelectedModule?.Title ?? Title;

    public string CurrentModuleDescription => SelectedModule?.Description ?? Description;

    public string ReportMessage
    {
        get => _reportMessage;
        private set => SetProperty(ref _reportMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public override async Task InitializeAsync()
    {
        await InitializeSelectedModuleAsync();
    }

    public override void OnNavigatedTo()
    {
        _ = InitializeSelectedModuleAsync();
    }

    private IEnumerable<ReportModuleViewModel> CreateModules()
    {
        return
        [
            new ReportModuleViewModel(
                "dashboard",
                "Dashboard",
                "Live operational summary.",
                "ViewDashboard",
                _dashboardViewModel),
            new ReportModuleViewModel(
                "occupancy",
                "Occupancy",
                "Room-night and occupancy rate report.",
                "OfficeBuilding",
                _occupancyReportViewModel),
            new ReportModuleViewModel(
                "revenue",
                "Revenue",
                "Payment and revenue performance.",
                "CashRegister",
                _revenueReportViewModel),
            new ReportModuleViewModel(
                "service-usage",
                "Service Usage",
                "Ordered services and service revenue.",
                "ReceiptText",
                _serviceUsageReportViewModel)
        ];
    }

    private async void SelectModule(ReportModuleViewModel? module)
    {
        if (module is null)
        {
            return;
        }

        SetSelectedModule(module);
        await InitializeSelectedModuleAsync();
    }

    private void SetSelectedModule(ReportModuleViewModel module)
    {
        foreach (var item in Modules)
        {
            item.IsSelected = ReferenceEquals(item, module);
        }

        SelectedModule = module;
        CurrentViewModel = module.ViewModel;
        ReportMessage = string.Empty;
    }

    private async Task InitializeSelectedModuleAsync()
    {
        if (SelectedModule is null || _initializedModuleKeys.Contains(SelectedModule.Key))
        {
            return;
        }

        IsBusy = true;
        ReportMessage = string.Empty;

        try
        {
            await SelectedModule.ViewModel.InitializeAsync();
            _initializedModuleKeys.Add(SelectedModule.Key);
        }
        catch (Exception ex)
        {
            ReportMessage = $"Unable to load {SelectedModule.Title}: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
