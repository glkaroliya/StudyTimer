using System.Security.Cryptography;
using StudyTimer.Core.Abstractions;
using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class AuthService(StudyDataStore store, IDateTimeProvider? dateTimeProvider = null)
{
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider ?? new SystemDateTimeProvider();

    public User RegisterAdmin(string username, string password)
    {
        return Register(username, password, UserRole.Admin, null);
    }

    public User RegisterStudentUser(string username, string password, int studentId)
    {
        Guard.Positive(studentId, nameof(studentId));
        if (!store.Students.Any(s => s.Id == studentId))
        {
            throw new NotFoundException($"Student {studentId} was not found.");
        }

        return Register(username, password, UserRole.Student, studentId);
    }

    public AuthenticatedSession Login(string username, string password)
    {
        Guard.NotNullOrWhiteSpace(username, nameof(username));
        Guard.NotNullOrWhiteSpace(password, nameof(password));
        var normalizedUsername = username.Trim();

        var user = store.Users.SingleOrDefault(u => string.Equals(u.Username, normalizedUsername, StringComparison.OrdinalIgnoreCase));
        if (user is null)
        {
            throw new UnauthorizedException("Invalid username or password.");
        }

        if (user.LockedUntilUtc.HasValue && user.LockedUntilUtc.Value > _dateTimeProvider.UtcNow)
        {
            throw new UnauthorizedException("Account is temporarily locked due to failed login attempts.");
        }

        if (!PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockedUntilUtc = _dateTimeProvider.UtcNow.AddMinutes(15);
            }

            throw new UnauthorizedException("Invalid username or password.");
        }

        user.FailedLoginAttempts = 0;
        user.LockedUntilUtc = null;

        return new AuthenticatedSession
        {
            User = user,
            AccessToken = GenerateAccessToken()
        };
    }

    private User Register(string username, string password, UserRole role, int? studentId)
    {
        Guard.NotNullOrWhiteSpace(username, nameof(username));
        Guard.NotNullOrWhiteSpace(password, nameof(password));
        var normalizedUsername = username.Trim();
        Guard.NotNullOrWhiteSpace(normalizedUsername, nameof(username));
        ValidatePasswordPolicy(password, username);

        if (store.Users.Any(u => string.Equals(u.Username, normalizedUsername, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException($"Username '{normalizedUsername}' is already in use.");
        }

        if (role is UserRole.Student && studentId is not null && store.Users.Any(x => x.StudentId == studentId))
        {
            throw new ValidationException($"Student {studentId} already has a linked user account.");
        }

        var (hash, salt) = PasswordHasher.HashPassword(password);
        var user = new User
        {
            Id = store.NextUserId(),
            Username = normalizedUsername,
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = role,
            StudentId = studentId,
            FailedLoginAttempts = 0,
            LockedUntilUtc = null,
            PasswordChangedAtUtc = _dateTimeProvider.UtcNow
        };

        store.Users.Add(user);
        return user;
    }

    private static string GenerateAccessToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    private static void ValidatePasswordPolicy(string password, string username)
    {
        if (password.Length < 8)
        {
            throw new ValidationException("Password must be at least 8 characters.");
        }

        if (password.Contains(' '))
        {
            throw new ValidationException("Password cannot contain spaces.");
        }

        if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
        {
            throw new ValidationException("Password must include uppercase, lowercase, and numeric characters.");
        }

        if (!string.IsNullOrWhiteSpace(username) && password.Contains(username.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("Password cannot contain username.");
        }
    }
}
