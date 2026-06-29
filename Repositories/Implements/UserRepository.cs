using BusinessObjects.Entities;
using DataAccessObjects.DAOs;
using Repositories.Interfaces;

namespace Repositories.Implements;

public sealed class UserRepository : IUserRepository
{
    private readonly UserDao _userDao;

    public UserRepository(UserDao? userDao = null)
    {
        _userDao = userDao ?? new UserDao();
    }

    public Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _userDao.GetByIdAsync(userId, cancellationToken);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return _userDao.GetByUsernameAsync(username, cancellationToken);
    }
}
