using System.Windows.Input;
using BusinessObjects.Constants;
using Services.Interfaces;
using WPF.Commands;

namespace WPF.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly IDashboardService _dashboardService;

    private DashboardSummaryDto _dashboard = new();
    private string _message = string.Empty;

    public DashboardSummaryDto Dashboard
    {
        get => _dashboard;
        set => SetProperty(ref _dashboard, value);
    }

    public ICommand RefreshCommand { get; }

    public override string Title => "Dashboard";

    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    public DashboardViewModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;

        RefreshCommand = new RelayCommand(LoadDashboard);
    }

    public override Task InitializeAsync()
    {
        LoadDashboard();
        return Task.CompletedTask;
    }

    private void LoadDashboard()
    {
        Message = string.Empty;

        try
        {
            Dashboard = _dashboardService.GetDashboardSummary();
        }
        catch
        {
            Dashboard = new DashboardSummaryDto();
            Message = ErrorMessages.DatabaseConnectionRequired;
        }
    }
}
