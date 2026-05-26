using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace SchoolSaaS.Infrastructure.Persistence;

internal static class MySqlServerVersionProvider
{
    public static readonly ServerVersion Version = new MySqlServerVersion(new Version(8, 0, 36));
}
