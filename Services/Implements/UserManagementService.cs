using System.Net.Mail;
using BusinessObjects.Constants;
using BusinessObjects.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using DataAccessObjects.Security;
using Repositories.Implements;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implements;

public sealed class UserManagementService : IUserManagementService
{
    private readonly IUserManagementRepository _userManagementRepository;

    public UserManagementService(IUserManagementRepository? userManagementRepository = null)
    {
        _userManagementRepository = userManagementRepository ?? new UserManagementRepository();
    }

    public async Task<ServiceResult<List<UserListItemDto>>> GetUsersAsync(
        CurrentSessionDto? currentUser,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var authorizationResult = EnsureAdmin<List<UserListItemDto>>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        try
        {
            var users = await _userManagementRepository.GetUsersAsync(searchTerm, cancellationToken);
            var items = new List<UserListItemDto>();

            foreach (var user in users)
            {
                items.Add(await MapToListItemAsync(user, cancellationToken));
            }

            return ServiceResult<List<UserListItemDto>>.Success(items);
        }
        catch
        {
            return ServiceResult<List<UserListItemDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<UserListItemDto>> CreateUserAsync(
        CreateUserRequestDto request,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var authorizationResult = EnsureAdmin<UserListItemDto>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        var username = NormalizeRequired(request.Username);
        var fullName = NormalizeRequired(request.FullName);
        var email = NormalizeRequired(request.Email);
        var phoneNumber = NormalizeOptional(request.PhoneNumber);
        var password = request.Password ?? string.Empty;

        var validationErrors = ValidateUserFields(
            username,
            fullName,
            email,
            phoneNumber,
            request.RoleId,
            request.Status,
            password,
            requirePassword: true);

        if (validationErrors.Count > 0)
        {
            return ServiceResult<UserListItemDto>.Failure(
                ErrorMessages.ValidationFailed,
                validationErrors.ToArray());
        }

        try
        {
            var ruleResult = await ValidateUniquenessAndRoleAsync(
                username,
                email,
                request.RoleId,
                excludedUserId: null,
                cancellationToken);

            if (ruleResult is not null)
            {
                return ruleResult;
            }

            var user = new User
            {
                RoleId = request.RoleId,
                Username = username,
                PasswordHash = PasswordHasher.HashPassword(password),
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
                Status = request.Status,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var createdUser = await _userManagementRepository.AddAsync(user, cancellationToken);
            var dto = await MapToListItemAsync(createdUser, cancellationToken);

            return ServiceResult<UserListItemDto>.Success(dto, "User created successfully.");
        }
        catch
        {
            return ServiceResult<UserListItemDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<UserListItemDto>> UpdateUserAsync(
        UpdateUserRequestDto request,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var authorizationResult = EnsureAdmin<UserListItemDto>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        if (request.UserId <= 0)
        {
            return ServiceResult<UserListItemDto>.Failure(ErrorMessages.InvalidInput);
        }

        var username = NormalizeRequired(request.Username);
        var fullName = NormalizeRequired(request.FullName);
        var email = NormalizeRequired(request.Email);
        var phoneNumber = NormalizeOptional(request.PhoneNumber);

        var validationErrors = ValidateUserFields(
            username,
            fullName,
            email,
            phoneNumber,
            request.RoleId,
            request.Status,
            password: null,
            requirePassword: false);

        if (validationErrors.Count > 0)
        {
            return ServiceResult<UserListItemDto>.Failure(
                ErrorMessages.ValidationFailed,
                validationErrors.ToArray());
        }

        if (IsSelfStatusRestriction(request.UserId, request.Status, currentUser))
        {
            return ServiceResult<UserListItemDto>.Failure(
                ErrorMessages.BusinessRuleViolation,
                "You cannot lock or inactivate your own account.");
        }

        try
        {
            var existingUser = await _userManagementRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (existingUser is null)
            {
                return ServiceResult<UserListItemDto>.Failure(ErrorMessages.NotFound);
            }

            var ruleResult = await ValidateUniquenessAndRoleAsync(
                username,
                email,
                request.RoleId,
                request.UserId,
                cancellationToken);

            if (ruleResult is not null)
            {
                return ruleResult;
            }

            existingUser.RoleId = request.RoleId;
            existingUser.Username = username;
            existingUser.FullName = fullName;
            existingUser.Email = email;
            existingUser.PhoneNumber = phoneNumber;
            existingUser.Status = request.Status;

            var updatedUser = await _userManagementRepository.UpdateAsync(existingUser, cancellationToken);
            if (updatedUser is null)
            {
                return ServiceResult<UserListItemDto>.Failure(ErrorMessages.NotFound);
            }

            var dto = await MapToListItemAsync(updatedUser, cancellationToken);

            return ServiceResult<UserListItemDto>.Success(dto, "User updated successfully.");
        }
        catch
        {
            return ServiceResult<UserListItemDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<UserListItemDto>> ChangeStatusAsync(
        ChangeUserStatusRequestDto request,
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var authorizationResult = EnsureAdmin<UserListItemDto>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        if (request.UserId <= 0 || !Enum.IsDefined(request.Status))
        {
            return ServiceResult<UserListItemDto>.Failure(ErrorMessages.InvalidInput);
        }

        if (IsSelfStatusRestriction(request.UserId, request.Status, currentUser))
        {
            return ServiceResult<UserListItemDto>.Failure(
                ErrorMessages.BusinessRuleViolation,
                "You cannot lock or inactivate your own account.");
        }

        try
        {
            var existingUser = await _userManagementRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (existingUser is null)
            {
                return ServiceResult<UserListItemDto>.Failure(ErrorMessages.NotFound);
            }

            var updatedUser = await _userManagementRepository.UpdateStatusAsync(
                request.UserId,
                request.Status,
                cancellationToken);

            if (updatedUser is null)
            {
                return ServiceResult<UserListItemDto>.Failure(ErrorMessages.NotFound);
            }

            var dto = await MapToListItemAsync(updatedUser, cancellationToken);

            return ServiceResult<UserListItemDto>.Success(dto, "User status updated successfully.");
        }
        catch
        {
            return ServiceResult<UserListItemDto>.Failure(ErrorMessages.SystemError);
        }
    }

    public async Task<ServiceResult<List<LookupItemDto>>> GetRoleLookupsAsync(
        CurrentSessionDto? currentUser,
        CancellationToken cancellationToken = default)
    {
        var authorizationResult = EnsureAdmin<List<LookupItemDto>>(currentUser);
        if (authorizationResult is not null)
        {
            return authorizationResult;
        }

        try
        {
            var roles = await _userManagementRepository.GetRolesAsync(cancellationToken);
            var lookups = roles
                .Select(role => new LookupItemDto
                {
                    Id = role.RoleId,
                    Value = role.Name.ToString(),
                    DisplayName = role.Name.ToString()
                })
                .ToList();

            return ServiceResult<List<LookupItemDto>>.Success(lookups);
        }
        catch
        {
            return ServiceResult<List<LookupItemDto>>.Failure(ErrorMessages.SystemError);
        }
    }

    private static ServiceResult<T>? EnsureAdmin<T>(CurrentSessionDto? currentUser)
    {
        if (currentUser is null || !currentUser.IsAuthenticated)
        {
            return ServiceResult<T>.Failure(ErrorMessages.Unauthorized);
        }

        if (currentUser.RoleName != RoleName.Admin)
        {
            return ServiceResult<T>.Failure(ErrorMessages.Forbidden);
        }

        return null;
    }

    private async Task<ServiceResult<UserListItemDto>?> ValidateUniquenessAndRoleAsync(
        string username,
        string email,
        int roleId,
        int? excludedUserId,
        CancellationToken cancellationToken)
    {
        if (!await _userManagementRepository.RoleExistsAsync(roleId, cancellationToken))
        {
            return ServiceResult<UserListItemDto>.Failure(
                ErrorMessages.ValidationFailed,
                "Selected role does not exist.");
        }

        if (await _userManagementRepository.ExistsByUsernameAsync(username, excludedUserId, cancellationToken))
        {
            return ServiceResult<UserListItemDto>.Failure(
                ErrorMessages.DuplicateRecord,
                "Username is already used by another account.");
        }

        if (await _userManagementRepository.ExistsByEmailAsync(email, excludedUserId, cancellationToken))
        {
            return ServiceResult<UserListItemDto>.Failure(
                ErrorMessages.DuplicateRecord,
                "Email is already used by another account.");
        }

        return null;
    }

    private static List<string> ValidateUserFields(
        string username,
        string fullName,
        string email,
        string? phoneNumber,
        int roleId,
        UserStatus status,
        string? password,
        bool requirePassword)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(username))
        {
            errors.Add("Username is required.");
        }
        else if (username.Length is < ValidationRules.UsernameMinLength or > ValidationRules.UsernameMaxLength)
        {
            errors.Add($"Username must be {ValidationRules.UsernameMinLength}-{ValidationRules.UsernameMaxLength} characters.");
        }

        if (requirePassword)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("Password is required.");
            }
            else if (password.Length < ValidationRules.PasswordMinLength)
            {
                errors.Add($"Password must be at least {ValidationRules.PasswordMinLength} characters.");
            }
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            errors.Add("Full name is required.");
        }
        else if (fullName.Length > ValidationRules.FullNameMaxLength)
        {
            errors.Add($"Full name cannot exceed {ValidationRules.FullNameMaxLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add("Email is required.");
        }
        else if (email.Length > ValidationRules.EmailMaxLength || !IsValidEmail(email))
        {
            errors.Add("Email is not valid.");
        }

        if (!string.IsNullOrWhiteSpace(phoneNumber) && phoneNumber.Length > ValidationRules.PhoneNumberMaxLength)
        {
            errors.Add($"Phone number cannot exceed {ValidationRules.PhoneNumberMaxLength} characters.");
        }

        if (roleId <= 0)
        {
            errors.Add("Role is required.");
        }

        if (!Enum.IsDefined(status))
        {
            errors.Add("Status is not valid.");
        }

        return errors;
    }

    private async Task<UserListItemDto> MapToListItemAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var hasBusinessReferences = await _userManagementRepository.HasBusinessReferencesAsync(
            user.UserId,
            cancellationToken);

        return new UserListItemDto
        {
            UserId = user.UserId,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name.ToString() ?? string.Empty,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Status = user.Status.ToString(),
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            HasBusinessReferences = hasBusinessReferences
        };
    }

    private static bool IsSelfStatusRestriction(
        int userId,
        UserStatus status,
        CurrentSessionDto? currentUser)
    {
        return currentUser?.UserId == userId && status != UserStatus.Active;
    }

    private static string NormalizeRequired(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new MailAddress(email);
            return string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
