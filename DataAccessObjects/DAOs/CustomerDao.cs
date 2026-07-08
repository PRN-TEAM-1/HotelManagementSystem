using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.DAOs;

public sealed class CustomerDao
{
    public async Task<List<Customer>> GetCustomersAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        await using var context = DbContextFactory.CreateDbContext();

        var query = context.Customers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{searchTerm.Trim()}%";
            query = query.Where(customer =>
                EF.Functions.Like(customer.FullName, pattern)
                || (customer.IdentityCard != null && EF.Functions.Like(customer.IdentityCard, pattern))
                || (customer.PhoneNumber != null && EF.Functions.Like(customer.PhoneNumber, pattern))
                || (customer.Email != null && EF.Functions.Like(customer.Email, pattern)));
        }

        return await query
            .OrderBy(customer => customer.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customer);

        await using var context = DbContextFactory.CreateDbContext();

        customer.CreatedAt = DateTime.Now;
        customer.UpdatedAt = DateTime.Now;

        context.Customers.Add(customer);
        await context.SaveChangesAsync(cancellationToken);
        return customer;
    }

    public async Task<Customer?> UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(customer);

        await using var context = DbContextFactory.CreateDbContext();

        var existingCustomer = await context.Customers.FirstOrDefaultAsync(item => item.CustomerId == customer.CustomerId, cancellationToken);

        if (existingCustomer is null)
        {
            return null;
        }

        existingCustomer.FullName = customer.FullName;
        existingCustomer.IdentityCard = customer.IdentityCard;
        existingCustomer.PhoneNumber = customer.PhoneNumber;
        existingCustomer.Email = customer.Email;
        existingCustomer.Address = customer.Address;
        existingCustomer.UpdatedAt = DateTime.Now;

        await context.SaveChangesAsync(cancellationToken);
        return existingCustomer;
    }

    public async Task<bool> ExistsByIdentityCardAsync(
        string? identityCard,
        int? excludedCustomerId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identityCard))
        {
            return false;
        }

        await using var context = DbContextFactory.CreateDbContext();

        var normalized = identityCard.Trim();

        return await context.Customers.AnyAsync(customer =>
            customer.IdentityCard != null
            && customer.IdentityCard == normalized
            && (!excludedCustomerId.HasValue || customer.CustomerId != excludedCustomerId.Value), cancellationToken);
    }
}
