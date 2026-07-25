using BusinessObjects.DTOs;
using Services.Interfaces;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class ServiceOrderViewModel : BaseViewModel
{
    private readonly IServiceOrderService _serviceOrderService;
    private readonly IServiceCatalogService _serviceCatalogService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICheckoutService _checkoutService;
    private readonly IAiServiceRecommendationService _aiServiceRecommendationService;

    private int _bookingDetailId;
    private List<CheckoutCandidateDto> _activeStays = new();
    private CheckoutCandidateDto? _selectedStay;
    private List<ServiceOrderListItemDto> _serviceOrders = new();
    private List<ServiceListItemDto> _availableServices = new();
    private ServiceOrderListItemDto? _selectedServiceOrder;
    private ServiceListItemDto? _selectedService;
    private List<AiRecommendedServiceDto> _aiRecommendations = new();
    private AiRecommendedServiceDto? _selectedAiRecommendation;
    private string _quantityText = "1";
    private string _guestPreferenceText = string.Empty;
    private string _aiRecommendationMessage = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;
    private bool _isAiRecommendationBusy;

    public ServiceOrderViewModel(
        IServiceOrderService serviceOrderService,
        IServiceCatalogService serviceCatalogService,
        ICurrentUserService currentUserService,
        ICheckoutService checkoutService,
        IAiServiceRecommendationService aiServiceRecommendationService)
    {
        _serviceOrderService = serviceOrderService ?? throw new ArgumentNullException(nameof(serviceOrderService));
        _serviceCatalogService = serviceCatalogService ?? throw new ArgumentNullException(nameof(serviceCatalogService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _checkoutService = checkoutService ?? throw new ArgumentNullException(nameof(checkoutService));
        _aiServiceRecommendationService = aiServiceRecommendationService ?? throw new ArgumentNullException(nameof(aiServiceRecommendationService));

        LoadServicesCommand = new AsyncRelayCommand(LoadServicesAsync, CanExecuteLoad);
        LoadServiceOrdersCommand = new AsyncRelayCommand(LoadServiceOrdersAsync, CanExecuteLoad);
        CreateServiceOrderCommand = new AsyncRelayCommand(CreateServiceOrderAsync, CanCreateServiceOrder);
        CancelServiceOrderCommand = new AsyncRelayCommand(CancelServiceOrderAsync, CanCancelServiceOrder);
        SuggestServicesCommand = new AsyncRelayCommand(SuggestServicesAsync, CanSuggestServices);
        UseRecommendationCommand = new RelayCommand(UseSelectedRecommendation, CanUseSelectedRecommendation);
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

    public List<CheckoutCandidateDto> ActiveStays
    {
        get => _activeStays;
        private set => SetProperty(ref _activeStays, value);
    }

    public CheckoutCandidateDto? SelectedStay
    {
        get => _selectedStay;
        set
        {
            if (SetProperty(ref _selectedStay, value))
            {
                BookingDetailId = value?.BookingDetailId ?? 0;
                ClearAiRecommendations();
                CreateServiceOrderCommand.RaiseCanExecuteChanged();
                SuggestServicesCommand.RaiseCanExecuteChanged();
                if (!IsBusy)
                {
                    _ = LoadServiceOrdersAsync();
                }
            }
        }
    }

    public List<AiRecommendedServiceDto> AiRecommendations
    {
        get => _aiRecommendations;
        private set
        {
            if (SetProperty(ref _aiRecommendations, value))
            {
                OnPropertyChanged(nameof(HasAiRecommendations));
            }
        }
    }

    public AiRecommendedServiceDto? SelectedAiRecommendation
    {
        get => _selectedAiRecommendation;
        set
        {
            if (SetProperty(ref _selectedAiRecommendation, value))
            {
                UseRecommendationCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public List<ServiceOrderListItemDto> ServiceOrders
    {
        get => _serviceOrders;
        private set => SetProperty(ref _serviceOrders, value);
    }

    public List<ServiceListItemDto> AvailableServices
    {
        get => _availableServices;
        private set
        {
            if (SetProperty(ref _availableServices, value))
            {
                SuggestServicesCommand.RaiseCanExecuteChanged();
            }
        }
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

    public string GuestPreferenceText
    {
        get => _guestPreferenceText;
        set => SetProperty(ref _guestPreferenceText, value);
    }

    public string AiRecommendationMessage
    {
        get => _aiRecommendationMessage;
        private set => SetProperty(ref _aiRecommendationMessage, value);
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
                SuggestServicesCommand.RaiseCanExecuteChanged();
                UseRecommendationCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsAiRecommendationBusy
    {
        get => _isAiRecommendationBusy;
        private set
        {
            if (SetProperty(ref _isAiRecommendationBusy, value))
            {
                OnPropertyChanged(nameof(CanEdit));
                SuggestServicesCommand.RaiseCanExecuteChanged();
                UseRecommendationCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanEdit => !IsBusy && !IsAiRecommendationBusy;

    public bool HasAiRecommendations => AiRecommendations.Count > 0;

    public decimal ServiceOrderTotal
    {
        get => ServiceOrders.Sum(s => s.TotalPrice);
    }

    public AsyncRelayCommand LoadServicesCommand { get; }

    public AsyncRelayCommand LoadServiceOrdersCommand { get; }

    public AsyncRelayCommand CreateServiceOrderCommand { get; }

    public AsyncRelayCommand CancelServiceOrderCommand { get; }

    public AsyncRelayCommand SuggestServicesCommand { get; }

    public RelayCommand UseRecommendationCommand { get; }

    public RelayCommand ClearMessagesCommand { get; }

    public RelayCommand ResetFormCommand { get; }

    public void SetBookingDetailId(int bookingDetailId)
    {
        BookingDetailId = bookingDetailId;
    }

    public override async Task InitializeAsync()
    {
        await LoadActiveStaysAsync();
        await LoadServicesAsync();
        await LoadServiceOrdersAsync();
    }

    public override void OnNavigatedTo()
    {
        base.OnNavigatedTo();
        if (!IsBusy)
        {
            _ = LoadServiceOrdersAsync();
        }
    }

    private bool CanExecuteLoad()
    {
        return !IsBusy;
    }

    private bool CanCreateServiceOrder()
    {
        return !IsBusy
            && SelectedService != null
            && SelectedStay != null
            && int.TryParse(QuantityText, out var qty)
            && qty > 0;
    }

    private bool CanCancelServiceOrder()
    {
        return !IsBusy && SelectedServiceOrder != null;
    }

    private bool CanSuggestServices()
    {
        return !IsBusy
            && !IsAiRecommendationBusy
            && SelectedStay is not null
            && BookingDetailId > 0
            && AvailableServices.Count > 0;
    }

    private bool CanUseSelectedRecommendation()
    {
        return !IsBusy
            && !IsAiRecommendationBusy
            && SelectedAiRecommendation is not null;
    }

    private async Task LoadActiveStaysAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _checkoutService.GetCheckoutCandidatesAsync();

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                ActiveStays = new();
                return;
            }

            ActiveStays = result.Data ?? new();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading active stays: {ex.Message}";
            ActiveStays = new();
        }
        finally
        {
            IsBusy = false;
        }
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
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var previousBookingDetailId = BookingDetailId;
            var staysResult = await _checkoutService.GetCheckoutCandidatesAsync();
            
            if (staysResult.IsSuccess)
            {
                ActiveStays = staysResult.Data ?? new();
                if (previousBookingDetailId > 0)
                {
                    _selectedStay = ActiveStays.FirstOrDefault(x => x.BookingDetailId == previousBookingDetailId);
                    BookingDetailId = _selectedStay?.BookingDetailId ?? 0;
                    OnPropertyChanged(nameof(SelectedStay));
                }
            }

            if (BookingDetailId <= 0)
            {
                ServiceOrders = new();
                IsBusy = false;
                return;
            }

            var result = await _serviceOrderService.GetServiceOrdersByBookingDetailAsync(BookingDetailId);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                ServiceOrders = new();
                IsBusy = false;
                return;
            }

            ServiceOrders = result.Data ?? new();
            OnPropertyChanged(nameof(ServiceOrderTotal));
            SuggestServicesCommand.RaiseCanExecuteChanged();
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

            var result = await _serviceOrderService.CreateServiceOrderAsync(request, currentUser);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                return;
            }

            SuccessMessage = $"Service order for '{SelectedService.ServiceName}' created successfully";
            ResetForm();
            ClearAiRecommendations();
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

    private async Task SuggestServicesAsync()
    {
        if (BookingDetailId <= 0 || SelectedStay is null)
        {
            ErrorMessage = "Please select a stay before requesting AI suggestions.";
            return;
        }

        IsAiRecommendationBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        AiRecommendationMessage = string.Empty;
        AiRecommendations = new();
        SelectedAiRecommendation = null;

        try
        {
            var result = await _aiServiceRecommendationService.GetRecommendationsAsync(
                new AiServiceRecommendationRequestDto
                {
                    BookingDetailId = BookingDetailId,
                    GuestPreference = GuestPreferenceText,
                    MaxRecommendations = 3
                },
                _currentUserService.User);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                return;
            }

            var response = result.Data;
            AiRecommendations = response?.Recommendations ?? new();
            SelectedAiRecommendation = AiRecommendations.FirstOrDefault();

            AiRecommendationMessage = response is null
                ? string.Empty
                : $"{response.Summary} ({response.ProviderName} / {response.ModelName})";

            if (AiRecommendations.Count == 0)
            {
                AiRecommendationMessage = "AI did not find a new suitable service for this stay.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading AI recommendations: {ex.Message}";
        }
        finally
        {
            IsAiRecommendationBusy = false;
        }
    }

    private void UseSelectedRecommendation()
    {
        if (SelectedAiRecommendation is null)
        {
            ErrorMessage = "Please select an AI recommendation first.";
            return;
        }

        var service = AvailableServices.FirstOrDefault(item =>
            item.ServiceId == SelectedAiRecommendation.ServiceId);

        if (service is null)
        {
            ErrorMessage = "The recommended service is no longer active.";
            return;
        }

        SelectedService = service;
        QuantityText = SelectedAiRecommendation.SuggestedQuantity.ToString();
        AiRecommendationMessage = SelectedAiRecommendation.UpsellMessage;
    }

    private void ClearAiRecommendations()
    {
        AiRecommendations = new();
        SelectedAiRecommendation = null;
        AiRecommendationMessage = string.Empty;
    }

    private void ClearMessages()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }
}
