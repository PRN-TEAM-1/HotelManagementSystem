using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using Repositories.Implements;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implements;

public sealed class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IPaymentRepository? paymentRepository = null)
    {
        _paymentRepository = paymentRepository ?? new PaymentRepository();
    }

    public async Task<ServiceResult<PaymentResultDto>> RecordPaymentAsync(
        PaymentRequestDto request,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var authorizationResult = EnsureCanManagePayments<PaymentResultDto>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        var validationErrors = ValidatePaymentRequest(request);
        if (validationErrors.Count > 0)
        {
            return ServiceResult<PaymentResultDto>.Failure(
                ErrorMessages.ValidationFailed,
                validationErrors.ToArray());
        }

        var sanitizedRequest = new PaymentRequestDto
        {
            InvoiceId = request.InvoiceId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            TransactionCode = NormalizeOptional(request.TransactionCode),
            Note = NormalizeOptional(request.Note)
        };

        try
        {
            var result = await _paymentRepository.RecordPaymentAsync(
                sanitizedRequest,
                currentUser!.UserId,
                cancellationToken);

            return ServiceResult<PaymentResultDto>.Success(result, "Payment recorded successfully.");
        }
        catch (KeyNotFoundException ex)
        {
            return ServiceResult<PaymentResultDto>.Failure(ErrorMessages.NotFound, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return ServiceResult<PaymentResultDto>.Failure(ErrorMessages.BusinessRuleViolation, ex.Message);
        }
        catch
        {
            return ServiceResult<PaymentResultDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<List<PaymentHistoryDto>>> GetPaymentHistoryAsync(
        int invoiceId,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default)
    {
        var authorizationResult = EnsureCanManagePayments<List<PaymentHistoryDto>>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        if (invoiceId <= 0)
        {
            return ServiceResult<List<PaymentHistoryDto>>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            var history = await _paymentRepository.GetPaymentHistoryAsync(invoiceId, cancellationToken);
            return ServiceResult<List<PaymentHistoryDto>>.Success(history);
        }
        catch
        {
            return ServiceResult<List<PaymentHistoryDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    private static ServiceResult<T>? EnsureCanManagePayments<T>(CurrentSessionDto? currentUser)
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

    private static List<string> ValidatePaymentRequest(PaymentRequestDto request)
    {
        var errors = new List<string>();

        if (request.InvoiceId <= 0)
        {
            errors.Add("Invoice is required.");
        }

        if (request.Amount <= 0m)
        {
            errors.Add(ErrorMessages.InvalidAmount);
        }

        if (!Enum.IsDefined(request.PaymentMethod))
        {
            errors.Add("Payment method is not supported.");
        }

        return errors;
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
