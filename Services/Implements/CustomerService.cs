using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Repositories.Implements;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implements;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUserActivityService _userActivityService;

    public CustomerService(
        ICustomerRepository? customerRepository = null,
        IUserActivityService? userActivityService = null)
    {
        _customerRepository = customerRepository ?? new CustomerRepository();
        _userActivityService = userActivityService ?? new UserActivityService();
    }

    public async Task<ServiceResult<List<CustomerListItemDto>>> GetCustomersAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var customers = await _customerRepository.GetCustomersAsync(searchTerm, cancellationToken);
            return ServiceResult<List<CustomerListItemDto>>.Success(customers.Select(MapToListItem).ToList());
        }
        catch
        {
            return ServiceResult<List<CustomerListItemDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<CustomerListItemDto>> CreateCustomerAsync(
        CreateCustomerRequestDto request,
        CurrentSessionDto? currentUser = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var fullName = NormalizeRequired(request.FullName);
        var identityCard = NormalizeOptional(request.IdentityCard);
        var phoneNumber = NormalizeOptional(request.PhoneNumber);
        var email = NormalizeOptional(request.Email);
        var address = NormalizeOptional(request.Address);

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return ServiceResult<CustomerListItemDto>.Failure(ErrorMessages.ValidationFailed, "Full name is required.");
        }

        if (!string.IsNullOrWhiteSpace(identityCard))
        {
            if (await _customerRepository.ExistsByIdentityCardAsync(identityCard, cancellationToken: cancellationToken))
            {
                return ServiceResult<CustomerListItemDto>.Failure(ErrorMessages.DuplicateRecord, "A customer with this identity card already exists.");
            }
        }


        try
        {
            var customer = new Customer
            {
                FullName = fullName,
                IdentityCard = identityCard,
                PhoneNumber = phoneNumber,
                Email = email,
                Address = address,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var created = await _customerRepository.AddAsync(customer, cancellationToken);
            await _userActivityService.RecordActivityAsync(
                currentUser,
                "CustomerCreated",
                "Customer",
                created.CustomerId.ToString(),
                $"Created customer '{created.FullName}'.",
                cancellationToken: cancellationToken);

            return ServiceResult<CustomerListItemDto>.Success(MapToListItem(created), "Customer created successfully.");
        }
        catch
        {
            return ServiceResult<CustomerListItemDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<CustomerListItemDto>> UpdateCustomerAsync(
        UpdateCustomerRequestDto request,
        CurrentSessionDto? currentUser = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.CustomerId <= 0)
        {
            return ServiceResult<CustomerListItemDto>.Failure(ErrorMessages.InvalidInput);
        }

        var fullName = NormalizeRequired(request.FullName);
        var identityCard = NormalizeOptional(request.IdentityCard);
        var phoneNumber = NormalizeOptional(request.PhoneNumber);
        var email = NormalizeOptional(request.Email);
        var address = NormalizeOptional(request.Address);

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return ServiceResult<CustomerListItemDto>.Failure(ErrorMessages.ValidationFailed, "Full name is required.");
        }

        if (!string.IsNullOrWhiteSpace(identityCard))
        {
            if (await _customerRepository.ExistsByIdentityCardAsync(identityCard, request.CustomerId, cancellationToken))
            {
                return ServiceResult<CustomerListItemDto>.Failure(ErrorMessages.DuplicateRecord, "A customer with this identity card already exists.");
            }
        }


        try
        {
            var customer = new Customer
            {
                CustomerId = request.CustomerId,
                FullName = fullName,
                IdentityCard = identityCard,
                PhoneNumber = phoneNumber,
                Email = email,
                Address = address,
                UpdatedAt = DateTime.Now
            };

            var updated = await _customerRepository.UpdateAsync(customer, cancellationToken);
            if (updated is not null)
            {
                await _userActivityService.RecordActivityAsync(
                    currentUser,
                    "CustomerUpdated",
                    "Customer",
                    updated.CustomerId.ToString(),
                    $"Updated customer '{updated.FullName}'.",
                    cancellationToken: cancellationToken);
            }

            return updated is null
                ? ServiceResult<CustomerListItemDto>.Failure(ErrorMessages.NotFound)
                : ServiceResult<CustomerListItemDto>.Success(MapToListItem(updated), "Customer updated successfully.");
        }
        catch
        {
            return ServiceResult<CustomerListItemDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<List<BookingSummaryDto>>> GetCustomerBookingsAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        if (customerId <= 0)
        {
            return ServiceResult<List<BookingSummaryDto>>.Failure(ErrorMessages.InvalidInput);
        }

        try
        {
            var bookings = await _customerRepository.GetCustomerBookingsAsync(customerId, cancellationToken);
            return ServiceResult<List<BookingSummaryDto>>.Success(bookings.Select(booking =>
            {
                var roomNumbers = string.Join(", ", booking.BookingDetails
                    .Where(bd => bd.Room != null)
                    .Select(bd => bd.Room!.RoomNumber));

                var checkIn = booking.BookingDetails.FirstOrDefault()?.CheckInDate ?? booking.BookingDate;
                var checkOut = booking.BookingDetails.FirstOrDefault()?.CheckOutDate ?? booking.BookingDate;
                var total = booking.BookingDetails.Sum(bd => bd.RoomTotal);

                return new BookingSummaryDto
                {
                    BookingId = booking.BookingId,
                    CustomerName = booking.Customer?.FullName ?? string.Empty,
                    RoomNumbers = roomNumbers,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    Status = booking.Status.ToString(),
                    RoomTotal = total
                };
            }).ToList());
        }
        catch
        {
            return ServiceResult<List<BookingSummaryDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    private static CustomerListItemDto MapToListItem(Customer customer)

    {
        return new CustomerListItemDto
        {
            CustomerId = customer.CustomerId,
            FullName = customer.FullName,
            IdentityCard = customer.IdentityCard,
            PhoneNumber = customer.PhoneNumber,
            Email = customer.Email,
            Address = customer.Address,
            CreatedAt = customer.CreatedAt
        };
    }

    private static string NormalizeRequired(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
