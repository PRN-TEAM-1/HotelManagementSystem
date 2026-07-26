using BusinessObjects.DTOs;

namespace Services.Interfaces;

public interface IRoomTypeService
{
    Task<ServiceResult<List<RoomTypeListItemDto>>> GetRoomTypesAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<RoomTypeListItemDto>> CreateRoomTypeAsync(
        CreateRoomTypeRequestDto request,
        CurrentSessionDto? currentUser = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<RoomTypeListItemDto>> UpdateRoomTypeAsync(
        UpdateRoomTypeRequestDto request,
        CurrentSessionDto? currentUser = null,
        CancellationToken cancellationToken = default);
}
