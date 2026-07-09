using System.Collections.ObjectModel;

namespace WPF.ViewModels;

public sealed class OperationsViewModel : BaseViewModel
{
    private readonly CheckInViewModel _checkInViewModel;
    private readonly CheckoutViewModel _checkoutViewModel;
    private readonly ServiceManagementViewModel _serviceManagementViewModel;
    private readonly ServiceOrderViewModel _serviceOrderViewModel;
    private readonly BillingViewModel _billingViewModel;
    private readonly CustomerManagementViewModel _customerManagementViewModel;
    private readonly RoomTypeManagementViewModel _roomTypeManagementViewModel;
    private readonly RoomManagementViewModel _roomManagementViewModel;

    private BaseViewModel _currentViewModel;

    public OperationsViewModel(
        CheckInViewModel checkInViewModel,
        CheckoutViewModel checkoutViewModel,
        ServiceManagementViewModel serviceManagementViewModel,
        ServiceOrderViewModel serviceOrderViewModel,
        BillingViewModel billingViewModel,
        CustomerManagementViewModel customerManagementViewModel,
        RoomTypeManagementViewModel roomTypeManagementViewModel,
        RoomManagementViewModel roomManagementViewModel)
    {
        _checkInViewModel = checkInViewModel ?? throw new ArgumentNullException(nameof(checkInViewModel));
        _checkoutViewModel = checkoutViewModel ?? throw new ArgumentNullException(nameof(checkoutViewModel));
        _serviceManagementViewModel = serviceManagementViewModel ?? throw new ArgumentNullException(nameof(serviceManagementViewModel));
        _serviceOrderViewModel = serviceOrderViewModel ?? throw new ArgumentNullException(nameof(serviceOrderViewModel));
        _billingViewModel = billingViewModel ?? throw new ArgumentNullException(nameof(billingViewModel));
        _customerManagementViewModel = customerManagementViewModel ?? throw new ArgumentNullException(nameof(customerManagementViewModel));
        _roomTypeManagementViewModel = roomTypeManagementViewModel ?? throw new ArgumentNullException(nameof(roomTypeManagementViewModel));
        _roomManagementViewModel = roomManagementViewModel ?? throw new ArgumentNullException(nameof(roomManagementViewModel));

        _currentViewModel = checkInViewModel;
    }

    public override string Title => "Operations";

    public override string Description => "Manage guest operations, check-in, check-out, and services";

    public CheckInViewModel CheckInViewModel => _checkInViewModel;

    public CheckoutViewModel CheckoutViewModel => _checkoutViewModel;

    public ServiceManagementViewModel ServiceManagementViewModel => _serviceManagementViewModel;

    public ServiceOrderViewModel ServiceOrderViewModel => _serviceOrderViewModel;

    public BillingViewModel BillingViewModel => _billingViewModel;

    public CustomerManagementViewModel CustomerManagementViewModel => _customerManagementViewModel;

    public RoomTypeManagementViewModel RoomTypeManagementViewModel => _roomTypeManagementViewModel;

    public RoomManagementViewModel RoomManagementViewModel => _roomManagementViewModel;

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
            _serviceOrderViewModel.InitializeAsync(),
            _billingViewModel.InitializeAsync(),
            _customerManagementViewModel.InitializeAsync(),
            _roomTypeManagementViewModel.InitializeAsync(),
            _roomManagementViewModel.InitializeAsync()
        );
    }
}
