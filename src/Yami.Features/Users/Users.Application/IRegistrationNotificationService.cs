public interface IRegistrationNotificationService
{
    Task SendRegistrationSuccessfulSms(Guid userId, string phone, string userName);
    Task SendRegistrationSuccessfulEmail(Guid userId, string email, string userName);
}
