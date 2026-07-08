using System;
using System.Threading.Tasks;

namespace WPF.ViewModels;

public sealed class ReportsViewModel : BaseViewModel
{
    private readonly DashboardViewModel _dashboardViewModel;
    private BaseViewModel _currentViewModel;

    public ReportsViewModel(DashboardViewModel dashboardViewModel)
    {
        _dashboardViewModel = dashboardViewModel ?? throw new ArgumentNullException(nameof(dashboardViewModel));
        _currentViewModel = dashboardViewModel;
    }

    public override string Title => "Reports";

    public override string Description => "Dashboard metrics, occupancy reports, revenue summaries and service analytics";

    public DashboardViewModel DashboardViewModel => _dashboardViewModel;

    public BaseViewModel CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public override async Task InitializeAsync()
    {
        await Task.WhenAll(
            _dashboardViewModel.InitializeAsync()
        );
    }
}
