using BusinessObjects.DTOs;
using BusinessObjects.DTOs.Reports;
using BusinessObjects.Enums;
using Services.Interfaces;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class WorkspaceViewModel : BaseViewModel
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserActivityService _userActivityService;
    private readonly IRevenueReportService _revenueReportService;

    private ActivityDashboardDto _dashboard = new();
    private IReadOnlyList<ManagerPeriodSummary> _managerSummaries = [];
    private string _message = string.Empty;
    private bool _isBusy;

    public WorkspaceViewModel(
        ICurrentUserService currentUserService,
        IUserActivityService userActivityService,
        IRevenueReportService revenueReportService)
    {
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _userActivityService = userActivityService ?? throw new ArgumentNullException(nameof(userActivityService));
        _revenueReportService = revenueReportService ?? throw new ArgumentNullException(nameof(revenueReportService));
        _currentUserService.SessionChanged += OnSessionChanged;

        RefreshCommand = new AsyncRelayCommand(LoadWorkspaceAsync, () => !IsBusy && (IsAdmin || IsManager));
    }

    public override string Title => "Workspace";

    public override string Description => IsAdmin
        ? "System activity dashboard and recent audit events"
        : IsManager
            ? "Revenue and service performance at a glance"
            : "Role-based hotel workspace";

    public ActivityDashboardDto Dashboard
    {
        get => _dashboard;
        private set => SetProperty(ref _dashboard, value);
    }

    public bool IsAdmin => _currentUserService.HasRole(RoleName.Admin);
    public bool IsManager => _currentUserService.HasRole(RoleName.Manager);
    public bool IsRegularWorkspace => !IsAdmin && !IsManager;

    public IReadOnlyList<ManagerPeriodSummary> ManagerSummaries
    {
        get => _managerSummaries;
        private set => SetProperty(ref _managerSummaries, value);
    }

    public decimal MaxManagerRevenue => Math.Max(1, ManagerSummaries.Select(x => x.Revenue).DefaultIfEmpty().Max());
    public int MaxManagerServiceQuantity => Math.Max(1, ManagerSummaries.Select(x => x.ServiceQuantity).DefaultIfEmpty().Max());

    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public AsyncRelayCommand RefreshCommand { get; }

    public override void OnNavigatedTo()
    {
        if (IsAdmin || IsManager)
        {
            _ = LoadWorkspaceAsync();
        }
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        OnPropertiesChanged(nameof(IsAdmin), nameof(IsManager), nameof(IsRegularWorkspace), nameof(Description));
        RefreshCommand.RaiseCanExecuteChanged();

        if (IsAdmin || IsManager)
        {
            _ = LoadWorkspaceAsync();
        }
        else
        {
            Dashboard = new ActivityDashboardDto();
            ManagerSummaries = [];
        }
    }

    private async Task LoadWorkspaceAsync()
    {
        if (!IsAdmin && !IsManager)
        {
            return;
        }

        IsBusy = true;
        Message = string.Empty;

        try
        {
            if (IsAdmin)
            {
                var result = await _userActivityService.GetActivityDashboardAsync(_currentUserService.User);
                if (result.IsSuccess)
                {
                    Dashboard = result.Data ?? new ActivityDashboardDto();
                }
                else
                {
                    Message = result.Errors.FirstOrDefault() ?? result.Message;
                }
            }
            else
            {
                LoadManagerSummaries();
            }
        }
        catch (Exception ex)
        {
            Message = $"Unable to load workspace dashboard: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LoadManagerSummaries()
    {
        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        ManagerSummaries =
        [
            CreateManagerSummary("Today", today, today),
            CreateManagerSummary("This Week", startOfWeek, today),
            CreateManagerSummary("This Month", startOfMonth, today)
        ];

        OnPropertiesChanged(nameof(MaxManagerRevenue), nameof(MaxManagerServiceQuantity));
    }

    private ManagerPeriodSummary CreateManagerSummary(string label, DateTime startDate, DateTime endDate)
    {
        var filter = new ReportFilterDto { StartDate = startDate, EndDate = endDate };
        var revenue = _revenueReportService.GetRevenueReport(filter);
        var services = _revenueReportService.GetRevenueByService(filter);

        return new ManagerPeriodSummary
        {
            Label = label,
            DateRange = startDate == endDate
                ? startDate.ToString("yyyy-MM-dd")
                : $"{startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd}",
            Revenue = revenue.Sum(x => x.TotalRevenue),
            PaymentCount = revenue.Sum(x => x.PaymentCount),
            ServiceQuantity = services.Sum(x => x.QuantityOrdered),
            ServiceRevenue = services.Sum(x => x.TotalRevenue)
        };
    }
}

public sealed class ManagerPeriodSummary
{
    public string Label { get; init; } = string.Empty;
    public string DateRange { get; init; } = string.Empty;
    public decimal Revenue { get; init; }
    public int PaymentCount { get; init; }
    public int ServiceQuantity { get; init; }
    public decimal ServiceRevenue { get; init; }
}
