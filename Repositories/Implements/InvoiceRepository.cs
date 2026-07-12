using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using DataAccessObjects.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly InvoiceDao _invoiceDao;
    private readonly InvoiceCalculationDao _invoiceCalculationDao;

    public InvoiceRepository(
        InvoiceDao? invoiceDao = null,
        InvoiceCalculationDao? invoiceCalculationDao = null)
    {
        _invoiceDao = invoiceDao ?? new InvoiceDao();
        _invoiceCalculationDao = invoiceCalculationDao ?? new InvoiceCalculationDao();
    }

    public Task<List<InvoiceCandidateDto>> GetInvoiceCandidatesAsync(
        CancellationToken cancellationToken = default)
    {
        return _invoiceCalculationDao.GetInvoiceCandidatesAsync(cancellationToken);
    }

    public Task<InvoiceCandidateDto?> GetInvoiceCandidateByBookingIdAsync(
        int bookingId,
        CancellationToken cancellationToken = default)
    {
        return _invoiceCalculationDao.GetInvoiceCandidateByBookingIdAsync(bookingId, cancellationToken);
    }

    public Task<bool> BookingExistsAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        return _invoiceCalculationDao.BookingExistsAsync(bookingId, cancellationToken);
    }

    public Task<Invoice?> GetByIdAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        return _invoiceDao.GetByIdAsync(invoiceId, cancellationToken);
    }

    public Task<Invoice?> GetByBookingIdAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        return _invoiceDao.GetByBookingIdAsync(bookingId, cancellationToken);
    }

    public Task<bool> HasInvoiceForBookingAsync(int bookingId, CancellationToken cancellationToken = default)
    {
        return _invoiceDao.HasInvoiceForBookingAsync(bookingId, cancellationToken);
    }

    public Task<Invoice> AddAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        return _invoiceDao.AddAsync(invoice, cancellationToken);
    }

    public Task<List<InvoiceSummaryDto>> GetInvoiceSummariesAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        return _invoiceDao.GetInvoiceSummariesAsync(searchTerm, cancellationToken);
    }

    public Task<InvoiceDetailDto?> GetInvoiceDetailAsync(
        int invoiceId,
        CancellationToken cancellationToken = default)
    {
        return _invoiceDao.GetInvoiceDetailAsync(invoiceId, cancellationToken);
    }

    public Task<InvoiceDetailDto?> GetInvoiceDetailByBookingIdAsync(
        int bookingId,
        CancellationToken cancellationToken = default)
    {
        return _invoiceDao.GetInvoiceDetailByBookingIdAsync(bookingId, cancellationToken);
    }
}
