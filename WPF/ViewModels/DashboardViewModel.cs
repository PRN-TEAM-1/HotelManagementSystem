using System.Windows.Input;
using Services.Interfaces;
using WPF.Commands;

namespace WPF.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly IDashboardService _dashboardService;

    private DashboardSummaryDto _dashboard = new();

    public DashboardSummaryDto Dashboard
    {
        get => _dashboard;
        set => SetProperty(ref _dashboard, value);
    }

    public ICommand RefreshCommand { get; }

    public override string Title => "Dashboard";

    public DashboardViewModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;

        RefreshCommand = new RelayCommand(LoadDashboard);

        LoadDashboard();
    }

    private void LoadDashboard()
    {
        Dashboard = _dashboardService.GetDashboardSummary();
    }
}