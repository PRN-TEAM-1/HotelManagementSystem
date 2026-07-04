using BusinessObjects.DTOs;
using Services.Interfaces;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class CheckoutViewModel : BaseViewModel
{
    private readonly ICheckoutService _checkoutService;
    private readonly ICurrentUserService _currentUserService;

    private List<CheckoutCandidateDto> _checkoutCandidates = new();
    private CheckoutCandidateDto? _selectedCandidate;
    private string _checkoutNote = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public CheckoutViewModel(ICheckoutService checkoutService, ICurrentUserService currentUserService)
    {
        _checkoutService = checkoutService ?? throw new ArgumentNullException(nameof(checkoutService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

        LoadCheckoutCandidatesCommand = new AsyncRelayCommand(LoadCheckoutCandidatesAsync, CanExecuteLoad);
        CheckoutCommand = new AsyncRelayCommand(CheckoutAsync, CanCheckout);
        ClearMessagesCommand = new RelayCommand(ClearMessages);
    }

    public override string Title => "Check-Out";

    public override string Description => "Check-out guests and process room cleaning";

    public List<CheckoutCandidateDto> CheckoutCandidates
    {
        get => _checkoutCandidates;
        private set => SetProperty(ref _checkoutCandidates, value);
    }

    public CheckoutCandidateDto? SelectedCandidate
    {
        get => _selectedCandidate;
        set
        {
            if (SetProperty(ref _selectedCandidate, value))
            {
                CheckoutNote = string.Empty;
                CheckoutCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string CheckoutNote
    {
        get => _checkoutNote;
        set
        {
            if (SetProperty(ref _checkoutNote, value))
            {
                CheckoutCommand.RaiseCanExecuteChanged();
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
                LoadCheckoutCandidatesCommand.RaiseCanExecuteChanged();
                CheckoutCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanEdit => !IsBusy;

    public AsyncRelayCommand LoadCheckoutCandidatesCommand { get; }

    public AsyncRelayCommand CheckoutCommand { get; }

    public RelayCommand ClearMessagesCommand { get; }

    public override async Task InitializeAsync()
    {
        await LoadCheckoutCandidatesAsync();
    }

    private bool CanExecuteLoad()
    {
        return !IsBusy;
    }

    private bool CanCheckout()
    {
        return !IsBusy && SelectedCandidate != null;
    }

    private async Task LoadCheckoutCandidatesAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var result = await _checkoutService.GetCheckoutCandidatesAsync();

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                CheckoutCandidates = new();
                return;
            }

            CheckoutCandidates = result.Data ?? new();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading checkout candidates: {ex.Message}";
            CheckoutCandidates = new();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CheckoutAsync()
    {
        if (SelectedCandidate is null)
        {
            ErrorMessage = "Please select a booking to check-out";
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

            var request = new CheckoutRequestDto
            {
                BookingDetailId = SelectedCandidate.BookingDetailId,
                CheckOutNote = CheckoutNote.Trim()
            };

            var result = await _checkoutService.CheckoutAsync(request, currentUser.UserId);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                return;
            }

            SuccessMessage = $"Check-out successful for room {SelectedCandidate.RoomNumber}. Room is now marked for cleaning.";

            // Reload the list
            await LoadCheckoutCandidatesAsync();
            SelectedCandidate = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error during check-out: {ex.Message}";
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
