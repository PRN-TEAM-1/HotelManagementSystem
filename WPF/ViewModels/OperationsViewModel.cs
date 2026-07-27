using System.Collections.ObjectModel;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class OperationsViewModel : BaseViewModel
{
    private readonly CheckInViewModel _checkInViewModel;
    private readonly CheckoutViewModel _checkoutViewModel;
    private readonly ServiceOrderViewModel _serviceOrderViewModel;
    private readonly BillingViewModel _billingViewModel;
    private readonly CustomerManagementViewModel _customerManagementViewModel;
    private readonly RoomMapViewModel _roomMapViewModel;

    private BaseViewModel _currentViewModel;
    private OperationModuleViewModel? _selectedModule;
    private string _operationMessage = string.Empty;
    private bool _isBusy;
    private readonly HashSet<string> _initializedModuleKeys = new(StringComparer.OrdinalIgnoreCase);

    public OperationsViewModel(
        CheckInViewModel checkInViewModel,
        CheckoutViewModel checkoutViewModel,
        ServiceOrderViewModel serviceOrderViewModel,
        BillingViewModel billingViewModel,
        CustomerManagementViewModel customerManagementViewModel,
        RoomMapViewModel roomMapViewModel)
    {
        _checkInViewModel = checkInViewModel ?? throw new ArgumentNullException(nameof(checkInViewModel));
        _checkoutViewModel = checkoutViewModel ?? throw new ArgumentNullException(nameof(checkoutViewModel));
        _serviceOrderViewModel = serviceOrderViewModel ?? throw new ArgumentNullException(nameof(serviceOrderViewModel));
        _billingViewModel = billingViewModel ?? throw new ArgumentNullException(nameof(billingViewModel));
        _customerManagementViewModel = customerManagementViewModel ?? throw new ArgumentNullException(nameof(customerManagementViewModel));
        _roomMapViewModel = roomMapViewModel ?? throw new ArgumentNullException(nameof(roomMapViewModel));


        _currentViewModel = roomMapViewModel;
        Modules = new ObservableCollection<OperationModuleViewModel>(CreateModules());
        SelectModuleCommand = new RelayCommand<OperationModuleViewModel>(SelectModule);

        if (Modules.FirstOrDefault() is { } firstModule)
        {
            SetSelectedModule(firstModule);
        }
    }

    public override string Title => "Operations";

    public override string Description => "Guest booking, stay service and billing tasks";

    public ObservableCollection<OperationModuleViewModel> Modules { get; }

    public RelayCommand<OperationModuleViewModel> SelectModuleCommand { get; }

    public CheckInViewModel CheckInViewModel => _checkInViewModel;

    public CheckoutViewModel CheckoutViewModel => _checkoutViewModel;

    public ServiceOrderViewModel ServiceOrderViewModel => _serviceOrderViewModel;

    public BillingViewModel BillingViewModel => _billingViewModel;

    public CustomerManagementViewModel CustomerManagementViewModel => _customerManagementViewModel;

    public RoomMapViewModel RoomMapViewModel => _roomMapViewModel;

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
        var modules = new List<OperationModuleViewModel>
        {
            new OperationModuleViewModel(
                "room-map",
                "Room Map",
                "Real-time room occupancy and operating status.",
                "ViewGrid",
                _roomMapViewModel),
            new OperationModuleViewModel(
                "customers",
                "Customers & Booking",
                "Create guests, find rooms and create bookings.",
                "AccountSearch",
                _customerManagementViewModel),
            new OperationModuleViewModel(
                "check-in",
                "Check-In",
                "Confirm reserved rooms and start a stay.",
                "Login",
                _checkInViewModel),
            new OperationModuleViewModel(
                "service-orders",
                "Service Orders",
                "Record services used during a stay.",
                "ReceiptText",
                _serviceOrderViewModel),
            new OperationModuleViewModel(
                "checkout",
                "Check-Out",
                "Close active stays and prepare billing.",
                "Logout",
                _checkoutViewModel)
        };

        modules.Add(new OperationModuleViewModel(
            "billing",
            "Billing",
            "Create invoices and receive payments.",
            "CreditCardOutline",
            _billingViewModel));

        return modules;
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

        module.ViewModel.OnNavigatedTo();
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
