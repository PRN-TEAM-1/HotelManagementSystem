using BusinessObjects.DTOs;
using DataAccessObjects.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly PaymentDao _paymentDao;

    public PaymentRepository(PaymentDao? paymentDao = null)
    {
        _paymentDao = paymentDao ?? new PaymentDao();
    }

    public Task<PaymentResultDto> RecordPaymentAsync(
        PaymentRequestDto request,
        int receivedByUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        return _paymentDao.RecordPaymentAsync(request, receivedByUserId, cancellationToken);
    }

    public Task<List<PaymentHistoryDto>> GetPaymentHistoryAsync(
        int invoiceId,
        CancellationToken cancellationToken = default)
    {
        return _paymentDao.GetPaymentHistoryAsync(invoiceId, cancellationToken);
    }
}
