namespace WPF.ViewModels;

public sealed class BillingViewModel : BaseViewModel
{
    public BillingViewModel(InvoiceViewModel invoiceFlow)
    {
        InvoiceFlow = invoiceFlow ?? throw new ArgumentNullException(nameof(invoiceFlow));
    }

    public override string Title => "Billing";

    public override string Description => "Create invoices, record payments, and review billing history";

    public InvoiceViewModel InvoiceFlow { get; }

    public override Task InitializeAsync()
    {
        return InvoiceFlow.InitializeAsync();
    }
}
