using BCrypt.Net;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (SaltParseException)
        {
            // Hash was never a valid BCrypt hash (e.g. a corrupted/legacy value) - treat as no match.
            return false;
        }
    }
}
