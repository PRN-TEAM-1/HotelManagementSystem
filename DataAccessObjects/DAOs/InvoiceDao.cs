using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class InvoiceDao
{
    public async Task<Invoice?> GetByIdAsync(
        int invoiceId,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(invoice => invoice.InvoiceId == invoiceId, cancellationToken);
    }

    public async Task<Invoice?> GetByBookingIdAsync(
        int bookingId,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(invoice => invoice.BookingId == bookingId, cancellationToken);
    }

    public async Task<bool> HasInvoiceForBookingAsync(
        int bookingId,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        return await context.Invoices
            .AsNoTracking()
            .AnyAsync(invoice => invoice.BookingId == bookingId, cancellationToken);
    }

    public async Task<Invoice> AddAsync(
        Invoice invoice,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        await using var context = DbContextFactory.CreateDbContext();

        invoice.CreatedAt = DateTime.Now;
        invoice.UpdatedAt = DateTime.Now;

        context.Invoices.Add(invoice);
        await context.SaveChangesAsync(cancellationToken);

        return invoice;
    }

    public async Task<List<InvoiceSummaryDto>> GetInvoiceSummariesAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var query = context.Invoices
            .AsNoTracking()
            .Join(
                context.Bookings.AsNoTracking(),
                invoice => invoice.BookingId,
                booking => booking.BookingId,
                (invoice, booking) => new { Invoice = invoice, Booking = booking })
            .Join(
                context.Customers.AsNoTracking(),
                row => row.Booking.CustomerId,
                customer => customer.CustomerId,
                (row, customer) => new { row.Invoice, Customer = customer })
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalized = searchTerm.Trim();
            var pattern = $"%{normalized}%";

            if (int.TryParse(normalized, out var numericTerm))
            {
                query = query.Where(row =>
                    row.Invoice.InvoiceId == numericTerm
                    || row.Invoice.BookingId == numericTerm
                    || EF.Functions.Like(row.Customer.FullName, pattern));
            }
            else
            {
                var hasStatusTerm = Enum.TryParse<InvoiceStatus>(
                    normalized,
                    ignoreCase: true,
                    out var statusTerm);

                query = query.Where(row =>
                    EF.Functions.Like(row.Customer.FullName, pattern)
                    || (hasStatusTerm && row.Invoice.Status == statusTerm)
                    || (row.Customer.PhoneNumber != null && EF.Functions.Like(row.Customer.PhoneNumber, pattern))
                    || (row.Customer.Email != null && EF.Functions.Like(row.Customer.Email, pattern)));
            }
        }

        return await query
            .OrderByDescending(row => row.Invoice.CreateDate)
            .ThenByDescending(row => row.Invoice.InvoiceId)
            .Select(row => new InvoiceSummaryDto
            {
                InvoiceId = row.Invoice.InvoiceId,
                BookingId = row.Invoice.BookingId,
                CustomerName = row.Customer.FullName,
                CreateDate = row.Invoice.CreateDate,
                RoomAmount = row.Invoice.RoomAmount,
                ServiceAmount = row.Invoice.ServiceAmount,
                DiscountAmount = row.Invoice.DiscountAmount,
                TaxAmount = row.Invoice.TaxAmount,
                TotalAmount = row.Invoice.TotalAmount,
                PaidAmount = row.Invoice.PaidAmount,
                RemainingAmount = row.Invoice.RemainingAmount,
                Status = row.Invoice.Status.ToString()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<InvoiceDetailDto?> GetInvoiceDetailAsync(
        int invoiceId,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var invoice = await context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(existingInvoice => existingInvoice.InvoiceId == invoiceId, cancellationToken);

        if (invoice is null)
        {
            return null;
        }

        return await BuildInvoiceDetailAsync(context, invoice, cancellationToken);
    }

    public async Task<InvoiceDetailDto?> GetInvoiceDetailByBookingIdAsync(
        int bookingId,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var invoice = await context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(existingInvoice => existingInvoice.BookingId == bookingId, cancellationToken);

        if (invoice is null)
        {
            return null;
        }

        return await BuildInvoiceDetailAsync(context, invoice, cancellationToken);
    }

    private static async Task<InvoiceDetailDto?> BuildInvoiceDetailAsync(
        HotelManagementContext context,
        Invoice invoice,
        CancellationToken cancellationToken)
    {
        var header = await context.Bookings
            .AsNoTracking()
            .Where(booking => booking.BookingId == invoice.BookingId)
            .Join(
                context.Customers.AsNoTracking(),
                booking => booking.CustomerId,
                customer => customer.CustomerId,
                (booking, customer) => new { Booking = booking, Customer = customer })
            .GroupJoin(
                context.Users.AsNoTracking(),
                row => invoice.CreatedByUserId,
                user => user.UserId,
                (row, users) => new { row.Booking, row.Customer, Users = users })
            .SelectMany(
                row => row.Users.DefaultIfEmpty(),
                (row, user) => new
                {
                    row.Booking,
                    row.Customer,
                    CreatedByUserName = user == null ? string.Empty : user.FullName
                })
            .FirstOrDefaultAsync(cancellationToken);

        if (header is null)
        {
            return null;
        }

        var roomLines = await context.BookingDetails
            .AsNoTracking()
            .Where(detail => detail.BookingId == invoice.BookingId)
            .Join(
                context.Rooms.AsNoTracking(),
                detail => detail.RoomId,
                room => room.RoomId,
                (detail, room) => new { Detail = detail, Room = room })
            .Join(
                context.RoomTypes.AsNoTracking(),
                row => row.Room.RoomTypeId,
                roomType => roomType.RoomTypeId,
                (row, roomType) => new InvoiceRoomLineDto
                {
                    BookingDetailId = row.Detail.BookingDetailId,
                    RoomNumber = row.Room.RoomNumber,
                    RoomType = roomType.TypeName,
                    CheckInDate = row.Detail.CheckInDate,
                    CheckOutDate = row.Detail.CheckOutDate,
                    NumberOfNights = row.Detail.NumberOfNights,
                    RoomPrice = row.Detail.RoomPrice,
                    RoomTotal = row.Detail.RoomTotal,
                    Status = row.Detail.Status.ToString()
                })
            .OrderBy(line => line.RoomNumber)
            .ToListAsync(cancellationToken);

        var bookingDetailIds = roomLines
            .Select(line => line.BookingDetailId)
            .ToArray();

        var serviceLines = await context.ServiceOrders
            .AsNoTracking()
            .Where(order =>
                bookingDetailIds.Contains(order.BookingDetailId)
                && order.Status == ServiceOrderStatus.Ordered)
            .Join(
                context.Services.AsNoTracking(),
                order => order.ServiceId,
                service => service.ServiceId,
                (order, service) => new { Order = order, Service = service })
            .Join(
                context.BookingDetails.AsNoTracking(),
                row => row.Order.BookingDetailId,
                detail => detail.BookingDetailId,
                (row, detail) => new { row.Order, row.Service, Detail = detail })
            .Join(
                context.Rooms.AsNoTracking(),
                row => row.Detail.RoomId,
                room => room.RoomId,
                (row, room) => new InvoiceServiceLineDto
                {
                    ServiceOrderId = row.Order.ServiceOrderId,
                    BookingDetailId = row.Order.BookingDetailId,
                    RoomNumber = room.RoomNumber,
                    ServiceName = row.Service.ServiceName,
                    Quantity = row.Order.Quantity,
                    UnitPrice = row.Order.UnitPrice,
                    TotalPrice = row.Order.TotalPrice,
                    OrderDate = row.Order.OrderDate,
                    Status = row.Order.Status.ToString()
                })
            .OrderBy(line => line.OrderDate)
            .ToListAsync(cancellationToken);

        var payments = await context.Payments
            .AsNoTracking()
            .Where(payment => payment.InvoiceId == invoice.InvoiceId)
            .OrderByDescending(payment => payment.PaymentDate)
            .Select(payment => new InvoicePaymentLineDto
            {
                PaymentId = payment.PaymentId,
                PaymentMethod = payment.PaymentMethod.ToString(),
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                TransactionCode = payment.TransactionCode,
                Status = payment.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return new InvoiceDetailDto
        {
            InvoiceId = invoice.InvoiceId,
            BookingId = invoice.BookingId,
            BookingDate = header.Booking.BookingDate,
            BookingStatus = header.Booking.Status.ToString(),
            CreatedByUserId = invoice.CreatedByUserId,
            CreatedByUserName = header.CreatedByUserName,
            CustomerName = header.Customer.FullName,
            CustomerPhone = header.Customer.PhoneNumber,
            CustomerEmail = header.Customer.Email,
            CustomerIdentityCard = header.Customer.IdentityCard,
            RoomAmount = invoice.RoomAmount,
            ServiceAmount = invoice.ServiceAmount,
            DiscountAmount = invoice.DiscountAmount,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            RemainingAmount = invoice.RemainingAmount,
            CreateDate = invoice.CreateDate,
            Status = invoice.Status.ToString(),
            Note = invoice.Note,
            RoomLines = roomLines,
            ServiceLines = serviceLines,
            Payments = payments
        };
    }
}
