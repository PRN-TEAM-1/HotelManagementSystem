using BusinessObjects.DTOs;

namespace Repositories.Interfaces;

public interface IPaymentRepository
{
    Task<PaymentResultDto> RecordPaymentAsync(
        PaymentRequestDto request,
        int receivedByUserId,
        CancellationToken cancellationToken = default);

    Task<List<PaymentHistoryDto>> GetPaymentHistoryAsync(
        int invoiceId,
        CancellationToken cancellationToken = default);
}
