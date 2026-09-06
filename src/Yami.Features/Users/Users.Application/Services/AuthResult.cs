// What every successful authentication path (signup, password login, OTP login, Google) hands back.
public record AuthResult(User User, string Token, DateTime ExpiresAt);
