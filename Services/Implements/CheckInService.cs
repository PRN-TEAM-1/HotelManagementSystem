using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Repositories.Implements;
using Repositories.Interfaces;
using Services.Interfaces;
using DataAccessObjects;
using Microsoft.EntityFrameworkCore;


namespace Services.Implements;

public sealed class CheckInService : ICheckInService
{
    private readonly ICheckInQueryRepository _checkInQueryRepository;
    private readonly ICheckRecordRepository _checkRecordRepository;
    private readonly IBookingOperationRepository _bookingOperationRepository;

    public CheckInService(
        ICheckInQueryRepository? checkInQueryRepository = null,
        ICheckRecordRepository? checkRecordRepository = null,
        IBookingOperationRepository? bookingOperationRepository = null)
    {
        _checkInQueryRepository = checkInQueryRepository ?? new CheckInQueryRepository();
        _checkRecordRepository = checkRecordRepository ?? new CheckRecordRepository();
        _bookingOperationRepository = bookingOperationRepository ?? new BookingOperationRepository();
    }

    public async Task<ServiceResult<List<CheckInCandidateDto>>> GetCheckInCandidatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var candidates = await _checkInQueryRepository.GetCandidatesForCheckInAsync(cancellationToken);
            return ServiceResult<List<CheckInCandidateDto>>.Success(candidates);
        }
        catch (Exception)
        {
            return ServiceResult<List<CheckInCandidateDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<CheckInCandidateDto>> GetCheckInCandidateByIdAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        if (bookingDetailId <= 0)
        {
            return ServiceResult<CheckInCandidateDto>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            var candidate = await _checkInQueryRepository.GetCheckInCandidateByBookingDetailIdAsync(bookingDetailId, cancellationToken);

            if (candidate is null)
            {
                return ServiceResult<CheckInCandidateDto>.Failure(ErrorMessages.NotFound);
            }

            return ServiceResult<CheckInCandidateDto>.Success(candidate);
        }
        catch (Exception)
        {
            return ServiceResult<CheckInCandidateDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<CheckRecordDto>> CheckInAsync(CheckInRequestDto request, CurrentSessionDto? currentUser, CancellationToken cancellationToken = default)
    {
        var authorizationResult = EnsureCanManageCheckIns<CheckRecordDto>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        ArgumentNullException.ThrowIfNull(request);

        if (request.BookingDetailId <= 0)
        {
            return ServiceResult<CheckRecordDto>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            // Validate user role
            await using (var dbContext = DbContextFactory.CreateDbContext())
            {
                var user = await dbContext.Users.Include(u => u.Role)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == currentUser!.UserId && u.Status == UserStatus.Active, cancellationToken);

                if (user is null)
                {
                    return ServiceResult<CheckRecordDto>.Failure(ErrorMessages.Unauthorized);
                }

                if (user.Role?.Name is not (RoleName.Admin or RoleName.Manager or RoleName.Receptionist))
                {
                    return ServiceResult<CheckRecordDto>.Failure(ErrorMessages.Forbidden);
                }
            }

            // Get booking detail
            var bookingDetail = await _bookingOperationRepository.GetBookingDetailByIdAsync(request.BookingDetailId, cancellationToken);
            if (bookingDetail is null)
            {
                return ServiceResult<CheckRecordDto>.Failure(ErrorMessages.NotFound);
            }

            // Check if booking detail is in Reserved status
            if (bookingDetail.Status != BookingDetailStatus.Reserved)
            {
                return ServiceResult<CheckRecordDto>.Failure(ErrorMessages.BusinessRuleViolation);
            }

            // Get room and check operational status
            var room = await _bookingOperationRepository.GetRoomByIdAsync(bookingDetail.RoomId, cancellationToken);
            if (room is null)
            {
                return ServiceResult<CheckRecordDto>.Failure(ErrorMessages.NotFound);
            }

            if (room.Status != RoomOperationalStatus.Available && room.Status != RoomOperationalStatus.Reserved)
            {
                return ServiceResult<CheckRecordDto>.Failure(GetRoomNotReadyMessage(room.RoomNumber, room.Status));
            }

            // Check if check record already exists
            var existingCheckRecord = await _checkRecordRepository.GetByBookingDetailIdAsync(request.BookingDetailId, cancellationToken);
            if (existingCheckRecord is not null)
            {
                return ServiceResult<CheckRecordDto>.Failure(ErrorMessages.BusinessRuleViolation);
            }

            // Create check record
            var checkRecord = new CheckRecord
            {
                BookingDetailId = request.BookingDetailId,
                CheckInByUserId = currentUser!.UserId,
                ActualCheckInDate = DateTime.Now,
                CheckInNote = request.CheckInNote,
                Status = CheckRecordStatus.CheckedIn,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var createdCheckRecord = await _bookingOperationRepository.CheckInWithTransactionAsync(
                checkRecord,
                BookingDetailStatus.CheckedIn,
                RoomOperationalStatus.Occupied,
                cancellationToken);

            var dto = MapToCheckRecordDto(createdCheckRecord);
            return ServiceResult<CheckRecordDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return ServiceResult<CheckRecordDto>.Failure(ErrorMessages.SystemError, ex.Message);
        }

    }

    private static string GetRoomNotReadyMessage(string roomNumber, RoomOperationalStatus status)
    {
        return status switch
        {
            RoomOperationalStatus.Cleaning =>
                $"Room {roomNumber} is still Cleaning. Please mark the room cleaned before check-in.",
            RoomOperationalStatus.Maintenance =>
                $"Room {roomNumber} is under Maintenance and cannot be checked in.",
            RoomOperationalStatus.Inactive =>
                $"Room {roomNumber} is Inactive and cannot be checked in.",
            RoomOperationalStatus.Occupied =>
                $"Room {roomNumber} is already Occupied and cannot be checked in.",
            _ =>
                $"Room {roomNumber} is currently {status} and cannot be checked in."
        };
    }

    public async Task<ServiceResult<CheckRecordDto>> GetCheckRecordAsync(int checkRecordId, CancellationToken cancellationToken = default)
    {
        if (checkRecordId <= 0)
        {
            return ServiceResult<CheckRecordDto>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            var checkRecord = await _checkRecordRepository.GetByIdAsync(checkRecordId, cancellationToken);

            if (checkRecord is null)
            {
                return ServiceResult<CheckRecordDto>.Failure(ErrorMessages.NotFound);
            }

            var dto = MapToCheckRecordDto(checkRecord);
            return ServiceResult<CheckRecordDto>.Success(dto);
        }
        catch (Exception)
        {
            return ServiceResult<CheckRecordDto>.Failure(ErrorMessages.SystemError);
        }
    }

    private CheckRecordDto MapToCheckRecordDto(CheckRecord checkRecord)
    {
        return new CheckRecordDto
        {
            CheckRecordId = checkRecord.CheckRecordId,
            BookingDetailId = checkRecord.BookingDetailId,
            CheckInByUserId = checkRecord.CheckInByUserId,
            CheckOutByUserId = checkRecord.CheckOutByUserId,
            ActualCheckInDate = checkRecord.ActualCheckInDate,
            ActualCheckOutDate = checkRecord.ActualCheckOutDate,
            CheckInNote = checkRecord.CheckInNote,
            CheckOutNote = checkRecord.CheckOutNote,
            Status = checkRecord.Status.ToString(),
            CreatedAt = checkRecord.CreatedAt,
            UpdatedAt = checkRecord.UpdatedAt
        };
    }

    private static ServiceResult<T>? EnsureCanManageCheckIns<T>(CurrentSessionDto? currentUser)
    {
        if (currentUser is null || !currentUser.IsAuthenticated)
        {
            return ServiceResult<T>.Failure(ErrorMessages.Unauthorized);
        }

        if (currentUser.RoleName is not (RoleName.Admin or RoleName.Receptionist or RoleName.Manager))
        {
            return ServiceResult<T>.Failure(ErrorMessages.Forbidden);
        }

        return null;
    }
}
