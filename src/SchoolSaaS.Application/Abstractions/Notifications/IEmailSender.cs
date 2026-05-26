namespace SchoolSaaS.Application.Abstractions.Notifications;

public interface IEmailSender
{
    Task SendPasswordResetAsync(
        string toEmail,
        string resetUrl,
        CancellationToken cancellationToken = default);

    Task SendUserInvitationAsync(
        string toEmail,
        string invitationUrl,
        CancellationToken cancellationToken = default);
}
