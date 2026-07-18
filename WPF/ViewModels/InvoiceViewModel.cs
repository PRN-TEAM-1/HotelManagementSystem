using BusinessObjects.DTOs;
using Services.Interfaces;
using WPF.Commands;
using WPF.Helpers;

namespace WPF.ViewModels;

public sealed class InvoiceViewModel : BaseViewModel
{
    private readonly IInvoiceService _invoiceService;
    private readonly IPaymentService _paymentService;
    private readonly ICurrentUserService _currentUserService;
    private readonly DialogService _dialogService;

    private List<InvoiceCandidateDto> _allInvoiceCandidates = new();
    private List<InvoiceCandidateDto> _invoiceCandidates = new();
    private List<InvoiceSummaryDto> _invoices = new();
    private InvoiceCandidateDto? _selectedCandidate;
    private InvoiceSummaryDto? _selectedInvoice;
    private InvoiceDetailViewModel _invoiceDetail = new();
    private PaymentViewModel _paymentView;
    private PaymentHistoryViewModel _paymentHistory;
    private string _candidateSearchTerm = string.Empty;
    private string _invoiceSearchTerm = string.Empty;
    private string _discountAmountText = "0";
    private string _taxAmountText = "0";
    private string _invoiceNote = string.Empty;
    private string _errorMessage = string.Empty;
    private string _successMessage = string.Empty;
    private bool _isBusy;
    private bool _isInitialized;

    public InvoiceViewModel(
        IInvoiceService invoiceService,
        IPaymentService paymentService,
        ICurrentUserService currentUserService,
        DialogService dialogService)
    {
        _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _paymentView = new PaymentViewModel(_paymentService, _currentUserService);
        _paymentHistory = new PaymentHistoryViewModel(_paymentService, _currentUserService);
        _paymentView.PaymentRecorded += OnPaymentRecorded;

        RefreshCommand = new AsyncRelayCommand(RefreshAsync, CanExecuteWhenIdle);
        SearchCandidatesCommand = new RelayCommand(SearchCandidates, CanExecuteWhenIdle);
        ClearCandidateSearchCommand = new RelayCommand(ClearCandidateSearch, CanClearCandidateSearch);
        SearchInvoicesCommand = new AsyncRelayCommand(SearchInvoicesAsync, CanExecuteWhenIdle);
        ClearInvoiceSearchCommand = new AsyncRelayCommand(ClearInvoiceSearchAsync, CanClearInvoiceSearch);
        CreateInvoiceCommand = new AsyncRelayCommand(CreateInvoiceAsync, CanCreateInvoice);
        ViewInvoiceDetailCommand = new AsyncRelayCommand(ViewInvoiceDetailAsync, CanViewInvoiceDetail);
        ClearMessagesCommand = new RelayCommand(ClearMessages);
    }

    public override string Title => "Billing";

    public override string Description => "Create invoices, record payments, and review billing history";

    public List<InvoiceCandidateDto> InvoiceCandidates
    {
        get => _invoiceCandidates;
        private set
        {
            if (SetProperty(ref _invoiceCandidates, value))
            {
                OnPropertyChanged(nameof(CandidateEmptyMessage));
            }
        }
    }

    public List<InvoiceSummaryDto> Invoices
    {
        get => _invoices;
        private set
        {
            if (SetProperty(ref _invoices, value))
            {
                OnPropertyChanged(nameof(InvoiceEmptyMessage));
            }
        }
    }

    public InvoiceCandidateDto? SelectedCandidate
    {
        get => _selectedCandidate;
        set
        {
            if (SetProperty(ref _selectedCandidate, value))
            {
                DiscountAmountText = "0";
                TaxAmountText = "0";
                InvoiceNote = string.Empty;
                CreateInvoiceCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public InvoiceSummaryDto? SelectedInvoice
    {
        get => _selectedInvoice;
        set
        {
            if (SetProperty(ref _selectedInvoice, value))
            {
                ViewInvoiceDetailCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public InvoiceDetailViewModel InvoiceDetail
    {
        get => _invoiceDetail;
        private set => SetProperty(ref _invoiceDetail, value);
    }

    public PaymentViewModel PaymentView
    {
        get => _paymentView;
        private set => SetProperty(ref _paymentView, value);
    }

    public PaymentHistoryViewModel PaymentHistory
    {
        get => _paymentHistory;
        private set => SetProperty(ref _paymentHistory, value);
    }

    public string CandidateSearchTerm
    {
        get => _candidateSearchTerm;
        set
        {
            if (SetProperty(ref _candidateSearchTerm, value))
            {
                ApplyCandidateFilter();
                SearchCandidatesCommand.RaiseCanExecuteChanged();
                ClearCandidateSearchCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string InvoiceSearchTerm
    {
        get => _invoiceSearchTerm;
        set
        {
            if (SetProperty(ref _invoiceSearchTerm, value))
            {
                SearchInvoicesCommand.RaiseCanExecuteChanged();
                ClearInvoiceSearchCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string DiscountAmountText
    {
        get => _discountAmountText;
        set
        {
            if (SetProperty(ref _discountAmountText, value))
            {
                OnPropertyChanged(nameof(PreviewTotalAmount));
                CreateInvoiceCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string TaxAmountText
    {
        get => _taxAmountText;
        set
        {
            if (SetProperty(ref _taxAmountText, value))
            {
                OnPropertyChanged(nameof(PreviewTotalAmount));
                CreateInvoiceCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string InvoiceNote
    {
        get => _invoiceNote;
        set => SetProperty(ref _invoiceNote, value);
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
                OnPropertyChanged(nameof(CandidateEmptyMessage));
                OnPropertyChanged(nameof(InvoiceEmptyMessage));
                RaiseCommandStates();
            }
        }
    }

    public bool CanEdit => !IsBusy;

    public decimal PreviewTotalAmount
    {
        get
        {
            if (SelectedCandidate is null)
            {
                return 0m;
            }

            var discount = TryParseMoney(DiscountAmountText, out var discountAmount)
                ? discountAmount
                : 0m;
            var tax = TryParseMoney(TaxAmountText, out var taxAmount)
                ? taxAmount
                : 0m;

            return SelectedCandidate.RoomAmount + SelectedCandidate.ServiceAmount + tax - discount;
        }
    }

    public string CandidateEmptyMessage
    {
        get
        {
            if (IsBusy || InvoiceCandidates.Count > 0)
            {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace(CandidateSearchTerm)
                ? "No checked-out bookings are ready for invoice creation."
                : "No checked-out bookings match the current search.";
        }
    }

    public string InvoiceEmptyMessage
    {
        get
        {
            if (IsBusy || Invoices.Count > 0)
            {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace(InvoiceSearchTerm)
                ? "No invoices have been created yet."
                : "No invoices match the current search.";
        }
    }

    public AsyncRelayCommand RefreshCommand { get; }

    public RelayCommand SearchCandidatesCommand { get; }

    public RelayCommand ClearCandidateSearchCommand { get; }

    public AsyncRelayCommand SearchInvoicesCommand { get; }

    public AsyncRelayCommand ClearInvoiceSearchCommand { get; }

    public AsyncRelayCommand CreateInvoiceCommand { get; }

    public AsyncRelayCommand ViewInvoiceDetailCommand { get; }

    public RelayCommand ClearMessagesCommand { get; }

    public override async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        await RefreshAsync();
        _isInitialized = true;
    }

    private bool CanExecuteWhenIdle()
    {
        return !IsBusy;
    }

    private bool CanClearInvoiceSearch()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(InvoiceSearchTerm);
    }

    private bool CanClearCandidateSearch()
    {
        return !IsBusy && !string.IsNullOrWhiteSpace(CandidateSearchTerm);
    }

    private bool CanCreateInvoice()
    {
        return !IsBusy
            && SelectedCandidate is not null
            && TryParseMoney(DiscountAmountText, out var discount)
            && TryParseMoney(TaxAmountText, out var tax)
            && discount >= 0m
            && tax >= 0m
            && PreviewTotalAmount >= 0m;
    }

    private bool CanViewInvoiceDetail()
    {
        return !IsBusy && SelectedInvoice is not null;
    }

    private async Task RefreshAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var candidateResult = await _invoiceService.GetInvoiceCandidatesAsync(_currentUserService.User);
            if (candidateResult.IsFailure)
            {
                ErrorMessage = candidateResult.Errors.FirstOrDefault() ?? candidateResult.Message;
                SetCandidateSource(new());
            }
            else
            {
                SetCandidateSource(candidateResult.Data ?? new());
            }

            var invoiceResult = await _invoiceService.GetInvoicesAsync(
                _currentUserService.User,
                InvoiceSearchTerm);

            if (invoiceResult.IsFailure)
            {
                ErrorMessage = invoiceResult.Errors.FirstOrDefault() ?? invoiceResult.Message;
                Invoices = new();
            }
            else
            {
                Invoices = invoiceResult.Data ?? new();
            }

            SelectedCandidate = null;
            SelectedInvoice = null;
            ClearSelectedInvoiceDetail();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading invoices: {ex.Message}";
            SetCandidateSource(new());
            Invoices = new();
            ClearSelectedInvoiceDetail();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SearchInvoicesAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var invoiceResult = await _invoiceService.GetInvoicesAsync(
                _currentUserService.User,
                InvoiceSearchTerm);

            if (invoiceResult.IsFailure)
            {
                ErrorMessage = invoiceResult.Errors.FirstOrDefault() ?? invoiceResult.Message;
                Invoices = new();
                SelectedInvoice = null;
                ClearSelectedInvoiceDetail();
                return;
            }

            Invoices = invoiceResult.Data ?? new();
            SelectedInvoice = null;
            ClearSelectedInvoiceDetail();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error searching invoices: {ex.Message}";
            Invoices = new();
            ClearSelectedInvoiceDetail();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ClearInvoiceSearchAsync()
    {
        InvoiceSearchTerm = string.Empty;
        await SearchInvoicesAsync();
    }

    private void SearchCandidates()
    {
        ApplyCandidateFilter();
    }

    private void ClearCandidateSearch()
    {
        CandidateSearchTerm = string.Empty;
        ApplyCandidateFilter();
    }

    private async Task CreateInvoiceAsync()
    {
        if (SelectedCandidate is null)
        {
            ErrorMessage = "Please select a checked-out booking first.";
            return;
        }

        if (!TryParseMoney(DiscountAmountText, out var discountAmount)
            || !TryParseMoney(TaxAmountText, out var taxAmount))
        {
            ErrorMessage = "Discount and tax must be valid numbers.";
            return;
        }

        var confirmMessage = $"Create invoice for booking #{SelectedCandidate.BookingId} with total {PreviewTotalAmount:C}?";
        if (!_dialogService.Confirm(confirmMessage, "Create invoice"))
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            var request = new CreateInvoiceRequestDto
            {
                BookingId = SelectedCandidate.BookingId,
                DiscountAmount = discountAmount,
                TaxAmount = taxAmount,
                Note = InvoiceNote
            };

            var result = await _invoiceService.CreateInvoiceAsync(request, _currentUserService.User);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                return;
            }

            SuccessMessage = $"Invoice #{result.Data?.InvoiceId} created successfully.";
            await ReloadAfterCreateAsync(result.Data?.InvoiceId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error creating invoice: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ReloadAfterCreateAsync(int? createdInvoiceId)
    {
        var candidateResult = await _invoiceService.GetInvoiceCandidatesAsync(_currentUserService.User);
        SetCandidateSource(candidateResult.Data ?? new());
        SelectedCandidate = null;

        var invoiceResult = await _invoiceService.GetInvoicesAsync(
            _currentUserService.User,
            InvoiceSearchTerm);
        Invoices = invoiceResult.Data ?? new();

        if (!createdInvoiceId.HasValue)
        {
            ClearSelectedInvoiceDetail();
            return;
        }

        SelectedInvoice = Invoices.FirstOrDefault(invoice => invoice.InvoiceId == createdInvoiceId.Value);
        if (SelectedInvoice is null)
        {
            InvoiceSearchTerm = string.Empty;

            invoiceResult = await _invoiceService.GetInvoicesAsync(_currentUserService.User);
            Invoices = invoiceResult.Data ?? new();
            SelectedInvoice = Invoices.FirstOrDefault(invoice => invoice.InvoiceId == createdInvoiceId.Value);
        }

        if (SelectedInvoice is not null)
        {
            await LoadInvoiceDetailAsync(SelectedInvoice.InvoiceId);
        }
    }

    private async Task ViewInvoiceDetailAsync()
    {
        if (SelectedInvoice is null)
        {
            ErrorMessage = "Please select an invoice first.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            await LoadInvoiceDetailAsync(SelectedInvoice.InvoiceId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading invoice detail: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadInvoiceDetailAsync(int invoiceId)
    {
        var result = await _invoiceService.GetInvoiceDetailAsync(
            invoiceId,
            _currentUserService.User);

        if (result.IsFailure)
        {
            ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
            ClearSelectedInvoiceDetail();
            return;
        }

        InvoiceDetail = new InvoiceDetailViewModel(result.Data);
        PaymentView.SetInvoice(result.Data);

        if (result.Data is null)
        {
            PaymentHistory.Clear();
            return;
        }

        await PaymentHistory.LoadAsync(result.Data.InvoiceId);
    }

    private async void OnPaymentRecorded(object? sender, PaymentResultDto result)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        try
        {
            await ReloadAfterPaymentAsync(result.InvoiceId);
            SuccessMessage = $"Payment #{result.PaymentId} recorded successfully. Remaining amount: {result.RemainingAmount:C}.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Payment was recorded, but invoice refresh failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ReloadAfterPaymentAsync(int invoiceId)
    {
        var invoiceResult = await _invoiceService.GetInvoicesAsync(
            _currentUserService.User,
            InvoiceSearchTerm);

        Invoices = invoiceResult.Data ?? new();
        SelectedInvoice = Invoices.FirstOrDefault(invoice => invoice.InvoiceId == invoiceId);
        if (SelectedInvoice is null)
        {
            InvoiceSearchTerm = string.Empty;

            invoiceResult = await _invoiceService.GetInvoicesAsync(_currentUserService.User);
            Invoices = invoiceResult.Data ?? new();
            SelectedInvoice = Invoices.FirstOrDefault(invoice => invoice.InvoiceId == invoiceId);
        }

        await LoadInvoiceDetailAsync(invoiceId);
    }

    private void SetCandidateSource(List<InvoiceCandidateDto> candidates)
    {
        _allInvoiceCandidates = candidates;
        ApplyCandidateFilter();
    }

    private void ApplyCandidateFilter()
    {
        var normalizedTerm = CandidateSearchTerm.Trim();
        var filteredCandidates = string.IsNullOrWhiteSpace(normalizedTerm)
            ? _allInvoiceCandidates.ToList()
            : _allInvoiceCandidates
                .Where(candidate => MatchesCandidateSearch(candidate, normalizedTerm))
                .ToList();

        if (SelectedCandidate is not null
            && filteredCandidates.All(candidate => candidate.BookingId != SelectedCandidate.BookingId))
        {
            SelectedCandidate = null;
        }

        InvoiceCandidates = filteredCandidates;
    }

    private static bool MatchesCandidateSearch(InvoiceCandidateDto candidate, string searchTerm)
    {
        return candidate.BookingId.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            || candidate.CustomerName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            || candidate.BookingStatus.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }

    private void ClearSelectedInvoiceDetail()
    {
        InvoiceDetail = new InvoiceDetailViewModel();
        PaymentView.ClearInvoice();
        PaymentHistory.Clear();
    }

    private void ClearMessages()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    private void RaiseCommandStates()
    {
        RefreshCommand.RaiseCanExecuteChanged();
        SearchCandidatesCommand.RaiseCanExecuteChanged();
        ClearCandidateSearchCommand.RaiseCanExecuteChanged();
        SearchInvoicesCommand.RaiseCanExecuteChanged();
        ClearInvoiceSearchCommand.RaiseCanExecuteChanged();
        CreateInvoiceCommand.RaiseCanExecuteChanged();
        ViewInvoiceDetailCommand.RaiseCanExecuteChanged();
    }

    private static bool TryParseMoney(string value, out decimal amount)
    {
        return MoneyInputParser.TryParse(value, out amount);
    }
}
