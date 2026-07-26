using System.Collections.ObjectModel;
using BusinessObjects.DTOs;
using Services.Interfaces;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class RoomTypeManagementViewModel : BaseViewModel
{
    private readonly IRoomTypeService _roomTypeService;
    private readonly ICurrentUserService _currentUserService;
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
    private bool _isEditMode;

    public RoomTypeManagementViewModel(
        IRoomTypeService roomTypeService,
        ICurrentUserService currentUserService)
    {
        _roomTypeService = roomTypeService;
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        LoadCommand = new AsyncRelayCommand(() => LoadAsync());
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        CreateCommand = new AsyncRelayCommand(CreateAsync);
        UpdateCommand = new AsyncRelayCommand(UpdateAsync);
        ResetFormCommand = new RelayCommand(ResetForm);
        ClearMessagesCommand = new RelayCommand(ClearMessages);
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
                    IsEditMode = false;
                    ClearFormFields();
                    return;
                }

                IsEditMode = true;
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

    public IReadOnlyList<string> StatusOptions { get; } = ["Active", "Inactive"];

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

    public bool IsEditMode
    {
        get => _isEditMode;
        private set
        {
            if (SetProperty(ref _isEditMode, value))
            {
                OnPropertyChanged(nameof(IsCreateMode));
            }
        }
    }

    public bool IsCreateMode => !IsEditMode;

    public AsyncRelayCommand LoadCommand { get; }

    public AsyncRelayCommand SearchCommand { get; }

    public AsyncRelayCommand CreateCommand { get; }

    public AsyncRelayCommand UpdateCommand { get; }

    public RelayCommand ResetFormCommand { get; }

    public RelayCommand ClearMessagesCommand { get; }

    public override async Task InitializeAsync()
    {
        await LoadAsync();
    }

    public override void OnNavigatedFrom()
    {
        base.OnNavigatedFrom();
        ClearMessages();
    }

    private async Task LoadAsync(bool clearMessages = true)
    {
        IsBusy = true;
        if (clearMessages)
        {
            ClearMessages();
        }

        try
        {
            var selectedRoomTypeId = SelectedRoomType?.RoomTypeId;
            var result = await _roomTypeService.GetRoomTypesAsync(SearchTerm);
            if (result.IsSuccess)
            {
                RoomTypes = new ObservableCollection<RoomTypeListItemDto>(result.Data ?? new List<RoomTypeListItemDto>());
                if (selectedRoomTypeId.HasValue)
                {
                    SelectedRoomType = RoomTypes.FirstOrDefault(roomType =>
                        roomType.RoomTypeId == selectedRoomTypeId.Value);
                }
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

    private async Task CreateAsync()
    {
        ClearMessages();

        if (string.IsNullOrWhiteSpace(TypeName))
        {
            ErrorMessage = "Room type name is required.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _roomTypeService.CreateRoomTypeAsync(new CreateRoomTypeRequestDto
            {
                TypeName = TypeName,
                Description = RoomTypeDescription,
                BasePrice = BasePrice,
                Capacity = Capacity,
                Status = Status
            }, _currentUserService.User);

            if (result.IsSuccess)
            {
                SuccessMessage = result.Message;
                ResetForm();
                await LoadAsync(clearMessages: false);
            }
            else
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task UpdateAsync()
    {
        ClearMessages();

        if (SelectedRoomType is null)
        {
            ErrorMessage = "Please select a room type to update.";
            return;
        }

        if (string.IsNullOrWhiteSpace(TypeName))
        {
            ErrorMessage = "Room type name is required.";
            return;
        }

        IsBusy = true;
        try
        {
            var updatedRoomTypeId = SelectedRoomType.RoomTypeId;
            var result = await _roomTypeService.UpdateRoomTypeAsync(new UpdateRoomTypeRequestDto
            {
                RoomTypeId = updatedRoomTypeId,
                TypeName = TypeName,
                Description = RoomTypeDescription,
                BasePrice = BasePrice,
                Capacity = Capacity,
                Status = Status
            }, _currentUserService.User);

            if (result.IsSuccess)
            {
                SuccessMessage = result.Message;
                await LoadAsync(clearMessages: false);
                SelectedRoomType = RoomTypes.FirstOrDefault(roomType => roomType.RoomTypeId == updatedRoomTypeId);
            }
            else
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ResetForm()
    {
        SelectedRoomType = null;
    }

    private void ClearFormFields()
    {
        TypeName = string.Empty;
        RoomTypeDescription = string.Empty;
        BasePrice = 0;
        Capacity = 2;
        Status = "Active";
    }

    private void ClearMessages()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }
}
