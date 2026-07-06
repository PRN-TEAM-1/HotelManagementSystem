using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Repositories.Implements;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implements;

public sealed class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;

    public InvoiceService(IInvoiceRepository? invoiceRepository = null)
    {
        _invoiceRepository = invoiceRepository ?? new InvoiceRepository();
    }

    public async Task<ServiceResult<List<InvoiceCandidateDto>>> GetInvoiceCandidatesAsync(
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default)
    {
        var authorizationResult = EnsureCanManageInvoices<List<InvoiceCandidateDto>>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        try
        {
            var candidates = await _invoiceRepository.GetInvoiceCandidatesAsync(cancellationToken);
            return ServiceResult<List<InvoiceCandidateDto>>.Success(candidates);
        }
        catch
        {
            return ServiceResult<List<InvoiceCandidateDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<List<InvoiceSummaryDto>>> GetInvoicesAsync(
        CurrentSessionDto? currentUser,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var authorizationResult = EnsureCanManageInvoices<List<InvoiceSummaryDto>>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        try
        {
            var invoices = await _invoiceRepository.GetInvoiceSummariesAsync(searchTerm, cancellationToken);
            return ServiceResult<List<InvoiceSummaryDto>>.Success(invoices);
        }
        catch
        {
            return ServiceResult<List<InvoiceSummaryDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<InvoiceDetailDto>> GetInvoiceDetailAsync(
        int invoiceId,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default)
    {
        var authorizationResult = EnsureCanManageInvoices<InvoiceDetailDto>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        if (invoiceId <= 0)
        {
            return ServiceResult<InvoiceDetailDto>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            var invoice = await _invoiceRepository.GetInvoiceDetailAsync(invoiceId, cancellationToken);
            if (invoice is null)
            {
                return ServiceResult<InvoiceDetailDto>.Failure(ErrorMessages.NotFound);
            }

            return ServiceResult<InvoiceDetailDto>.Success(invoice);
        }
        catch
        {
            return ServiceResult<InvoiceDetailDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<InvoiceDetailDto>> CreateInvoiceAsync(
        CreateInvoiceRequestDto request,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var authorizationResult = EnsureCanManageInvoices<InvoiceDetailDto>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        if (request.BookingId <= 0)
        {
            return ServiceResult<InvoiceDetailDto>.Failure(ErrorMessages.InvalidInput);
        }

        var validationErrors = ValidateCreateRequest(request);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<InvoiceDetailDto>.Failure(
                ErrorMessages.ValidationFailed,
                validationErrors.ToArray());
        }

        try
        {
            if (await _invoiceRepository.HasInvoiceForBookingAsync(request.BookingId, cancellationToken))
            {
                return ServiceResult<InvoiceDetailDto>.Failure(ErrorMessages.DuplicateInvoice);
            }

            if (!await _invoiceRepository.BookingExistsAsync(request.BookingId, cancellationToken))
            {
                return ServiceResult<InvoiceDetailDto>.Failure(ErrorMessages.NotFound);
            }

            var candidate = await _invoiceRepository.GetInvoiceCandidateByBookingIdAsync(
                request.BookingId,
                cancellationToken);

            if (candidate is null)
            {
                return ServiceResult<InvoiceDetailDto>.Failure(
                    ErrorMessages.BusinessRuleViolation,
                    "Invoice can only be created when every booking room is CheckedOut or Cancelled.");
            }

            var totalAmount = candidate.RoomAmount
                + candidate.ServiceAmount
                + request.TaxAmount
                - request.DiscountAmount;

            if (totalAmount < 0)
            {
                return ServiceResult<InvoiceDetailDto>.Failure(
                    ErrorMessages.ValidationFailed,
                    "Discount cannot exceed room, service and tax amount.");
            }

            var now = DateTime.Now;
            var invoice = new Invoice
            {
                BookingId = request.BookingId,
                CreatedByUserId = currentUser!.UserId,
                RoomAmount = candidate.RoomAmount,
                ServiceAmount = candidate.ServiceAmount,
                DiscountAmount = request.DiscountAmount,
                TaxAmount = request.TaxAmount,
                TotalAmount = totalAmount,
                PaidAmount = 0m,
                RemainingAmount = totalAmount,
                CreateDate = now,
                Status = InvoiceStatus.Unpaid,
                Note = NormalizeOptional(request.Note),
                CreatedAt = now,
                UpdatedAt = now
            };

            var createdInvoice = await _invoiceRepository.AddAsync(invoice, cancellationToken);
            var detail = await _invoiceRepository.GetInvoiceDetailAsync(
                createdInvoice.InvoiceId,
                cancellationToken);

            if (detail is null)
            {
                return ServiceResult<InvoiceDetailDto>.Failure(ErrorMessages.SystemError);
            }

            return ServiceResult<InvoiceDetailDto>.Success(detail, "Invoice created successfully.");
        }
        catch
        {
            return ServiceResult<InvoiceDetailDto>.Failure(ErrorMessages.SystemError);
        }
    }

    private static ServiceResult<T>? EnsureCanManageInvoices<T>(CurrentSessionDto? currentUser)
    {
        if (currentUser is null || !currentUser.IsAuthenticated)
        {
            return ServiceResult<T>.Failure(ErrorMessages.Unauthorized);
        }

        if (currentUser.RoleName is not (RoleName.Admin or RoleName.Receptionist))
        {
            return ServiceResult<T>.Failure(ErrorMessages.Forbidden);
        }

        return null;
    }

    private static List<string> ValidateCreateRequest(CreateInvoiceRequestDto request)
    {
        var errors = new List<string>();

        if (request.DiscountAmount < 0)
        {
            errors.Add("Discount amount cannot be negative.");
        }

        if (request.TaxAmount < 0)
        {
            errors.Add("Tax amount cannot be negative.");
        }

        return errors;
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
