using StudyTimer.Core.Exceptions;
using StudyTimer.Core.Models;
using StudyTimer.Core.Services;

namespace StudyTimer.Tests;

public class AuthServiceTests
{
    [Fact]
    public void RegisterAndLogin_Admin_Succeeds()
    {
        var store = new StudyDataStore();
        var service = new AuthService(store);

        var user = service.RegisterAdmin("admin", "Password123");
        var session = service.Login("admin", "Password123");

        Assert.Equal(UserRole.Admin, user.Role);
        Assert.Equal(user.Id, session.User.Id);
        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
    }

    [Fact]
    public void RegisterStudentUser_WithoutStudent_Throws()
    {
        var store = new StudyDataStore();
        var service = new AuthService(store);

        var exception = Assert.Throws<NotFoundException>(() => service.RegisterStudentUser("s1", "Password123", 999));
        Assert.Contains("Student 999", exception.Message);
    }

    [Fact]
    public void Login_WithWrongPassword_Throws()
    {
        var store = new StudyDataStore();
        var service = new AuthService(store);
        service.RegisterAdmin("admin", "Password123");

        Assert.Throws<UnauthorizedException>(() => service.Login("admin", "wrong-pass"));
    }
}
