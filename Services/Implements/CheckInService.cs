using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Repositories.Implements;
using Repositories.Interfaces;
using Services.Interfaces;

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
        catch (Exception ex)
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
        catch (Exception ex)
        {
            return ServiceResult<CheckInCandidateDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<CheckRecordDto>> CheckInAsync(CheckInRequestDto request, int currentUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.BookingDetailId <= 0)
        {
            return ServiceResult<CheckRecordDto>.Failure(ErrorMessages.InvalidInput);
        }

        if (currentUserId <= 0)
        {
            return ServiceResult<CheckRecordDto>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
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

            if (!await _bookingOperationRepository.IsRoomOperationalAsync(bookingDetail.RoomId, cancellationToken))
            {
                return ServiceResult<CheckRecordDto>.Failure("Room is not operational.");
            }

            if (room.Status != RoomOperationalStatus.Available)
            {
                return ServiceResult<CheckRecordDto>.Failure("Room must be Available to check-in.");
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
                CheckInByUserId = currentUserId,
                ActualCheckInDate = DateTime.Now,
                CheckInNote = request.CheckInNote,
                Status = CheckRecordStatus.CheckedIn,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var createdCheckRecord = await _checkRecordRepository.AddAsync(checkRecord, cancellationToken);

            // Update booking detail status to CheckedIn
            await _bookingOperationRepository.UpdateBookingDetailStatusAsync(
                request.BookingDetailId,
                BookingDetailStatus.CheckedIn,
                cancellationToken);

            // Update room status to Occupied (Wait, there is no Occupied in enum, using Available as operational status)
            await _bookingOperationRepository.UpdateRoomStatusAsync(
                bookingDetail.RoomId,
                RoomOperationalStatus.Available,
                cancellationToken);

            var dto = MapToCheckRecordDto(createdCheckRecord);
            return ServiceResult<CheckRecordDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return ServiceResult<CheckRecordDto>.Failure(ErrorMessages.SystemError);
        }
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
        catch (Exception ex)
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
}
