// Thrown for expected, user-facing auth failures (bad credentials, expired OTP, taken phone
// number, etc). Controllers catch this and map StatusCode straight onto the HTTP response,
// so services can just describe *what* went wrong without knowing about ASP.NET Core.
public class AuthException : Exception
{
    public int StatusCode { get; }

    public AuthException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }

    public AuthException(string message, int statusCode, Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
