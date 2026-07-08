using BusinessObjects.DTOs;
using BusinessObjects.Entities;

namespace Repositories.Interfaces;

public interface IInvoiceRepository
{
    Task<List<InvoiceCandidateDto>> GetInvoiceCandidatesAsync(
        CancellationToken cancellationToken = default);

    Task<InvoiceCandidateDto?> GetInvoiceCandidateByBookingIdAsync(
        int bookingId,
        CancellationToken cancellationToken = default);

    Task<bool> BookingExistsAsync(int bookingId, CancellationToken cancellationToken = default);

    Task<Invoice?> GetByIdAsync(int invoiceId, CancellationToken cancellationToken = default);

    Task<Invoice?> GetByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default);

    Task<bool> HasInvoiceForBookingAsync(int bookingId, CancellationToken cancellationToken = default);

    Task<Invoice> AddAsync(Invoice invoice, CancellationToken cancellationToken = default);

    Task<List<InvoiceSummaryDto>> GetInvoiceSummariesAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<InvoiceDetailDto?> GetInvoiceDetailAsync(
        int invoiceId,
        CancellationToken cancellationToken = default);

    Task<InvoiceDetailDto?> GetInvoiceDetailByBookingIdAsync(
        int bookingId,
        CancellationToken cancellationToken = default);
}
