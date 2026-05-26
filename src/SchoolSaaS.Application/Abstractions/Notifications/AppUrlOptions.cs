namespace SchoolSaaS.Application.Abstractions.Notifications;

public sealed class AppUrlOptions
{
    public const string SectionName = "App";

    public string PublicBaseUrl { get; set; } = "http://localhost:5275";
}
