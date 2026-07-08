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

    public CustomerService(ICustomerRepository? customerRepository = null)
    {
        _customerRepository = customerRepository ?? new CustomerRepository();
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

        if (await _customerRepository.ExistsByIdentityCardAsync(identityCard, cancellationToken: cancellationToken))
        {
            return ServiceResult<CustomerListItemDto>.Failure(ErrorMessages.DuplicateRecord, "Identity card already exists.");
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
            return ServiceResult<CustomerListItemDto>.Success(MapToListItem(created), "Customer created successfully.");
        }
        catch
        {
            return ServiceResult<CustomerListItemDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<CustomerListItemDto>> UpdateCustomerAsync(
        UpdateCustomerRequestDto request,
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

        if (await _customerRepository.ExistsByIdentityCardAsync(identityCard, request.CustomerId, cancellationToken))
        {
            return ServiceResult<CustomerListItemDto>.Failure(ErrorMessages.DuplicateRecord, "Identity card already exists.");
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
            return updated is null
                ? ServiceResult<CustomerListItemDto>.Failure(ErrorMessages.NotFound)
                : ServiceResult<CustomerListItemDto>.Success(MapToListItem(updated), "Customer updated successfully.");
        }
        catch
        {
            return ServiceResult<CustomerListItemDto>.Failure(ErrorMessages.SystemError);
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
