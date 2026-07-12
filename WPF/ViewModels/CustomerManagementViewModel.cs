using System.Collections.ObjectModel;
using BusinessObjects.DTOs;
using Services.Interfaces;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class CustomerManagementViewModel : BaseViewModel
{
    private readonly ICustomerService _customerService;
    private readonly IRoomService _roomService;
    private readonly IBookingService _bookingService;
    private readonly ICurrentUserService _currentUserService;

    private ObservableCollection<CustomerListItemDto> _customers = new();
    private ObservableCollection<RoomListItemDto> _availableRooms = new();
    private ObservableCollection<BookingSummaryDto> _bookings = new();
    private CustomerListItemDto? _selectedCustomer;
    private RoomListItemDto? _selectedRoom;
    private string _searchTerm = string.Empty;
    private string _customerName = string.Empty;
    private string _identityCard = string.Empty;
    private string _phoneNumber = string.Empty;
    private string _email = string.Empty;
    private string _address = string.Empty;
    private DateTime _checkInDate = DateTime.Today;
    private DateTime _checkOutDate = DateTime.Today.AddDays(1);
    private string _message = string.Empty;
    private bool _isBusy;

    public CustomerManagementViewModel(
        ICustomerService customerService,
        IRoomService roomService,
        IBookingService bookingService,
        ICurrentUserService currentUserService)
    {
        _customerService = customerService;
        _roomService = roomService;
        _bookingService = bookingService;
        _currentUserService = currentUserService;

        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SearchCustomersCommand = new AsyncRelayCommand(SearchCustomersAsync);
        CreateCustomerCommand = new AsyncRelayCommand(CreateCustomerAsync);
        CreateBookingCommand = new AsyncRelayCommand(CreateBookingAsync);
        RefreshRoomsCommand = new AsyncRelayCommand(RefreshRoomsAsync);
    }

    public override string Title => "Customer & Booking";

    public override string Description => "Manage guests, rooms, room map and reservations";

    public ObservableCollection<CustomerListItemDto> Customers
    {
        get => _customers;
        private set => SetProperty(ref _customers, value);
    }

    public ObservableCollection<RoomListItemDto> AvailableRooms
    {
        get => _availableRooms;
        private set => SetProperty(ref _availableRooms, value);
    }

    public ObservableCollection<BookingSummaryDto> Bookings
    {
        get => _bookings;
        private set => SetProperty(ref _bookings, value);
    }

    public CustomerListItemDto? SelectedCustomer
    {
        get => _selectedCustomer;
        set => SetProperty(ref _selectedCustomer, value);
    }

    public RoomListItemDto? SelectedRoom
    {
        get => _selectedRoom;
        set => SetProperty(ref _selectedRoom, value);
    }

    public string SearchTerm
    {
        get => _searchTerm;
        set => SetProperty(ref _searchTerm, value);
    }

    public string CustomerName
    {
        get => _customerName;
        set => SetProperty(ref _customerName, value);
    }

    public string IdentityCard
    {
        get => _identityCard;
        set => SetProperty(ref _identityCard, value);
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => SetProperty(ref _phoneNumber, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    public DateTime CheckInDate
    {
        get => _checkInDate;
        set => SetProperty(ref _checkInDate, value);
    }

    public DateTime CheckOutDate
    {
        get => _checkOutDate;
        set => SetProperty(ref _checkOutDate, value);
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

    public AsyncRelayCommand SearchCustomersCommand { get; }

    public AsyncRelayCommand CreateCustomerCommand { get; }

    public AsyncRelayCommand CreateBookingCommand { get; }

    public AsyncRelayCommand RefreshRoomsCommand { get; }

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
            var customerResult = await _customerService.GetCustomersAsync(SearchTerm);
            if (customerResult.IsSuccess)
            {
                Customers = new ObservableCollection<CustomerListItemDto>(customerResult.Data ?? new List<CustomerListItemDto>());
            }

            var bookingResult = await _bookingService.GetRecentBookingsAsync(10);
            if (bookingResult.IsSuccess)
            {
                Bookings = new ObservableCollection<BookingSummaryDto>(bookingResult.Data ?? new List<BookingSummaryDto>());
            }

            await RefreshRoomsAsync();
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

    private async Task SearchCustomersAsync()
    {
        await LoadAsync();
    }

    private async Task CreateCustomerAsync()
    {
        if (string.IsNullOrWhiteSpace(CustomerName))
        {
            Message = "Customer name is required.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _customerService.CreateCustomerAsync(new CreateCustomerRequestDto
            {
                FullName = CustomerName,
                IdentityCard = IdentityCard,
                PhoneNumber = PhoneNumber,
                Email = Email,
                Address = Address
            });

            Message = result.Message;
            if (result.IsSuccess)
            {
                CustomerName = string.Empty;
                IdentityCard = string.Empty;
                PhoneNumber = string.Empty;
                Email = string.Empty;
                Address = string.Empty;
                await LoadAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CreateBookingAsync()
    {
        if (SelectedCustomer is null || SelectedRoom is null)
        {
            Message = "Please select a customer and a room first.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _bookingService.CreateBookingAsync(new CreateBookingRequestDto
            {
                CustomerId = SelectedCustomer.CustomerId,
                CreatedByUserId = _currentUserService.User?.UserId ?? 0,
                CheckInDate = CheckInDate,
                CheckOutDate = CheckOutDate,
                RoomIds = new List<int> { SelectedRoom.RoomId },
                Note = $"Created from Customer/Room/Booking view"
            });

            Message = result.Message;
            if (result.IsSuccess)
            {
                await LoadAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshRoomsAsync()
    {
        var result = await _roomService.GetAvailableRoomsAsync(CheckInDate, CheckOutDate, SearchTerm);
        if (result.IsSuccess)
        {
            AvailableRooms = new ObservableCollection<RoomListItemDto>(result.Data ?? new List<RoomListItemDto>());
        }
    }
}
