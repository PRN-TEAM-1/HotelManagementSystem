using System.Collections.ObjectModel;
using WPF.Commands;

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
    private OperationModuleViewModel? _selectedModule;
    private string _operationMessage = string.Empty;
    private bool _isBusy;
    private readonly HashSet<string> _initializedModuleKeys = new(StringComparer.OrdinalIgnoreCase);

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
        Modules = new ObservableCollection<OperationModuleViewModel>(CreateModules());
        SelectModuleCommand = new RelayCommand<OperationModuleViewModel>(SelectModule);

        if (Modules.FirstOrDefault() is { } firstModule)
        {
            SetSelectedModule(firstModule);
        }
    }

    public override string Title => "Operations";

    public override string Description => "Guest operations, rooms, services and billing";

    public ObservableCollection<OperationModuleViewModel> Modules { get; }

    public RelayCommand<OperationModuleViewModel> SelectModuleCommand { get; }

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

    public OperationModuleViewModel? SelectedModule
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

    public string OperationMessage
    {
        get => _operationMessage;
        private set => SetProperty(ref _operationMessage, value);
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

    private IEnumerable<OperationModuleViewModel> CreateModules()
    {
        return
        [
            new OperationModuleViewModel(
                "check-in",
                "Check-In",
                "Confirm reserved rooms and start a stay.",
                "Login",
                _checkInViewModel),
            new OperationModuleViewModel(
                "checkout",
                "Check-Out",
                "Close active stays and prepare billing.",
                "Logout",
                _checkoutViewModel),
            new OperationModuleViewModel(
                "customers",
                "Customers & Booking",
                "Create guests, find rooms and create bookings.",
                "AccountSearch",
                _customerManagementViewModel),
            new OperationModuleViewModel(
                "services",
                "Services",
                "Maintain the hotel service catalog.",
                "InformationOutline",
                _serviceManagementViewModel),
            new OperationModuleViewModel(
                "service-orders",
                "Service Orders",
                "Record services used during a stay.",
                "ReceiptText",
                _serviceOrderViewModel),
            new OperationModuleViewModel(
                "billing",
                "Billing",
                "Create invoices and receive payments.",
                "CreditCardOutline",
                _billingViewModel),
            new OperationModuleViewModel(
                "room-types",
                "Room Types",
                "Manage room categories, price and capacity.",
                "FileDocumentPlus",
                _roomTypeManagementViewModel),
            new OperationModuleViewModel(
                "rooms",
                "Rooms",
                "Manage room inventory and operating status.",
                "OfficeBuilding",
                _roomManagementViewModel)
        ];
    }

    private async void SelectModule(OperationModuleViewModel? module)
    {
        if (module is null)
        {
            return;
        }

        try
        {
            SetSelectedModule(module);
            await InitializeSelectedModuleAsync();
        }
        catch (Exception ex)
        {
            OperationMessage = $"Unable to open {module.Title}: {ex.Message}";
        }
    }

    private void SetSelectedModule(OperationModuleViewModel module)
    {
        foreach (var item in Modules)
        {
            item.IsSelected = ReferenceEquals(item, module);
        }

        SelectedModule = module;
        CurrentViewModel = module.ViewModel;
        OperationMessage = string.Empty;
    }

    private async Task InitializeSelectedModuleAsync()
    {
        if (SelectedModule is null || _initializedModuleKeys.Contains(SelectedModule.Key))
        {
            return;
        }

        IsBusy = true;
        OperationMessage = string.Empty;

        try
        {
            await SelectedModule.ViewModel.InitializeAsync();
            _initializedModuleKeys.Add(SelectedModule.Key);
        }
        catch (Exception ex)
        {
            OperationMessage = $"Unable to load {SelectedModule.Title}: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
