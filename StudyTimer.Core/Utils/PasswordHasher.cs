using System.Security.Cryptography;

namespace StudyTimer.Core.Utils;

internal static class PasswordHasher
{
    public static (string hash, byte[] salt) HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Hash(password, salt);
        return (hash, salt);
    }

    public static bool Verify(string password, string expectedHash, byte[] salt)
    {
        var hash = Hash(password, salt);
        return CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(hash), Convert.FromBase64String(expectedHash));
    }

    private static string Hash(string password, byte[] salt)
    {
        using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(deriveBytes.GetBytes(32));
    }
}
