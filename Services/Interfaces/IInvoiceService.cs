using BusinessObjects.DTOs;

namespace Services.Interfaces;

public interface IInvoiceService
{
    Task<ServiceResult<List<InvoiceCandidateDto>>> GetInvoiceCandidatesAsync(
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<List<InvoiceSummaryDto>>> GetInvoicesAsync(
        CurrentSessionDto? currentUser,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<InvoiceDetailDto>> GetInvoiceDetailAsync(
        int invoiceId,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<InvoiceDetailDto>> CreateInvoiceAsync(
        CreateInvoiceRequestDto request,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default);
}
