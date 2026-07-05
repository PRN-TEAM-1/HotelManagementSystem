using BusinessObjects.DTOs;

namespace Services.Interfaces;

public interface IUserManagementService
{
    Task<ServiceResult<List<UserListItemDto>>> GetUsersAsync(
        CurrentSessionDto? currentUser,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<UserListItemDto>> CreateUserAsync(
        CreateUserRequestDto request,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<UserListItemDto>> UpdateUserAsync(
        UpdateUserRequestDto request,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<UserListItemDto>> ChangeStatusAsync(
        ChangeUserStatusRequestDto request,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<List<LookupItemDto>>> GetRoleLookupsAsync(
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default);
}
