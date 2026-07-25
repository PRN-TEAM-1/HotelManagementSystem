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
        UpdateCustomerCommand = new AsyncRelayCommand(UpdateCustomerAsync);
        CreateBookingCommand = new AsyncRelayCommand(CreateBookingAsync);
        CancelBookingCommand = new AsyncRelayCommand(CancelBookingAsync);
        MarkNoShowCommand = new AsyncRelayCommand(MarkNoShowAsync);
        RefreshRoomsCommand = new AsyncRelayCommand(RefreshRoomsAsync);
        ClearMessagesCommand = new RelayCommand(ClearMessages);
    }


    public override string Title => "Customer & Booking";

    public override string Description => "Manage guests, rooms, room map and reservations";

    public AsyncRelayCommand LoadCommand { get; }

    public AsyncRelayCommand SearchCustomersCommand { get; }

    public AsyncRelayCommand CreateCustomerCommand { get; }

    public AsyncRelayCommand UpdateCustomerCommand { get; }

    public AsyncRelayCommand CreateBookingCommand { get; }

    public AsyncRelayCommand CancelBookingCommand { get; }

    public AsyncRelayCommand MarkNoShowCommand { get; }


    public AsyncRelayCommand RefreshRoomsCommand { get; }


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

    private ObservableCollection<BookingSummaryDto> _selectedCustomerBookings = new();

    public ObservableCollection<BookingSummaryDto> SelectedCustomerBookings
    {
        get => _selectedCustomerBookings;
        private set => SetProperty(ref _selectedCustomerBookings, value);
    }

    public CustomerListItemDto? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            if (SetProperty(ref _selectedCustomer, value))
            {
                if (value != null)
                {
                    CustomerName = value.FullName;
                    IdentityCard = value.IdentityCard ?? string.Empty;
                    PhoneNumber = value.PhoneNumber ?? string.Empty;
                    Email = value.Email ?? string.Empty;
                    Address = value.Address ?? string.Empty;

                    _ = LoadSelectedCustomerBookingsAsync(value.CustomerId);
                }
                else
                {
                    CustomerName = string.Empty;
                    IdentityCard = string.Empty;
                    PhoneNumber = string.Empty;
                    Email = string.Empty;
                    Address = string.Empty;
                    SelectedCustomerBookings.Clear();
                }
            }
        }
    }



    private BookingSummaryDto? _selectedBooking;

    public BookingSummaryDto? SelectedBooking
    {
        get => _selectedBooking;
        set => SetProperty(ref _selectedBooking, value);
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

    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;

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

    public RelayCommand ClearMessagesCommand { get; }


    public override void OnNavigatedFrom()
    {
        base.OnNavigatedFrom();
        ClearMessages();
    }

    public override async Task InitializeAsync()

    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsBusy = true;
        ClearMessages();

        try
        {
            var customerResult = await _customerService.GetCustomersAsync(SearchTerm);
            if (customerResult.IsSuccess)
            {
                Customers = new ObservableCollection<CustomerListItemDto>(customerResult.Data ?? new List<CustomerListItemDto>());
            }

            var bookingResult = await _bookingService.GetRecentBookingsAsync(_currentUserService.User, 10);
            if (bookingResult.IsSuccess)
            {
                Bookings = new ObservableCollection<BookingSummaryDto>(bookingResult.Data ?? new List<BookingSummaryDto>());
            }

            await RefreshRoomsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
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
        ClearMessages();

        if (string.IsNullOrWhiteSpace(CustomerName))
        {
            ErrorMessage = "Please enter the Full Name.";
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

            if (result.IsSuccess)
            {
                SuccessMessage = result.Message;
                CustomerName = string.Empty;
                IdentityCard = string.Empty;
                PhoneNumber = string.Empty;
                Email = string.Empty;
                Address = string.Empty;
                await LoadAsync();
                if (result.Data != null)
                {
                    SelectedCustomer = Customers.FirstOrDefault(c => c.CustomerId == result.Data.CustomerId);
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


    private async Task CreateBookingAsync()
    {
        ClearMessages();

        if (SelectedCustomer is null || SelectedRoom is null)
        {
            ErrorMessage = "Please select a customer and a room first.";
            return;
        }

        if (!string.Equals(SelectedRoom.Status, "Available", StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage = $"Room {SelectedRoom.RoomNumber} is currently '{SelectedRoom.Status}' and cannot be booked. Only 'Available' rooms can be booked.";
            return;
        }

        var confirmMessage = $"Are you sure you want to create this booking?\n\n" +
                             $"• Guest Name: {SelectedCustomer.FullName}\n" +
                             $"• Identity Card (CCCD): {SelectedCustomer.IdentityCard}\n" +
                             $"• Room: {SelectedRoom.RoomNumber} ({SelectedRoom.RoomTypeName})\n" +
                             $"• Check-in: {CheckInDate:yyyy-MM-dd}\n" +
                             $"• Check-out: {CheckOutDate:yyyy-MM-dd}";

        var confirmResult = System.Windows.MessageBox.Show(
            confirmMessage,
            "Confirm Booking",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (confirmResult != System.Windows.MessageBoxResult.Yes)
        {
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
            }, _currentUserService.User);

            if (result.IsSuccess)
            {
                SuccessMessage = result.Message;
                await LoadAsync();
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

    private async Task UpdateCustomerAsync()
    {
        ClearMessages();

        if (SelectedCustomer is null)
        {
            ErrorMessage = "Please select a customer to update.";
            return;
        }

        if (string.IsNullOrWhiteSpace(CustomerName))
        {
            ErrorMessage = "Please enter the Full Name.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _customerService.UpdateCustomerAsync(new UpdateCustomerRequestDto
            {
                CustomerId = SelectedCustomer.CustomerId,
                FullName = CustomerName,
                IdentityCard = IdentityCard,
                PhoneNumber = PhoneNumber,
                Email = Email,
                Address = Address
            });

            if (result.IsSuccess)
            {
                SuccessMessage = result.Message;
                await LoadAsync();
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


    private async Task CancelBookingAsync()
    {
        ClearMessages();

        if (SelectedBooking is null)
        {
            ErrorMessage = "Please select a booking to cancel.";
            return;
        }

        var confirmResult = System.Windows.MessageBox.Show(
            $"Are you sure you want to cancel booking #{SelectedBooking.BookingId}?",
            "Confirm Cancellation",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (confirmResult != System.Windows.MessageBoxResult.Yes)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _bookingService.CancelBookingAsync(SelectedBooking.BookingId, _currentUserService.User);
            if (result.IsSuccess)
            {
                SuccessMessage = result.Message;
                await LoadAsync();
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

    private async Task MarkNoShowAsync()
    {
        ClearMessages();

        if (SelectedBooking is null)
        {
            ErrorMessage = "Please select a booking to mark as No-Show.";
            return;
        }

        var confirmResult = System.Windows.MessageBox.Show(
            $"Are you sure you want to mark booking #{SelectedBooking.BookingId} as No-Show?",
            "Confirm No-Show",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (confirmResult != System.Windows.MessageBoxResult.Yes)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _bookingService.MarkNoShowAsync(SelectedBooking.BookingId);
            if (result.IsSuccess)
            {
                SuccessMessage = result.Message;
                await LoadAsync();
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



    private async Task RefreshRoomsAsync()
    {
        var result = await _roomService.GetAvailableRoomsAsync(CheckInDate, CheckOutDate, SearchTerm);
        if (result.IsSuccess)
        {
            AvailableRooms = new ObservableCollection<RoomListItemDto>(result.Data ?? new List<RoomListItemDto>());
        }
    }

    private async Task LoadSelectedCustomerBookingsAsync(int customerId)
    {
        var result = await _customerService.GetCustomerBookingsAsync(customerId);
        if (result.IsSuccess)
        {
            SelectedCustomerBookings = new ObservableCollection<BookingSummaryDto>(result.Data ?? new List<BookingSummaryDto>());
        }
    }

    private void ClearMessages()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }
}


