using System.Collections.ObjectModel;

using BusinessObjects.DTOs;
using Services.Interfaces;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class FloorRoomGroupViewModel
{
    public int Floor { get; set; }

    public string FloorName => $"Floor {Floor}";

    public ObservableCollection<RoomMapItemDto> Rooms { get; set; } = new();
}

public sealed class RoomMapViewModel : BaseViewModel
{
    private readonly IRoomService _roomService;
    private readonly ICurrentUserService _currentUserService;

    private DateTime _asOfDate = DateTime.Today;
    private string _selectedStatusFilter = "All";
    private ObservableCollection<FloorRoomGroupViewModel> _roomGroups = new();
    private List<RoomMapItemDto> _allMapRooms = new();
    private string _message = string.Empty;
    private bool _isBusy;

    private int _totalRooms;
    private int _availableCount;
    private int _occupiedCount;
    private int _reservedCount;
    private int _cleaningCount;
    private int _maintenanceCount;

    public RoomMapViewModel(
        IRoomService roomService,
        ICurrentUserService currentUserService)
    {
        _roomService = roomService ?? throw new ArgumentNullException(nameof(roomService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

        LoadCommand = new AsyncRelayCommand(LoadRoomMapAsync);
        RefreshCommand = new AsyncRelayCommand(LoadRoomMapAsync);
        MarkCleanedCommand = new AsyncRelayCommand<RoomMapItemDto>(MarkCleanedAsync);
    }

    public override string Title => "Room Map";

    public override string Description => "Real-time room status, floor layout and occupancy map";

    public AsyncRelayCommand<RoomMapItemDto> MarkCleanedCommand { get; }


    public DateTime AsOfDate
    {
        get => _asOfDate;
        set
        {
            if (SetProperty(ref _asOfDate, value))
            {
                _ = LoadRoomMapAsync();
            }
        }
    }

    public string SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            if (SetProperty(ref _selectedStatusFilter, value))
            {
                ApplyFilter();
            }
        }
    }

    public ObservableCollection<FloorRoomGroupViewModel> RoomGroups
    {
        get => _roomGroups;
        private set => SetProperty(ref _roomGroups, value);
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

    public int TotalRooms
    {
        get => _totalRooms;
        private set => SetProperty(ref _totalRooms, value);
    }

    public int AvailableCount
    {
        get => _availableCount;
        private set => SetProperty(ref _availableCount, value);
    }

    public int OccupiedCount
    {
        get => _occupiedCount;
        private set => SetProperty(ref _occupiedCount, value);
    }

    public int ReservedCount
    {
        get => _reservedCount;
        private set => SetProperty(ref _reservedCount, value);
    }

    public int CleaningCount
    {
        get => _cleaningCount;
        private set => SetProperty(ref _cleaningCount, value);
    }

    public int MaintenanceCount
    {
        get => _maintenanceCount;
        private set => SetProperty(ref _maintenanceCount, value);
    }

    public AsyncRelayCommand LoadCommand { get; }

    public AsyncRelayCommand RefreshCommand { get; }

    public override async Task InitializeAsync()
    {
        await LoadRoomMapAsync();
    }

    private async Task LoadRoomMapAsync()
    {
        IsBusy = true;
        Message = string.Empty;

        try
        {
            var result = await _roomService.GetRoomMapAsync(AsOfDate);
            if (result.IsSuccess && result.Data != null)
            {
                _allMapRooms = result.Data;

                TotalRooms = _allMapRooms.Count;
                AvailableCount = _allMapRooms.Count(r => r.Status.Equals("Available", StringComparison.OrdinalIgnoreCase));
                OccupiedCount = _allMapRooms.Count(r => r.Status.Equals("Occupied", StringComparison.OrdinalIgnoreCase));
                ReservedCount = _allMapRooms.Count(r => r.Status.Equals("Reserved", StringComparison.OrdinalIgnoreCase));
                CleaningCount = _allMapRooms.Count(r => r.Status.Equals("Cleaning", StringComparison.OrdinalIgnoreCase));
                MaintenanceCount = _allMapRooms.Count(r => r.Status.Equals("Maintenance", StringComparison.OrdinalIgnoreCase));

                ApplyFilter();
            }
            else
            {
                Message = result.Message ?? "Failed to load room map.";
            }
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        var filtered = _allMapRooms.AsEnumerable();

        if (!string.Equals(SelectedStatusFilter, "All", StringComparison.OrdinalIgnoreCase))
        {
            filtered = filtered.Where(r => r.Status.Equals(SelectedStatusFilter, StringComparison.OrdinalIgnoreCase));
        }

        var groups = filtered
            .GroupBy(r => r.Floor)
            .OrderBy(g => g.Key)
            .Select(g => new FloorRoomGroupViewModel
            {
                Floor = g.Key,
                Rooms = new ObservableCollection<RoomMapItemDto>(g.OrderBy(r => r.RoomNumber))
            })
            .ToList();

        RoomGroups = new ObservableCollection<FloorRoomGroupViewModel>(groups);
    }

    private async Task MarkCleanedAsync(RoomMapItemDto? roomItem)
    {
        if (roomItem is null) return;

        var confirmResult = System.Windows.MessageBox.Show(
            $"Are you sure you want to mark Room {roomItem.RoomNumber} as Cleaned & Available?",
            "Confirm Room Cleaning",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (confirmResult != System.Windows.MessageBoxResult.Yes)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var updateResult = await _roomService.UpdateRoomAsync(new UpdateRoomRequestDto
            {
                RoomId = roomItem.RoomId,
                RoomTypeId = roomItem.RoomTypeId,
                RoomNumber = roomItem.RoomNumber,
                Floor = roomItem.Floor,
                Status = "Available"
            }, _currentUserService.User);

            if (updateResult.IsSuccess)
            {
                Message = $"Room {roomItem.RoomNumber} marked as Cleaned & Available.";
                System.Windows.MessageBox.Show(Message, "Room Status Updated", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                await LoadRoomMapAsync();
            }
            else
            {
                Message = updateResult.Message ?? "Failed to update room status.";
                System.Windows.MessageBox.Show(Message, "Update Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            Message = ex.Message;
            System.Windows.MessageBox.Show(ex.Message, "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
}

