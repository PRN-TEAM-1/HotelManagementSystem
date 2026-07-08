using BusinessObjects.Entities;

namespace Repositories.Interfaces;

public interface ICustomerRepository
{
    Task<List<Customer>> GetCustomersAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default);

    Task<Customer?> UpdateAsync(Customer customer, CancellationToken cancellationToken = default);

    Task<bool> ExistsByIdentityCardAsync(
        string? identityCard,
        int? excludedCustomerId = null,
        CancellationToken cancellationToken = default);
}
