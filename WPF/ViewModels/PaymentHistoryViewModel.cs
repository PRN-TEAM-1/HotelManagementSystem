using BusinessObjects.DTOs;
using Services.Interfaces;

namespace WPF.ViewModels;

public sealed class PaymentHistoryViewModel : BaseViewModel
{
    private readonly IPaymentService _paymentService;
    private readonly ICurrentUserService _currentUserService;

    private List<PaymentHistoryDto> _payments = new();
    private string _errorMessage = string.Empty;
    private bool _isBusy;
    private int _invoiceId;

    public PaymentHistoryViewModel(
        IPaymentService paymentService,
        ICurrentUserService currentUserService)
    {
        _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public override string Title => "Payment History";

    public override string Description => "Recorded payment transactions for the selected invoice";

    public int InvoiceId
    {
        get => _invoiceId;
        private set => SetProperty(ref _invoiceId, value);
    }

    public List<PaymentHistoryDto> Payments
    {
        get => _payments;
        private set
        {
            if (SetProperty(ref _payments, value))
            {
                OnPropertyChanged(nameof(EmptyMessage));
            }
        }
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
                OnPropertyChanged(nameof(EmptyMessage));
            }
        }
    }

    public string EmptyMessage
    {
        get
        {
            if (IsBusy || Payments.Count > 0)
            {
                return string.Empty;
            }

            return InvoiceId <= 0
                ? "Select an invoice to view payment history."
                : "No payments have been recorded for this invoice.";
        }
    }

    public async Task LoadAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        InvoiceId = invoiceId;
        ErrorMessage = string.Empty;

        if (invoiceId <= 0)
        {
            Payments = new();
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _paymentService.GetPaymentHistoryAsync(
                invoiceId,
                _currentUserService.User,
                cancellationToken);

            if (result.IsFailure)
            {
                ErrorMessage = result.Errors.FirstOrDefault() ?? result.Message;
                Payments = new();
                return;
            }

            Payments = result.Data ?? new();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading payment history: {ex.Message}";
            Payments = new();
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void Clear()
    {
        InvoiceId = 0;
        Payments = new();
        ErrorMessage = string.Empty;
    }
}
