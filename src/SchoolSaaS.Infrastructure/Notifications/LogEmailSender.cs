using Microsoft.Extensions.Logging;
using SchoolSaaS.Application.Abstractions.Notifications;

namespace SchoolSaaS.Infrastructure.Notifications;

public sealed class LogEmailSender(ILogger<LogEmailSender> logger) : IEmailSender
{
    public Task SendPasswordResetAsync(
        string toEmail,
        string resetUrl,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Password reset email for {Email}: {ResetUrl}",
            MaskEmail(toEmail),
            resetUrl);

        return Task.CompletedTask;
    }

    public Task SendUserInvitationAsync(
        string toEmail,
        string invitationUrl,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "User invitation email for {Email}: {InvitationUrl}",
            MaskEmail(toEmail),
            invitationUrl);

        return Task.CompletedTask;
    }

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1)
        {
            return "***";
        }

        return $"{email[0]}***{email[at..]}";
    }
}
