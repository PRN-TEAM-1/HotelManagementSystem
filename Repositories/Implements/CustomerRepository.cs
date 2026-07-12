using BusinessObjects.Entities;
using DataAccessObjects.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly CustomerDao _dao;

    public CustomerRepository(CustomerDao? dao = null)
    {
        _dao = dao ?? new CustomerDao();
    }

    public Task<List<Customer>> GetCustomersAsync(string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        return _dao.GetCustomersAsync(searchTerm, cancellationToken);
    }

    public Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customer);
        return _dao.AddAsync(customer, cancellationToken);
    }

    public Task<Customer?> UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customer);
        return _dao.UpdateAsync(customer, cancellationToken);
    }

    public Task<bool> ExistsByIdentityCardAsync(string? identityCard, int? excludedCustomerId = null, CancellationToken cancellationToken = default)
    {
        return _dao.ExistsByIdentityCardAsync(identityCard, excludedCustomerId, cancellationToken);
    }
}
