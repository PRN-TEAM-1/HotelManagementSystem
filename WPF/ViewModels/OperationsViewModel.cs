using System.Collections.ObjectModel;

namespace WPF.ViewModels;

public sealed class OperationsViewModel : BaseViewModel
{
    private readonly CheckInViewModel _checkInViewModel;
    private readonly CheckoutViewModel _checkoutViewModel;
    private readonly ServiceManagementViewModel _serviceManagementViewModel;
    private readonly ServiceOrderViewModel _serviceOrderViewModel;

    private BaseViewModel _currentViewModel;

    public OperationsViewModel(
        CheckInViewModel checkInViewModel,
        CheckoutViewModel checkoutViewModel,
        ServiceManagementViewModel serviceManagementViewModel,
        ServiceOrderViewModel serviceOrderViewModel)
    {
        _checkInViewModel = checkInViewModel ?? throw new ArgumentNullException(nameof(checkInViewModel));
        _checkoutViewModel = checkoutViewModel ?? throw new ArgumentNullException(nameof(checkoutViewModel));
        _serviceManagementViewModel = serviceManagementViewModel ?? throw new ArgumentNullException(nameof(serviceManagementViewModel));
        _serviceOrderViewModel = serviceOrderViewModel ?? throw new ArgumentNullException(nameof(serviceOrderViewModel));

        _currentViewModel = checkInViewModel;
    }

    public override string Title => "Operations";

    public override string Description => "Manage guest operations, check-in, check-out, and services";

    public CheckInViewModel CheckInViewModel => _checkInViewModel;

    public CheckoutViewModel CheckoutViewModel => _checkoutViewModel;

    public ServiceManagementViewModel ServiceManagementViewModel => _serviceManagementViewModel;

    public ServiceOrderViewModel ServiceOrderViewModel => _serviceOrderViewModel;

    public BaseViewModel CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public override async Task InitializeAsync()
    {
        await Task.WhenAll(
            _checkInViewModel.InitializeAsync(),
            _checkoutViewModel.InitializeAsync(),
            _serviceManagementViewModel.InitializeAsync(),
            _serviceOrderViewModel.InitializeAsync()
        );
    }
}
