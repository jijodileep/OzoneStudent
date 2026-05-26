namespace SchoolSaaS.Infrastructure.Caching;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = "localhost:6379,abortConnect=false";

    public string InstancePrefix { get; set; } = "schoolsaas";
}
