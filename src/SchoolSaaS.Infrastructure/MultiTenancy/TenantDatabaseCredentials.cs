namespace SchoolSaaS.Infrastructure.MultiTenancy;

public sealed record TenantDatabaseCredentials(
    string Server,
    int Port,
    string Database,
    string User,
    string Password);
