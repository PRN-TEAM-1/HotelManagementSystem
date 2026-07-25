using System.Collections.ObjectModel;
using BusinessObjects.DTOs;
using Services.Interfaces;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class RoomTypeManagementViewModel : BaseViewModel
{
    private readonly IRoomTypeService _roomTypeService;
    private ObservableCollection<RoomTypeListItemDto> _roomTypes = new();
    private RoomTypeListItemDto? _selectedRoomType;
    private string _searchTerm = string.Empty;
    private string _typeName = string.Empty;
    private string _description = string.Empty;
    private decimal _basePrice;
    private int _capacity = 2;
    private string _status = "Active";
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public RoomTypeManagementViewModel(IRoomTypeService roomTypeService)
    {
        _roomTypeService = roomTypeService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
        ClearCommand = new WPF.Commands.RelayCommand(ClearForm);
        ClearMessagesCommand = new WPF.Commands.RelayCommand(ClearMessages);
    }

    public override string Title => "Room Type Management";

    public override string Description => "Create and maintain room type definitions";

    public ObservableCollection<RoomTypeListItemDto> RoomTypes
    {
        get => _roomTypes;
        private set => SetProperty(ref _roomTypes, value);
    }

    public RoomTypeListItemDto? SelectedRoomType
    {
        get => _selectedRoomType;
        set
        {
            if (SetProperty(ref _selectedRoomType, value))
            {
                if (value is null)
                {
                    ClearForm();
                    return;
                }

                TypeName = value.TypeName;
                RoomTypeDescription = value.Description ?? string.Empty;
                BasePrice = value.BasePrice;
                Capacity = value.Capacity;
                Status = value.Status;
            }
        }
    }

    public string SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    public string TypeName
    {
        get => _typeName;
        set => SetProperty(ref _typeName, value);
    }

    public string RoomTypeDescription
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public decimal BasePrice
    {
        get => _basePrice;
        set => SetProperty(ref _basePrice, value);
    }

    public int Capacity
    {
        get => _capacity;
        set => SetProperty(ref _capacity, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
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
        private set => SetProperty(ref _isBusy, value);
    }

    public AsyncRelayCommand LoadCommand { get; }
    public AsyncRelayCommand SearchCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }
    public AsyncRelayCommand DeleteCommand { get; }
    public WPF.Commands.RelayCommand ClearCommand { get; }
    public WPF.Commands.RelayCommand ClearMessagesCommand { get; }

    public override async Task InitializeAsync()
    {
        await LoadAsync();
    }

    public override void OnNavigatedFrom()
    {
        base.OnNavigatedFrom();
        ClearMessages();
    }

    private async Task LoadAsync()
    {
        IsBusy = true;
        ClearMessages();
        try
        {
            var result = await _roomTypeService.GetRoomTypesAsync(SearchTerm);
            if (result.IsSuccess)
            {
                RoomTypes = new ObservableCollection<RoomTypeListItemDto>(result.Data ?? new List<RoomTypeListItemDto>());
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SearchAsync()
    {
        await LoadAsync();
    }

    private async Task DeleteAsync()
    {
        if (SelectedRoomType is null) return;

        var confirmResult = System.Windows.MessageBox.Show(
            $"Are you sure you want to delete room type '{SelectedRoomType.TypeName}'?",
            "Confirm Delete",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (confirmResult != System.Windows.MessageBoxResult.Yes) return;

        IsBusy = true;
        try
        {
            var result = await _roomTypeService.DeleteRoomTypeAsync(SelectedRoomType.RoomTypeId);
            if (result.IsSuccess)
            {
                SuccessMessage = result.Message;
                await LoadAsync();
                ClearForm();
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearForm()
    {
        SelectedRoomType = null;
        TypeName = string.Empty;
        RoomTypeDescription = string.Empty;
        BasePrice = 0;
        Capacity = 2;
        Status = "Active";
        ClearMessages();
    }

    private async Task SaveAsync()
    {
        ClearMessages();

        if (string.IsNullOrWhiteSpace(TypeName))
        {
            ErrorMessage = "Room type name is required.";
            return;
        }

        var actionText = SelectedRoomType is null ? "create" : "update";
        var confirmResult = System.Windows.MessageBox.Show(
            $"Are you sure you want to {actionText} room type '{TypeName}'?",
            "Confirm Save",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (confirmResult != System.Windows.MessageBoxResult.Yes)
        {
            return;
        }

        IsBusy = true;
        try
        {
            ServiceResult<RoomTypeListItemDto> result;
            if (SelectedRoomType is null)
            {
                result = await _roomTypeService.CreateRoomTypeAsync(new CreateRoomTypeRequestDto
                {
                    TypeName = TypeName,
                    Description = RoomTypeDescription,
                    BasePrice = BasePrice,
                    Capacity = Capacity,
                    Status = Status
                });
            }
            else
            {
                result = await _roomTypeService.UpdateRoomTypeAsync(new UpdateRoomTypeRequestDto
                {
                    RoomTypeId = SelectedRoomType.RoomTypeId,
                    TypeName = TypeName,
                    Description = RoomTypeDescription,
                    BasePrice = BasePrice,
                    Capacity = Capacity,
                    Status = Status
                });
            }

            if (result.IsSuccess)
            {
                SuccessMessage = result.Message;
                await LoadAsync();
                ClearForm();
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearMessages()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }
}
