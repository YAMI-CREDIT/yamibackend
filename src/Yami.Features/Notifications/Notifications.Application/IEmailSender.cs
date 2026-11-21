/// <summary>
/// Provider-neutral email attachment. Implementations map this to their
/// underlying SDK's attachment type.
/// </summary>
public record Attachment(string Filename, byte[] Content, string ContentType);

public interface IEmailSender
{
    Task SendEmail(string email, string subject, string htmlContent);

    Task SendEmailWithAttachment(
        string email,
        string subject,
        string htmlContent,
        IEnumerable<Attachment> attachments);
}