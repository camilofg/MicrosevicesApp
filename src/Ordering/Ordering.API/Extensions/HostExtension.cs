using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordering.API.Extensions
{
    public static class HostExtension
    {
        public static async Task<IHost> MigrateDatabase<T>(this IHost host, int? retry = 0) where T : DbContext {
            int retryForAvailability = retry.Value;
            using (var scope = host.Services.CreateScope()) {
                var services = scope.ServiceProvider;

                try
                {
                    var db = services.GetRequiredService<T>();
                    db.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError($"An error occured while migratign the Database. Retries {retryForAvailability}");
                    if (retryForAvailability < 5) {
                        retryForAvailability++;
                        await Task.Delay(2000);
                        await MigrateDatabase<T>(host, retryForAvailability);
                    }
                    throw;
                }
                return host;
            }
        }
    }
}
