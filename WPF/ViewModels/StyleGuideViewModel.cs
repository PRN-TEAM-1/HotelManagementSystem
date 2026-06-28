using System.Collections.ObjectModel;
using BusinessObjects.DTOs;
using WPF.Commands;
using WPF.Helpers;

namespace WPF.ViewModels;

public sealed class StyleGuideViewModel : BaseViewModel
{
    private readonly DialogService _dialogService;
    private string _searchKeyword = "Deluxe Ocean";
    private LookupItemDto? _selectedRoomStatus;
    private DateTime? _plannedCheckIn = DateTime.Today.AddDays(2);
    private string _notes = "Use this screen to preview shared input and table styles before wiring a real feature form.";

    public StyleGuideViewModel(DialogService dialogService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        RoomStatuses =
        [
            new LookupItemDto { Id = 1, Value = "available", DisplayName = "Available" },
            new LookupItemDto { Id = 2, Value = "reserved", DisplayName = "Reserved" },
            new LookupItemDto { Id = 3, Value = "occupied", DisplayName = "Occupied" },
            new LookupItemDto { Id = 4, Value = "cleaning", DisplayName = "Cleaning" }
        ];

        SelectedRoomStatus = RoomStatuses.FirstOrDefault();

        PreviewRows =
        [
            new StyleGuideBookingRow("A-101", "Nguyen Minh Chau", "Reserved", 3, 275.00m),
            new StyleGuideBookingRow("B-202", "Tran Gia Han", "Occupied", 2, 195.00m),
            new StyleGuideBookingRow("C-305", "Le Anh Khoa", "Cleaning", 1, 115.00m)
        ];

        ShowInfoDialogCommand = new RelayCommand(() => _dialogService.ShowInfo("Shared dialogs are ready for info and success messages.", "DialogService"));
        ShowWarningDialogCommand = new RelayCommand(() => _dialogService.ShowWarning("Use warnings for destructive or high-attention actions.", "DialogService"));
        ShowConfirmDialogCommand = new RelayCommand(ShowConfirmDialog);
        ResetPreviewCommand = new RelayCommand(ResetPreview);
    }

    public override string Title => "Style Guide";

    public override string Description =>
        "Preview the shared WPF buttons, text inputs and data tables introduced by CORE-004 so later screens stay visually consistent.";

    public RelayCommand ShowInfoDialogCommand { get; }

    public RelayCommand ShowWarningDialogCommand { get; }

    public RelayCommand ShowConfirmDialogCommand { get; }

    public RelayCommand ResetPreviewCommand { get; }

    public ObservableCollection<LookupItemDto> RoomStatuses { get; }

    public ObservableCollection<StyleGuideBookingRow> PreviewRows { get; }

    public string SearchKeyword
    {
        get => _searchKeyword;
        set => SetProperty(ref _searchKeyword, value);
    }

    public LookupItemDto? SelectedRoomStatus
    {
        get => _selectedRoomStatus;
        set => SetProperty(ref _selectedRoomStatus, value);
    }

    public DateTime? PlannedCheckIn
    {
        get => _plannedCheckIn;
        set => SetProperty(ref _plannedCheckIn, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    private void ShowConfirmDialog()
    {
        var confirmed = _dialogService.Confirm(
            "DialogService confirmation is working. Would you like to keep this shared flow for future forms?",
            "Confirm Preview");

        if (confirmed)
        {
            _dialogService.ShowInfo("Confirmed. Future screens can reuse the same confirmation pattern.", "DialogService");
        }
    }

    private void ResetPreview()
    {
        SearchKeyword = string.Empty;
        SelectedRoomStatus = RoomStatuses.FirstOrDefault();
        PlannedCheckIn = DateTime.Today.AddDays(1);
        Notes = string.Empty;
    }
}

public sealed record StyleGuideBookingRow(
    string RoomNumber,
    string GuestName,
    string Status,
    int Nights,
    decimal TotalAmount);
