using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Repositories.Implements;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implements;

public sealed class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IUserActivityService _userActivityService;

    public RoomService(
        IRoomRepository? roomRepository = null,
        IUserActivityService? userActivityService = null)
    {
        _roomRepository = roomRepository ?? new RoomRepository();
        _userActivityService = userActivityService ?? new UserActivityService();
    }

    public async Task<ServiceResult<List<RoomListItemDto>>> GetRoomsAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var rooms = await _roomRepository.GetRoomsAsync(searchTerm, cancellationToken);
            return ServiceResult<List<RoomListItemDto>>.Success(rooms.Select(MapToListItem).ToList());
        }
        catch
        {
            return ServiceResult<List<RoomListItemDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<List<RoomListItemDto>>> GetAvailableRoomsAsync(
        DateTime checkInDate,
        DateTime checkOutDate,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        if (checkOutDate <= checkInDate)
        {
            return ServiceResult<List<RoomListItemDto>>.Failure(ErrorMessages.InvalidDateRange);
        }

        try
        {
            var rooms = await _roomRepository.GetAvailableRoomsAsync(checkInDate, checkOutDate, searchTerm, cancellationToken);
            return ServiceResult<List<RoomListItemDto>>.Success(rooms.Select(MapToListItem).ToList());
        }
        catch
        {
            return ServiceResult<List<RoomListItemDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<List<RoomMapItemDto>>> GetRoomMapAsync(
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var rooms = await _roomRepository.GetRoomMapAsync(asOfDate, cancellationToken);
            return ServiceResult<List<RoomMapItemDto>>.Success(rooms.Select(room =>
            {
                var isOccupied = string.Equals(room.Status.ToString(), "Occupied", StringComparison.OrdinalIgnoreCase);


                return new RoomMapItemDto
                {
                    RoomId = room.RoomId,
                    RoomTypeId = room.RoomTypeId,
                    RoomNumber = room.RoomNumber,
                    Floor = room.Floor,
                    RoomTypeName = room.RoomType?.TypeName ?? string.Empty,
                    BasePrice = room.RoomType?.BasePrice ?? 0m,
                    Status = room.Status.ToString(),
                    IsOccupied = isOccupied,
                    OccupancyLabel = room.Status.ToString()
                };
            }).ToList());
        }
        catch
        {
            return ServiceResult<List<RoomMapItemDto>>.Failure(ErrorMessages.SystemError);
        }
    }


    public async Task<ServiceResult<RoomListItemDto>> CreateRoomAsync(
        CreateRoomRequestDto request,
        CurrentSessionDto? currentUser = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.RoomNumber))
        {
            return ServiceResult<RoomListItemDto>.Failure(ErrorMessages.ValidationFailed, "Room number is required.");
        }

        if (await _roomRepository.RoomNumberExistsAsync(request.RoomNumber, cancellationToken: cancellationToken))
        {
            return ServiceResult<RoomListItemDto>.Failure(ErrorMessages.DuplicateRecord, "Room number already exists.");
        }

        if (!Enum.TryParse<RoomOperationalStatus>(request.Status, true, out var status))
        {
            status = RoomOperationalStatus.Available;
        }

        try
        {
            var room = new Room
            {
                RoomTypeId = request.RoomTypeId,
                RoomNumber = request.RoomNumber.Trim(),
                Floor = request.Floor,
                Status = status,
                Note = request.Note,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var created = await _roomRepository.AddAsync(room, cancellationToken);
            await _userActivityService.RecordActivityAsync(
                currentUser,
                "RoomCreated",
                "Room",
                created.RoomId.ToString(),
                $"Created room {created.RoomNumber}.",
                cancellationToken: cancellationToken);

            return ServiceResult<RoomListItemDto>.Success(MapToListItem(created), "Room created successfully.");
        }
        catch
        {
            return ServiceResult<RoomListItemDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<RoomListItemDto>> UpdateRoomAsync(
        UpdateRoomRequestDto request,
        CurrentSessionDto? currentUser = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.RoomId <= 0)
        {
            return ServiceResult<RoomListItemDto>.Failure(ErrorMessages.InvalidInput);
        }

        if (string.IsNullOrWhiteSpace(request.RoomNumber))
        {
            return ServiceResult<RoomListItemDto>.Failure(ErrorMessages.ValidationFailed, "Room number is required.");
        }

        if (await _roomRepository.RoomNumberExistsAsync(request.RoomNumber, request.RoomId, cancellationToken))
        {
            return ServiceResult<RoomListItemDto>.Failure(ErrorMessages.DuplicateRecord, "Room number already exists.");
        }

        if (!Enum.TryParse<RoomOperationalStatus>(request.Status, true, out var status))
        {
            status = RoomOperationalStatus.Available;
        }

        try
        {
            var room = new Room
            {
                RoomId = request.RoomId,
                RoomTypeId = request.RoomTypeId,
                RoomNumber = request.RoomNumber.Trim(),
                Floor = request.Floor,
                Status = status,
                Note = request.Note,
                UpdatedAt = DateTime.Now
            };

            var updated = await _roomRepository.UpdateAsync(room, cancellationToken);
            if (updated is not null)
            {
                await _userActivityService.RecordActivityAsync(
                    currentUser,
                    "RoomUpdated",
                    "Room",
                    updated.RoomId.ToString(),
                    $"Updated room {updated.RoomNumber} status to {updated.Status}.",
                    cancellationToken: cancellationToken);
            }

            return updated is null
                ? ServiceResult<RoomListItemDto>.Failure(ErrorMessages.NotFound)
                : ServiceResult<RoomListItemDto>.Success(MapToListItem(updated), "Room updated successfully.");
        }
        catch
        {
        
            return ServiceResult<RoomListItemDto>.Failure(ErrorMessages.SystemError);
        }
    }

    private static RoomListItemDto MapToListItem(Room room)
    {
        return new RoomListItemDto
        {
            RoomId = room.RoomId,
            RoomTypeId = room.RoomTypeId,
            RoomNumber = room.RoomNumber,
            Floor = room.Floor,
            Status = room.Status.ToString(),
            RoomTypeName = room.RoomType?.TypeName ?? string.Empty,
            BasePrice = room.RoomType?.BasePrice ?? 0,
            Note = room.Note
        };
    }
}
