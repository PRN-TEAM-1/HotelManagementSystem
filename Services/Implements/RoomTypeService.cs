using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Repositories.Implements;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implements;

public sealed class RoomTypeService : IRoomTypeService
{
    private readonly IRoomTypeRepository _roomTypeRepository;

    public RoomTypeService(IRoomTypeRepository? roomTypeRepository = null)
    {
        _roomTypeRepository = roomTypeRepository ?? new RoomTypeRepository();
    }

    public async Task<ServiceResult<List<RoomTypeListItemDto>>> GetRoomTypesAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var roomTypes = await _roomTypeRepository.GetRoomTypesAsync(searchTerm, cancellationToken);
            return ServiceResult<List<RoomTypeListItemDto>>.Success(roomTypes.Select(MapToListItem).ToList());
        }
        catch
        {
            return ServiceResult<List<RoomTypeListItemDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<RoomTypeListItemDto>> CreateRoomTypeAsync(
        CreateRoomTypeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var typeName = NormalizeRequired(request.TypeName);
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return ServiceResult<RoomTypeListItemDto>.Failure(ErrorMessages.ValidationFailed, "Room type name is required.");
        }

        if (await _roomTypeRepository.TypeNameExistsAsync(typeName, cancellationToken: cancellationToken))
        {
            return ServiceResult<RoomTypeListItemDto>.Failure(ErrorMessages.DuplicateRecord, "Room type already exists.");
        }

        if (!Enum.TryParse<RoomTypeStatus>(request.Status, true, out var status))
        {
            status = RoomTypeStatus.Active;
        }

        try
        {
            var roomType = new RoomType
            {
                TypeName = typeName,
                Description = NormalizeOptional(request.Description),
                BasePrice = request.BasePrice,
                Capacity = request.Capacity,
                Status = status,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var created = await _roomTypeRepository.AddAsync(roomType, cancellationToken);
            return ServiceResult<RoomTypeListItemDto>.Success(MapToListItem(created), "Room type created successfully.");
        }
        catch
        {
            return ServiceResult<RoomTypeListItemDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<RoomTypeListItemDto>> UpdateRoomTypeAsync(
        UpdateRoomTypeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.RoomTypeId <= 0)
        {
            return ServiceResult<RoomTypeListItemDto>.Failure(ErrorMessages.InvalidInput);
        }

        var typeName = NormalizeRequired(request.TypeName);
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return ServiceResult<RoomTypeListItemDto>.Failure(ErrorMessages.ValidationFailed, "Room type name is required.");
        }

        if (await _roomTypeRepository.TypeNameExistsAsync(typeName, request.RoomTypeId, cancellationToken))
        {
            return ServiceResult<RoomTypeListItemDto>.Failure(ErrorMessages.DuplicateRecord, "Room type already exists.");
        }

        if (!Enum.TryParse<RoomTypeStatus>(request.Status, true, out var status))
        {
            status = RoomTypeStatus.Active;
        }

        try
        {
            var roomType = new RoomType
            {
                RoomTypeId = request.RoomTypeId,
                TypeName = typeName,
                Description = NormalizeOptional(request.Description),
                BasePrice = request.BasePrice,
                Capacity = request.Capacity,
                Status = status,
                UpdatedAt = DateTime.Now
            };

            var updated = await _roomTypeRepository.UpdateAsync(roomType, cancellationToken);
            return updated is null
                ? ServiceResult<RoomTypeListItemDto>.Failure(ErrorMessages.NotFound)
                : ServiceResult<RoomTypeListItemDto>.Success(MapToListItem(updated), "Room type updated successfully.");
        }
        catch
        {
            return ServiceResult<RoomTypeListItemDto>.Failure(ErrorMessages.SystemError);
        }
    }

    private static RoomTypeListItemDto MapToListItem(RoomType roomType)
    {
        return new RoomTypeListItemDto
        {
            RoomTypeId = roomType.RoomTypeId,
            TypeName = roomType.TypeName,
            Description = roomType.Description,
            BasePrice = roomType.BasePrice,
            Capacity = roomType.Capacity,
            Status = roomType.Status.ToString()
        };
    }

    private static string NormalizeRequired(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
