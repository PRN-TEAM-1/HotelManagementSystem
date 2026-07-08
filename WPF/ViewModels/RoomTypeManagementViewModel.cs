using System.Collections.ObjectModel;
using BusinessObjects.DTOs;
using Services.Interfaces;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class RoomTypeManagementViewModel : BaseViewModel
{
    private readonly IRoomTypeService _roomTypeService;
    private ObservableCollection<RoomTypeListItemDto> _roomTypes = new();
    private string _searchTerm = string.Empty;
    private string _typeName = string.Empty;
    private string _description = string.Empty;
    private decimal _basePrice;
    private int _capacity = 2;
    private string _status = "Active";
    private string _message = string.Empty;
    private bool _isBusy;

    public RoomTypeManagementViewModel(IRoomTypeService roomTypeService)
    {
        _roomTypeService = roomTypeService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        CreateCommand = new AsyncRelayCommand(CreateAsync);
    }

    public override string Title => "Room Type Management";

    public override string Description => "Create and maintain room type definitions";

    public ObservableCollection<RoomTypeListItemDto> RoomTypes
    {
        get => _roomTypes;
        private set => SetProperty(ref _roomTypes, value);
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

    public string Message
    {
        get => _message;
        private set => SetProperty(ref _message, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public AsyncRelayCommand LoadCommand { get; }

    public AsyncRelayCommand SearchCommand { get; }

    public AsyncRelayCommand CreateCommand { get; }

    public override async Task InitializeAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var result = await _roomTypeService.GetRoomTypesAsync(SearchTerm);
            if (result.IsSuccess)
            {
                RoomTypes = new ObservableCollection<RoomTypeListItemDto>(result.Data ?? new List<RoomTypeListItemDto>());
            }
            else
            {
                Message = result.Message;
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
        if (string.IsNullOrWhiteSpace(TypeName))
        {
            Message = "Room type name is required.";
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
            });

            Message = result.Message;
            if (result.IsSuccess)
            {
                TypeName = string.Empty;
                RoomTypeDescription = string.Empty;
                BasePrice = 0;
                Capacity = 2;
                Status = "Active";
                await LoadAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
