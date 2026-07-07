using BusinessObjects.DTOs;

namespace Services.Interfaces;

public interface IPaymentService
{
    Task<ServiceResult<PaymentResultDto>> RecordPaymentAsync(
        PaymentRequestDto request,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<List<PaymentHistoryDto>>> GetPaymentHistoryAsync(
        int invoiceId,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default);
}
