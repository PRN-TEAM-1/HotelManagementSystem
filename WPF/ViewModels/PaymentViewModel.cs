using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using Services.Interfaces;
using WPF.Commands;

namespace WPF.ViewModels;

public sealed class PaymentViewModel : BaseViewModel
{
    private readonly IPaymentService _paymentService;
    private readonly ICurrentUserService _currentUserService;

    private InvoiceDetailDto? _invoice;
    private PaymentMethod _selectedPaymentMethod = PaymentMethod.Cash;
    private string _amountText = "0";
    private string _transactionCode = string.Empty;
    private string _note = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isBusy;

    public PaymentViewModel(
        IPaymentService paymentService,
        ICurrentUserService currentUserService)
    {
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));

        PaymentMethods = Enum.GetValues<PaymentMethod>().ToList();
        RecordPaymentCommand = new AsyncRelayCommand(RecordPaymentAsync, CanRecordPayment);
        UseRemainingAmountCommand = new RelayCommand(UseRemainingAmount, CanUseRemainingAmount);
        ResetFormCommand = new RelayCommand(ResetForm, CanResetForm);
        ClearMessagesCommand = new RelayCommand(ClearMessages);
    }

    public event EventHandler<PaymentResultDto>? PaymentRecorded;

    public override string Title => "Record Payment";

    public override string Description => "Receive partial or full payment for the selected invoice";

    public InvoiceDetailDto? Invoice
    {
        get => _invoice;
        private set
        {
            if (SetProperty(ref _invoice, value))
            {
                OnPropertyChanged(nameof(HasInvoice));
                OnPropertyChanged(nameof(InvoiceLabel));
                OnPropertyChanged(nameof(RemainingAmount));
                OnPropertyChanged(nameof(CanEdit));
                RaiseCommandStates();
            }
        }
    }

    public bool HasInvoice => Invoice is not null;

    public string InvoiceLabel => Invoice is null
        ? "No invoice selected"
        : $"Invoice #{Invoice.InvoiceId} - {Invoice.CustomerName}";

    public decimal RemainingAmount => Invoice?.RemainingAmount ?? 0m;

    public List<PaymentMethod> PaymentMethods { get; }

    public PaymentMethod SelectedPaymentMethod
    {
        get => _selectedPaymentMethod;
        set
        {
            if (SetProperty(ref _selectedPaymentMethod, value))
            {
                RecordPaymentCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string AmountText
    {
        get => _amountText;
        set
        {
            if (SetProperty(ref _amountText, value))
            {
                RecordPaymentCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string TransactionCode
    {
        get => _transactionCode;
        set => SetProperty(ref _transactionCode, value);
    }

    public string Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(CanEdit));
                RaiseCommandStates();
            }
        }
    }

    public bool CanEdit => !IsBusy
        && Invoice is not null
        && Invoice.RemainingAmount > 0m
        && !string.Equals(Invoice.Status, InvoiceStatus.Paid.ToString(), StringComparison.OrdinalIgnoreCase)
        && !string.Equals(Invoice.Status, InvoiceStatus.Cancelled.ToString(), StringComparison.OrdinalIgnoreCase);

    public AsyncRelayCommand RecordPaymentCommand { get; }

    public RelayCommand UseRemainingAmountCommand { get; }

    public RelayCommand ResetFormCommand { get; }

    public RelayCommand ClearMessagesCommand { get; }

    public void SetInvoice(InvoiceDetailDto? invoice)
    {
        Invoice = invoice;
        SelectedPaymentMethod = PaymentMethod.Cash;
        AmountText = invoice is null || invoice.RemainingAmount <= 0m
            ? "0"
            : invoice.RemainingAmount.ToString("0.##");
        TransactionCode = string.Empty;
        Note = string.Empty;
        ErrorMessage = string.Empty;
        RaiseCommandStates();
    }

    public void ClearInvoice()
    {
        SetInvoice(null);
    }

    private bool CanRecordPayment()
    {
        return CanEdit
            && Enum.IsDefined(SelectedPaymentMethod)
            && decimal.TryParse(AmountText, out var amount)
            && amount > 0m
            && amount <= RemainingAmount;
    }

    private bool CanUseRemainingAmount()
    {
        return CanEdit && RemainingAmount > 0m;
    }

    private bool CanResetForm()
    {
        return !IsBusy && Invoice is not null;
    }

    private async Task RecordPaymentAsync()
    {
        if (Invoice is null)
        {
            ErrorMessage = "Please select an invoice first.";
            return;
        }

        if (!decimal.TryParse(AmountText, out var amount) || amount <= 0m)
        {
            ErrorMessage = "Payment amount must be a positive number.";
            return;
        }

        if (amount > Invoice.RemainingAmount)
        {
            ErrorMessage = "Payment amount cannot exceed the remaining invoice amount.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var request = new PaymentRequestDto
            {
                InvoiceId = Invoice.InvoiceId,
                Amount = amount,
                PaymentMethod = SelectedPaymentMethod,
                TransactionCode = TransactionCode,
                Note = Note
            };

            var result = await _paymentService.RecordPaymentAsync(
                request,
                _currentUserService.User);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                return;
            }

            if (result.Data is not null)
            {
                PaymentRecorded?.Invoke(this, result.Data);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error recording payment: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UseRemainingAmount()
    {
        AmountText = RemainingAmount.ToString("0.##");
    }

    private void ResetForm()
    {
        if (Invoice is null)
        {
            return;
        }

        SelectedPaymentMethod = PaymentMethod.Cash;
        AmountText = Invoice.RemainingAmount > 0m
            ? Invoice.RemainingAmount.ToString("0.##")
            : "0";
        TransactionCode = string.Empty;
        Note = string.Empty;
        ErrorMessage = string.Empty;
    }

    private void ClearMessages()
    {
        ErrorMessage = string.Empty;
    }

    private void RaiseCommandStates()
    {
        RecordPaymentCommand.RaiseCanExecuteChanged();
        UseRemainingAmountCommand.RaiseCanExecuteChanged();
        ResetFormCommand.RaiseCanExecuteChanged();
    }
}
