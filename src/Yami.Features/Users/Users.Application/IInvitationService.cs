// Contract for sending invitation notifications. Lets us swap the underlying
// provider (e.g. SMS, email, push) without touching the service that calls it.
public interface IInvitationService
{
    // Sends the counterparty an invitation SMS with a link to the generic
    // registration page. The link carries no token: the counterparty
    // authenticates via Cognito (phone OTP) and is matched to their
    // invitation by phone during registration.
    Task SendInvitationSms(string phone, string inviterName);
    Task SendInvitationEmail(string email, string inviterName);
}
