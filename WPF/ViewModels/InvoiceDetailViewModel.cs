using BusinessObjects.DTOs;

namespace WPF.ViewModels;

public sealed class InvoiceDetailViewModel : BaseViewModel
{
    public InvoiceDetailViewModel(InvoiceDetailDto? invoice = null)
    {
        Invoice = invoice;
    }

    public override string Title => "Invoice Detail";

    public override string Description => "Booking, room, service and payment summary";

    public InvoiceDetailDto? Invoice { get; }

    public bool HasInvoice => Invoice is not null;

    public string EmptyMessage => "Select an invoice to view its full detail.";
}
