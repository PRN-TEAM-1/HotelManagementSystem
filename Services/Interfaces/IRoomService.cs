using BusinessObjects.DTOs;

namespace Services.Interfaces;

public interface IRoomService
{
    Task<ServiceResult<List<RoomListItemDto>>> GetRoomsAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<List<RoomListItemDto>>> GetAvailableRoomsAsync(
        DateTime checkInDate,
        DateTime checkOutDate,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<List<RoomMapItemDto>>> GetRoomMapAsync(
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<RoomListItemDto>> CreateRoomAsync(
        CreateRoomRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<RoomListItemDto>> UpdateRoomAsync(
        UpdateRoomRequestDto request,
        CancellationToken cancellationToken = default);
}
