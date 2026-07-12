using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class PaymentDao
{
    public async Task<PaymentResultDto> RecordPaymentAsync(
        PaymentRequestDto request,
        int receivedByUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using var context = DbContextFactory.CreateDbContext();
        await using var transaction = await context.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var invoice = await context.Invoices
            .FirstOrDefaultAsync(
                existingInvoice => existingInvoice.InvoiceId == request.InvoiceId,
                cancellationToken);

        if (invoice is null)
        {
            throw new KeyNotFoundException("Invoice was not found.");
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled invoices cannot receive payments.");
        }

        var actualPaidAmount = await context.Payments
            .Where(payment =>
                payment.InvoiceId == invoice.InvoiceId
                && payment.Status == PaymentStatus.Success)
            .SumAsync(payment => (decimal?)payment.Amount, cancellationToken) ?? 0m;

        var actualRemainingAmount = invoice.TotalAmount - actualPaidAmount;
        if (invoice.Status == InvoiceStatus.Paid || actualRemainingAmount <= 0m)
        {
            throw new InvalidOperationException("This invoice has already been paid in full.");
        }

        if (request.Amount > actualRemainingAmount)
        {
            throw new InvalidOperationException("Payment amount cannot exceed the remaining invoice amount.");
        }

        var now = DateTime.Now;
        var payment = new Payment
        {
            InvoiceId = invoice.InvoiceId,
            ReceivedByUserId = receivedByUserId,
            PaymentMethod = request.PaymentMethod,
            Amount = request.Amount,
            PaymentDate = now,
            TransactionCode = request.TransactionCode,
            Status = PaymentStatus.Success,
            Note = request.Note,
            CreatedAt = now,
            UpdatedAt = now
        };

        var newPaidAmount = actualPaidAmount + request.Amount;
        var newRemainingAmount = invoice.TotalAmount - newPaidAmount;

        invoice.PaidAmount = newPaidAmount;
        invoice.RemainingAmount = newRemainingAmount;
        invoice.Status = newRemainingAmount == 0m
            ? InvoiceStatus.Paid
            : InvoiceStatus.PartiallyPaid;
        invoice.UpdatedAt = now;

        context.Payments.Add(payment);

        var booking = await context.Bookings
            .FirstOrDefaultAsync(
                existingBooking => existingBooking.BookingId == invoice.BookingId,
                cancellationToken);

        if (invoice.Status == InvoiceStatus.Paid && booking is not null)
        {
            var bookingDetails = await context.BookingDetails
                .Where(detail => detail.BookingId == booking.BookingId)
                .Select(detail => detail.Status)
                .ToListAsync(cancellationToken);

            var isReadyToComplete = bookingDetails.Count > 0
                && bookingDetails.All(status =>
                    status is BookingDetailStatus.CheckedOut or BookingDetailStatus.Cancelled);

            if (isReadyToComplete
                && booking.Status is not (BookingStatus.Completed or BookingStatus.Cancelled or BookingStatus.NoShow))
            {
                booking.Status = BookingStatus.Completed;
                booking.UpdatedAt = now;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new PaymentResultDto
        {
            PaymentId = payment.PaymentId,
            InvoiceId = invoice.InvoiceId,
            BookingId = invoice.BookingId,
            PaidAmount = invoice.PaidAmount,
            RemainingAmount = invoice.RemainingAmount,
            InvoiceStatus = invoice.Status.ToString(),
            BookingStatus = booking?.Status.ToString()
        };
    }

    public async Task<List<PaymentHistoryDto>> GetPaymentHistoryAsync(
        int invoiceId,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.Payments
            .AsNoTracking()
            .Where(payment => payment.InvoiceId == invoiceId)
            .GroupJoin(
                context.Users.AsNoTracking(),
                payment => payment.ReceivedByUserId,
                user => user.UserId,
                (payment, users) => new { Payment = payment, Users = users })
            .SelectMany(
                row => row.Users.DefaultIfEmpty(),
                (row, user) => new PaymentHistoryDto
                {
                    PaymentId = row.Payment.PaymentId,
                    InvoiceId = row.Payment.InvoiceId,
                    ReceivedByUserId = row.Payment.ReceivedByUserId,
                    ReceivedByUserName = user == null ? string.Empty : user.FullName,
                    PaymentMethod = row.Payment.PaymentMethod.ToString(),
                    Amount = row.Payment.Amount,
                    PaymentDate = row.Payment.PaymentDate,
                    TransactionCode = row.Payment.TransactionCode,
                    Status = row.Payment.Status.ToString(),
                    Note = row.Payment.Note
                })
            .OrderByDescending(payment => payment.PaymentDate)
            .ThenByDescending(payment => payment.PaymentId)
            .ToListAsync(cancellationToken);
    }
}
