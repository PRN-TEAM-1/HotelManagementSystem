using BusinessObjects.DTOs;
using Services.Interfaces;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class ServiceOrderViewModel : BaseViewModel
{
    private readonly IServiceOrderService _serviceOrderService;
    private readonly IServiceCatalogService _serviceCatalogService;
    private readonly ICurrentUserService _currentUserService;

    private int _bookingDetailId;
    private List<ServiceOrderListItemDto> _serviceOrders = new();
    private List<ServiceListItemDto> _availableServices = new();
    private ServiceOrderListItemDto? _selectedServiceOrder;
    private ServiceListItemDto? _selectedService;
    private string _quantityText = "1";
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public ServiceOrderViewModel(
        IServiceOrderService serviceOrderService,
        IServiceCatalogService serviceCatalogService,
        ICurrentUserService currentUserService)
    {
        _serviceOrderService = serviceOrderService ?? throw new ArgumentNullException(nameof(serviceOrderService));
        _serviceCatalogService = serviceCatalogService ?? throw new ArgumentNullException(nameof(serviceCatalogService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

        LoadServicesCommand = new AsyncRelayCommand(LoadServicesAsync, CanExecuteLoad);
        LoadServiceOrdersCommand = new AsyncRelayCommand(LoadServiceOrdersAsync, CanExecuteLoad);
        CreateServiceOrderCommand = new AsyncRelayCommand(CreateServiceOrderAsync, CanCreateServiceOrder);
        CancelServiceOrderCommand = new AsyncRelayCommand(CancelServiceOrderAsync, CanCancelServiceOrder);
        ClearMessagesCommand = new RelayCommand(ClearMessages);
        ResetFormCommand = new RelayCommand(ResetForm);
    }

    public override string Title => "Service Orders";

    public override string Description => "Record and manage service orders during guest stay";

    public int BookingDetailId
    {
        get => _bookingDetailId;
        set => SetProperty(ref _bookingDetailId, value);
    }

    public List<ServiceOrderListItemDto> ServiceOrders
    {
        get => _serviceOrders;
        private set => SetProperty(ref _serviceOrders, value);
    }

    public List<ServiceListItemDto> AvailableServices
    {
        get => _availableServices;
        private set => SetProperty(ref _availableServices, value);
    }

    public ServiceOrderListItemDto? SelectedServiceOrder
    {
        get => _selectedServiceOrder;
        set
        {
            if (SetProperty(ref _selectedServiceOrder, value))
            {
                CancelServiceOrderCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ServiceListItemDto? SelectedService
    {
        get => _selectedService;
        set
        {
            if (SetProperty(ref _selectedService, value))
            {
                CreateServiceOrderCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string QuantityText
    {
        get => _quantityText;
        set
        {
            if (SetProperty(ref _quantityText, value))
            {
                CreateServiceOrderCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public string SuccessMessage
    {
        get => _successMessage;
        private set => SetProperty(ref _successMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(CanEdit));
                LoadServicesCommand.RaiseCanExecuteChanged();
                LoadServiceOrdersCommand.RaiseCanExecuteChanged();
                CreateServiceOrderCommand.RaiseCanExecuteChanged();
                CancelServiceOrderCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanEdit => !IsBusy;

    public decimal ServiceOrderTotal
    {
        get => ServiceOrders.Sum(s => s.TotalPrice);
    }

    public AsyncRelayCommand LoadServicesCommand { get; }

    public AsyncRelayCommand LoadServiceOrdersCommand { get; }

    public AsyncRelayCommand CreateServiceOrderCommand { get; }

    public AsyncRelayCommand CancelServiceOrderCommand { get; }

    public RelayCommand ClearMessagesCommand { get; }

    public RelayCommand ResetFormCommand { get; }

    public void SetBookingDetailId(int bookingDetailId)
    {
        BookingDetailId = bookingDetailId;
    }

    public override async Task InitializeAsync()
    {
        await LoadServicesAsync();
        await LoadServiceOrdersAsync();
    }

    private bool CanExecuteLoad()
    {
        return !IsBusy;
    }

    private bool CanCreateServiceOrder()
    {
        return !IsBusy
            && SelectedService != null
            && int.TryParse(QuantityText, out var qty)
            && qty > 0
            && BookingDetailId > 0;
    }

    private bool CanCancelServiceOrder()
    {
        return !IsBusy && SelectedServiceOrder != null;
    }

    private async Task LoadServicesAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _serviceCatalogService.GetActiveServicesAsync();

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                AvailableServices = new();
                return;
            }

            AvailableServices = result.Data ?? new();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading services: {ex.Message}";
            AvailableServices = new();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadServiceOrdersAsync()
    {
        if (BookingDetailId <= 0)
        {
            ServiceOrders = new();
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _serviceOrderService.GetServiceOrdersByBookingDetailAsync(BookingDetailId);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                ServiceOrders = new();
                return;
            }

            ServiceOrders = result.Data ?? new();
            OnPropertyChanged(nameof(ServiceOrderTotal));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading service orders: {ex.Message}";
            ServiceOrders = new();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CreateServiceOrderAsync()
    {
        if (BookingDetailId <= 0)
        {
            ErrorMessage = "Booking detail not selected";
            return;
        }

        if (SelectedService is null)
        {
            ErrorMessage = "Please select a service";
            return;
        }

        if (!int.TryParse(QuantityText, out var quantity) || quantity <= 0)
        {
            ErrorMessage = "Invalid quantity";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var currentUser = _currentUserService.User;
            if (currentUser is null)
            {
                ErrorMessage = "User session not found";
                return;
            }

            var request = new ServiceOrderRequestDto
            {
                BookingDetailId = BookingDetailId,
                ServiceId = SelectedService.ServiceId,
                Quantity = quantity
            };

            var result = await _serviceOrderService.CreateServiceOrderAsync(request, currentUser.UserId);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                return;
            }

            SuccessMessage = $"Service order for '{SelectedService.ServiceName}' created successfully";
            ResetForm();
            await LoadServiceOrdersAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error creating service order: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CancelServiceOrderAsync()
    {
        if (SelectedServiceOrder is null)
        {
            ErrorMessage = "Please select a service order to cancel";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var result = await _serviceOrderService.CancelServiceOrderAsync(SelectedServiceOrder.ServiceOrderId);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                return;
            }

            SuccessMessage = $"Service order '{SelectedServiceOrder.ServiceName}' cancelled successfully";
            await LoadServiceOrdersAsync();
            SelectedServiceOrder = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error cancelling service order: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ResetForm()
    {
        SelectedService = null;
        QuantityText = "1";
    }

    private void ClearMessages()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }
}
