using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Models;
using StudyTimer.Core.Utils;

namespace StudyTimer.Core.Services;

public sealed class AuthService(StudyDataStore store)
{
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

        var user = store.Users.SingleOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
        if (user is null || !PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt))
        {
            throw new UnauthorizedException("Invalid username or password.");
        }

        return new AuthenticatedSession
        {
            User = user,
            AccessToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
        };
    }

    private User Register(string username, string password, UserRole role, int? studentId)
    {
        Guard.NotNullOrWhiteSpace(username, nameof(username));
        Guard.NotNullOrWhiteSpace(password, nameof(password));
        if (password.Length < 8)
        {
            throw new ValidationException("Password must be at least 8 characters.");
        }

        if (store.Users.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ValidationException($"Username '{username}' is already in use.");
        }

        if (role is UserRole.Student && studentId is not null && store.Users.Any(x => x.StudentId == studentId))
        {
            throw new ValidationException($"Student {studentId} already has a linked user account.");
        }

        var (hash, salt) = PasswordHasher.HashPassword(password);
        var user = new User
        {
            Id = store.NextUserId(),
            Username = username.Trim(),
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = role,
            StudentId = studentId
        };

        store.Users.Add(user);
        return user;
    }
}
