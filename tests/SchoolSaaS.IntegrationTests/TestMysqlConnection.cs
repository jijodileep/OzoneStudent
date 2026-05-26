using MySqlConnector;

namespace SchoolSaaS.IntegrationTests;

internal static class TestMysqlConnection
{
    public static MySqlConnectionStringBuilder Parse(string connectionString) =>
        new(connectionString);

    public static string Platform(string baseConnectionString) =>
        WithDatabase(baseConnectionString, "schoolsaas_platform");

    public static string Provisioning(string baseConnectionString)
    {
        var builder = Parse(baseConnectionString);
        builder.Database = string.Empty;
        return builder.ConnectionString;
    }

    public static string TenantDatabase(string baseConnectionString, string databaseName) =>
        WithDatabase(baseConnectionString, databaseName);

    private static string WithDatabase(string baseConnectionString, string database)
    {
        var builder = Parse(baseConnectionString);
        builder.Database = database;
        return builder.ConnectionString;
    }
}
