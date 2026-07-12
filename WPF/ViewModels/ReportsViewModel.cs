using System;
using System.Threading.Tasks;

namespace WPF.ViewModels;

public sealed class ReportsViewModel : BaseViewModel
{
    private readonly DashboardViewModel _dashboardViewModel;
    private readonly OccupancyReportViewModel _occupancyReportViewModel;
    private BaseViewModel _currentViewModel;

    public ReportsViewModel(
        DashboardViewModel dashboardViewModel,
        OccupancyReportViewModel occupancyReportViewModel)
    {
        _dashboardViewModel = dashboardViewModel;
        _occupancyReportViewModel = occupancyReportViewModel;
        _currentViewModel = dashboardViewModel;
    }

    public override string Title => "Reports";

    public DashboardViewModel DashboardViewModel => _dashboardViewModel;

    public OccupancyReportViewModel OccupancyReportViewModel => _occupancyReportViewModel;

    public BaseViewModel CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public override async Task InitializeAsync()
    {
        await _dashboardViewModel.InitializeAsync();
    }
}