using BusinessObjects.Entities;
using BusinessObjects.Enums;

namespace Repositories.Interfaces;

public interface IBookingOperationRepository
{
    Task<BookingDetail?> GetBookingDetailByIdAsync(int bookingDetailId, CancellationToken cancellationToken = default);

    Task<Room?> GetRoomByIdAsync(int roomId, CancellationToken cancellationToken = default);

    Task UpdateBookingDetailStatusAsync(int bookingDetailId, BookingDetailStatus status, CancellationToken cancellationToken = default);

    Task UpdateRoomStatusAsync(int roomId, RoomOperationalStatus status, CancellationToken cancellationToken = default);

    Task<bool> IsRoomOperationalAsync(int roomId, CancellationToken cancellationToken = default);

    Task<CheckRecord> CheckInWithTransactionAsync(
        CheckRecord checkRecord,
        BookingDetailStatus newDetailStatus,
        RoomOperationalStatus newRoomStatus,
        CancellationToken cancellationToken = default);

    Task<CheckRecord> CheckoutWithTransactionAsync(
        CheckRecord checkRecord,
        BookingDetailStatus newDetailStatus,
        RoomOperationalStatus newRoomStatus,
        CancellationToken cancellationToken = default);
}
