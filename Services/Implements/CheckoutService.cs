using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Enums;
using Repositories.Implements;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implements;

public sealed class CheckoutService : ICheckoutService
{
    private readonly ICheckoutQueryRepository _checkoutQueryRepository;
    private readonly ICheckRecordRepository _checkRecordRepository;
    private readonly IBookingOperationRepository _bookingOperationRepository;

    public CheckoutService(
        ICheckoutQueryRepository? checkoutQueryRepository = null,
        ICheckRecordRepository? checkRecordRepository = null,
        IBookingOperationRepository? bookingOperationRepository = null)
    {
        _checkoutQueryRepository = checkoutQueryRepository ?? new CheckoutQueryRepository();
        _checkRecordRepository = checkRecordRepository ?? new CheckRecordRepository();
        _bookingOperationRepository = bookingOperationRepository ?? new BookingOperationRepository();
    }

    public async Task<ServiceResult<List<CheckoutCandidateDto>>> GetCheckoutCandidatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var candidates = await _checkoutQueryRepository.GetCandidatesForCheckoutAsync(cancellationToken);
            return ServiceResult<List<CheckoutCandidateDto>>.Success(candidates);
        }
        catch (Exception ex)
        {
            return ServiceResult<List<CheckoutCandidateDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<CheckoutCandidateDto>> GetCheckoutCandidateByIdAsync(int bookingDetailId, CancellationToken cancellationToken = default)
    {
        if (bookingDetailId <= 0)
        {
            return ServiceResult<CheckoutCandidateDto>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            var candidate = await _checkoutQueryRepository.GetCheckoutCandidateByBookingDetailIdAsync(bookingDetailId, cancellationToken);

            if (candidate is null)
            {
                return ServiceResult<CheckoutCandidateDto>.Failure(ErrorMessages.NotFound);
            }

            return ServiceResult<CheckoutCandidateDto>.Success(candidate);
        }
        catch (Exception ex)
        {
            return ServiceResult<CheckoutCandidateDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<CheckoutResultDto>> CheckoutAsync(CheckoutRequestDto request, int currentUserId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.BookingDetailId <= 0)
        {
            return ServiceResult<CheckoutResultDto>.Failure(ErrorMessages.InvalidInput);
        }

        if (currentUserId <= 0)
        {
            return ServiceResult<CheckoutResultDto>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            // Get booking detail
            var bookingDetail = await _bookingOperationRepository.GetBookingDetailByIdAsync(request.BookingDetailId, cancellationToken);
            if (bookingDetail is null)
            {
                return ServiceResult<CheckoutResultDto>.Failure(ErrorMessages.NotFound);
            }

            // Check if booking detail is CheckedIn
            if (bookingDetail.Status != BookingDetailStatus.CheckedIn)
            {
                return ServiceResult<CheckoutResultDto>.Failure(ErrorMessages.BusinessRuleViolation);
            }

            // Get check record
            var checkRecord = await _checkRecordRepository.GetByBookingDetailIdAsync(request.BookingDetailId, cancellationToken);
            if (checkRecord is null)
            {
                return ServiceResult<CheckoutResultDto>.Failure(ErrorMessages.NotFound);
            }

            // Get room
            var room = await _bookingOperationRepository.GetRoomByIdAsync(bookingDetail.RoomId, cancellationToken);
            if (room is null)
            {
                return ServiceResult<CheckoutResultDto>.Failure(ErrorMessages.NotFound);
            }

            // Update check record
            checkRecord.ActualCheckOutDate = DateTime.Now;
            checkRecord.CheckOutByUserId = currentUserId;
            checkRecord.CheckOutNote = request.CheckOutNote;
            checkRecord.Status = CheckRecordStatus.CheckedOut;

            await _checkRecordRepository.UpdateAsync(checkRecord, cancellationToken);

            // Update booking detail status to CheckedOut
            await _bookingOperationRepository.UpdateBookingDetailStatusAsync(
                request.BookingDetailId,
                BookingDetailStatus.CheckedOut,
                cancellationToken);

            // Update room status to Cleaning
            await _bookingOperationRepository.UpdateRoomStatusAsync(
                bookingDetail.RoomId,
                RoomOperationalStatus.Cleaning,
                cancellationToken);

            var result = new CheckoutResultDto
            {
                BookingDetailId = request.BookingDetailId,
                RoomId = bookingDetail.RoomId,
                RoomNumber = room.RoomNumber,
                ActualCheckOutDate = checkRecord.ActualCheckOutDate ?? DateTime.Now,
                Message = "Checkout successful.",
                IsSuccess = true
            };

            return ServiceResult<CheckoutResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<CheckoutResultDto>.Failure(ErrorMessages.SystemError);
        }
    }
}
