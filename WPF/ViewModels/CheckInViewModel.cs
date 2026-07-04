using BusinessObjects.DTOs;
using Services.Interfaces;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class CheckInViewModel : BaseViewModel
{
    private readonly ICheckInService _checkInService;
    private readonly ICurrentUserService _currentUserService;

    private List<CheckInCandidateDto> _checkInCandidates = new();
    private CheckInCandidateDto? _selectedCandidate;
    private string _checkInNote = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;

    public CheckInViewModel(ICheckInService checkInService, ICurrentUserService currentUserService)
    {
        _checkInService = checkInService ?? throw new ArgumentNullException(nameof(checkInService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

        LoadCheckInCandidatesCommand = new AsyncRelayCommand(LoadCheckInCandidatesAsync, CanExecuteLoad);
        CheckInCommand = new AsyncRelayCommand(CheckInAsync, CanCheckIn);
        ClearMessagesCommand = new RelayCommand(ClearMessages);
    }

    public override string Title => "Check-In";

    public override string Description => "Check-in guests and update room status";

    public List<CheckInCandidateDto> CheckInCandidates
    {
        get => _checkInCandidates;
        private set => SetProperty(ref _checkInCandidates, value);
    }

    public CheckInCandidateDto? SelectedCandidate
    {
        get => _selectedCandidate;
        set
        {
            if (SetProperty(ref _selectedCandidate, value))
            {
                CheckInNote = string.Empty;
                CheckInCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string CheckInNote
    {
        get => _checkInNote;
        set
        {
            if (SetProperty(ref _checkInNote, value))
            {
                CheckInCommand.RaiseCanExecuteChanged();
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
                LoadCheckInCandidatesCommand.RaiseCanExecuteChanged();
                CheckInCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanEdit => !IsBusy;

    public AsyncRelayCommand LoadCheckInCandidatesCommand { get; }

    public AsyncRelayCommand CheckInCommand { get; }

    public RelayCommand ClearMessagesCommand { get; }

    public override async Task InitializeAsync()
    {
        await LoadCheckInCandidatesAsync();
    }

    private bool CanExecuteLoad()
    {
        return !IsBusy;
    }

    private bool CanCheckIn()
    {
        return !IsBusy && SelectedCandidate != null;
    }

    private async Task LoadCheckInCandidatesAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var result = await _checkInService.GetCheckInCandidatesAsync();

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                CheckInCandidates = new();
                return;
            }

            CheckInCandidates = result.Data ?? new();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading check-in candidates: {ex.Message}";
            CheckInCandidates = new();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task CheckInAsync()
    {
        if (SelectedCandidate is null)
        {
            ErrorMessage = "Please select a booking to check-in";
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

            var request = new CheckInRequestDto
            {
                BookingDetailId = SelectedCandidate.BookingDetailId,
                CheckInNote = CheckInNote.Trim()
            };

            var result = await _checkInService.CheckInAsync(request, currentUser.UserId);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                return;
            }

            SuccessMessage = $"Check-in successful for room {SelectedCandidate.RoomNumber}";

            // Reload the list
            await LoadCheckInCandidatesAsync();
            SelectedCandidate = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error during check-in: {ex.Message}";
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
