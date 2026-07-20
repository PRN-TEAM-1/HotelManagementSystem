using BusinessObjects.DTOs;
using Services.Interfaces;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class ServiceManagementViewModel : BaseViewModel
{
    private readonly IServiceCatalogService _serviceCatalogService;

    private List<ServiceListItemDto> _services = new();
    private ServiceListItemDto? _selectedService;
    private ServiceDto? _selectedServiceDetails;
    private string _serviceName = string.Empty;
    private string _category = string.Empty;
    private string _priceText = string.Empty;
    private string _selectedStatus = "Active";
    private string _searchTerm = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;
    private bool _isEditMode;

    public ServiceManagementViewModel(IServiceCatalogService serviceCatalogService)
    {
        _serviceCatalogService = serviceCatalogService ?? throw new ArgumentNullException(nameof(serviceCatalogService));

        LoadServicesCommand = new AsyncRelayCommand(LoadServicesAsync, CanExecuteLoad);
        SearchServicesCommand = new AsyncRelayCommand(SearchServicesAsync, CanExecuteSearch);
        CreateServiceCommand = new AsyncRelayCommand(CreateServiceAsync, CanCreateService);
        UpdateServiceCommand = new AsyncRelayCommand(UpdateServiceAsync, CanUpdateService);
        ClearMessagesCommand = new RelayCommand(ClearMessages);
        ResetFormCommand = new RelayCommand(ResetForm);
        SelectServiceCommand = new RelayCommand<ServiceListItemDto>(SelectService);
    }

    public override string Title => "Service Management";

    public override string Description => "Manage service catalog and pricing";

    public List<ServiceListItemDto> Services
    {
        get => _services;
        private set => SetProperty(ref _services, value);
    }

    public ServiceListItemDto? SelectedService
    {
        get => _selectedService;
        set
        {
            if (SetProperty(ref _selectedService, value))
            {
                SelectService(value);
                SelectServiceCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ServiceDto? SelectedServiceDetails
    {
        get => _selectedServiceDetails;
        private set
        {
            if (SetProperty(ref _selectedServiceDetails, value))
            {
                UpdateServiceCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string ServiceName
    {
        get => _serviceName;
        set
        {
            if (SetProperty(ref _serviceName, value))
            {
                CreateServiceCommand.RaiseCanExecuteChanged();
                UpdateServiceCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string Category
    {
        get => _category;
        set
        {
            if (SetProperty(ref _category, value))
            {
                CreateServiceCommand.RaiseCanExecuteChanged();
                UpdateServiceCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string PriceText
    {
        get => _priceText;
        set
        {
            if (SetProperty(ref _priceText, value))
            {
                CreateServiceCommand.RaiseCanExecuteChanged();
                UpdateServiceCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public List<string> AvailableStatuses { get; } = new() { "Active", "Inactive" };

    public string SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            if (SetProperty(ref _selectedStatus, value))
            {
                UpdateServiceCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (SetProperty(ref _searchTerm, value))
            {
                SearchServicesCommand.RaiseCanExecuteChanged();
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
                SearchServicesCommand.RaiseCanExecuteChanged();
                CreateServiceCommand.RaiseCanExecuteChanged();
                UpdateServiceCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanEdit => !IsBusy;

    public bool IsEditMode
    {
        get => _isEditMode;
        set
        {
            if (SetProperty(ref _isEditMode, value))
            {
                OnPropertyChanged(nameof(IsCreateMode));
            }
        }
    }

    public bool IsCreateMode => !IsEditMode;

    public AsyncRelayCommand LoadServicesCommand { get; }

    public AsyncRelayCommand SearchServicesCommand { get; }

    public AsyncRelayCommand CreateServiceCommand { get; }

    public AsyncRelayCommand UpdateServiceCommand { get; }

    public RelayCommand ClearMessagesCommand { get; }

    public RelayCommand ResetFormCommand { get; }

    public RelayCommand<ServiceListItemDto> SelectServiceCommand { get; }

    public override async Task InitializeAsync()
    {
        await LoadServicesAsync();
    }

    private bool CanExecuteLoad()
    {
        return !IsBusy;
    }

    private bool CanExecuteSearch()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(SearchTerm);
    }

    private bool CanCreateService()
    {
        return !IsBusy
            && !string.IsNullOrWhiteSpace(ServiceName)
            && !string.IsNullOrWhiteSpace(Category)
            && !string.IsNullOrWhiteSpace(PriceText)
            && decimal.TryParse(PriceText, out _);
    }

    private bool CanUpdateService()
    {
        return !IsBusy
            && SelectedServiceDetails != null
            && !string.IsNullOrWhiteSpace(ServiceName)
            && !string.IsNullOrWhiteSpace(Category)
            && !string.IsNullOrWhiteSpace(PriceText)
            && decimal.TryParse(PriceText, out _);
    }

    private async Task LoadServicesAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var result = await _serviceCatalogService.GetAllServicesAsync();

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                Services = new();
                return;
            }

            Services = result.Data ?? new();
            SearchTerm = string.Empty;
            ResetForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading services: {ex.Message}";
            Services = new();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SearchServicesAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            await LoadServicesAsync();
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var result = await _serviceCatalogService.SearchServicesAsync(SearchTerm.Trim());

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                Services = new();
                return;
            }

            Services = result.Data ?? new();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error searching services: {ex.Message}";
            Services = new();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CreateServiceAsync()
    {
        if (!decimal.TryParse(PriceText, out var price) || price <= 0)
        {
            ErrorMessage = "Price must be a valid number greater than 0";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var request = new CreateServiceRequestDto
            {
                ServiceName = ServiceName.Trim(),
                Category = Category.Trim(),
                Price = price
            };

            var result = await _serviceCatalogService.CreateServiceAsync(request);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                return;
            }

            SuccessMessage = $"Service '{ServiceName}' created successfully";
            ResetForm();
            await LoadServicesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error creating service: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UpdateServiceAsync()
    {
        if (SelectedServiceDetails is null)
        {
            ErrorMessage = "Please select a service to update";
            return;
        }

        if (!decimal.TryParse(PriceText, out var price) || price <= 0)
        {
            ErrorMessage = "Price must be a valid number greater than 0";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var request = new UpdateServiceRequestDto
            {
                ServiceId = SelectedServiceDetails.ServiceId,
                ServiceName = ServiceName.Trim(),
                Category = Category.Trim(),
                Price = price,
                Status = SelectedStatus
            };

            var result = await _serviceCatalogService.UpdateServiceAsync(request);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                return;
            }

            SuccessMessage = $"Service '{ServiceName}' updated successfully";
            IsEditMode = false;
            ResetForm();
            await LoadServicesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error updating service: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SelectService(ServiceListItemDto? service)
    {
        if (service is null)
        {
            SelectedServiceDetails = null;
            IsEditMode = false;
            ResetForm();
            return;
        }

        IsEditMode = true;
        ServiceName = service.ServiceName;
        Category = service.Category;
        PriceText = service.Price.ToString("F2");
        SelectedStatus = service.Status;
        SelectedServiceDetails = new ServiceDto
        {
            ServiceId = service.ServiceId,
            ServiceName = service.ServiceName,
            Category = service.Category,
            Price = service.Price,
            Status = service.Status
        };
    }

    private void ResetForm()
    {
        ServiceName = string.Empty;
        Category = string.Empty;
        PriceText = string.Empty;
        SelectedStatus = "Active";
        SelectedServiceDetails = null;
        IsEditMode = false;
    }

    private void ClearMessages()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }
}
