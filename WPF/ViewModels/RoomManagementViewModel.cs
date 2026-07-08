using System.Collections.ObjectModel;
using BusinessObjects.DTOs;
using Services.Interfaces;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class RoomManagementViewModel : BaseViewModel
{
    private readonly IRoomService _roomService;
    private readonly IRoomTypeService _roomTypeService;

    private ObservableCollection<RoomListItemDto> _rooms = new();
    private ObservableCollection<RoomTypeListItemDto> _roomTypes = new();
    private ObservableCollection<string> _statusOptions = new() { "Available", "Cleaning", "Maintenance", "Inactive" };
    private RoomListItemDto? _selectedRoom;
    private string _searchTerm = string.Empty;
    private string _roomNumber = string.Empty;
    private int _selectedRoomTypeId;
    private int _floor;
    private string _status = "Available";
    private string _note = string.Empty;
    private string _message = string.Empty;
    private bool _isBusy;

    public RoomManagementViewModel(IRoomService roomService, IRoomTypeService roomTypeService)
    {
        _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
        _roomTypeService = roomTypeService ?? throw new ArgumentNullException(nameof(roomTypeService));

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        ClearCommand = new RelayCommand(ClearForm);
    }

    public override string Title => "Room Management";

    public override string Description => "Create, update and review room inventory";

    public ObservableCollection<RoomListItemDto> Rooms
    {
        get => _rooms;
        private set => SetProperty(ref _rooms, value);
    }

    public ObservableCollection<RoomTypeListItemDto> RoomTypes
    {
        get => _roomTypes;
        private set => SetProperty(ref _roomTypes, value);
    }

    public ObservableCollection<string> StatusOptions => _statusOptions;

    public RoomListItemDto? SelectedRoom
    {
        get => _selectedRoom;
        set
        {
            if (SetProperty(ref _selectedRoom, value))
            {
                if (value is null)
                {
                    ClearForm();
                    return;
                }

                RoomNumber = value.RoomNumber;
                SelectedRoomTypeId = value.RoomTypeId;
                Floor = value.Floor;
                Status = value.Status;
                Note = value.Note ?? string.Empty;
            }
        }
    }

    public string SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    public string RoomNumber
    {
        get => _roomNumber;
        set => SetProperty(ref _roomNumber, value);
    }

    public int SelectedRoomTypeId
    {
        get => _selectedRoomTypeId;
        set => SetProperty(ref _selectedRoomTypeId, value);
    }

    public int Floor
    {
        get => _floor;
        set => SetProperty(ref _floor, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
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

    public AsyncRelayCommand SaveCommand { get; }

    public RelayCommand ClearCommand { get; }

    public override async Task InitializeAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsBusy = true;
        Message = string.Empty;

        try
        {
            var roomTypeResult = await _roomTypeService.GetRoomTypesAsync();
            if (roomTypeResult.IsSuccess)
            {
                RoomTypes = new ObservableCollection<RoomTypeListItemDto>(roomTypeResult.Data ?? new List<RoomTypeListItemDto>());
            }

            var roomResult = await _roomService.GetRoomsAsync(SearchTerm);
            if (roomResult.IsSuccess)
            {
                Rooms = new ObservableCollection<RoomListItemDto>(roomResult.Data ?? new List<RoomListItemDto>());
            }
            else
            {
                Message = roomResult.Message;
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

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(RoomNumber))
        {
            Message = "Room number is required.";
            return;
        }

        if (SelectedRoomTypeId <= 0)
        {
            Message = "Please select a room type.";
            return;
        }

        IsBusy = true;
        try
        {
            ServiceResult<RoomListItemDto> result;
            if (SelectedRoom is null)
            {
                result = await _roomService.CreateRoomAsync(new CreateRoomRequestDto
                {
                    RoomTypeId = SelectedRoomTypeId,
                    RoomNumber = RoomNumber,
                    Floor = Floor,
                    Status = Status,
                    Note = Note
                });
            }
            else
            {
                result = await _roomService.UpdateRoomAsync(new UpdateRoomRequestDto
                {
                    RoomId = SelectedRoom.RoomId,
                    RoomTypeId = SelectedRoomTypeId,
                    RoomNumber = RoomNumber,
                    Floor = Floor,
                    Status = Status,
                    Note = Note
                });
            }

            Message = result.Message;
            if (result.IsSuccess)
            {
                ClearForm();
                await LoadAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearForm()
    {
        SelectedRoom = null;
        RoomNumber = string.Empty;
        SelectedRoomTypeId = 0;
        Floor = 0;
        Status = "Available";
        Note = string.Empty;
        Message = string.Empty;
    }
}
