using Npgsql;

namespace Discount.Grpc.Extensions 
{
    public static class HostExtension
    {
        public static WebApplication MigrateDatabase<TContext>(this WebApplication webApp, int? retry = 0)
        {
            int retryForAvailability = retry.Value;

            using var scope = webApp.Services.CreateScope();
            var services = scope.ServiceProvider;
            var config = services.GetRequiredService<IConfiguration>();
            var logger = services.GetRequiredService<ILogger<TContext>>();

            try
            {
                logger.LogInformation("Migrating postgresql database...");

                using var conn = new NpgsqlConnection(config.GetValue<string>("DatabaseSettings:ConnectionString"));
                conn.Open();

                using var cmd = new NpgsqlCommand { Connection= conn };
                cmd.CommandText = "DROP TABLE IF EXISTS Coupon";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"create table Coupon(ID serial primary key not null,
                                                        ProductName varchar(24) not null,
                                                        Description text,
                                                        Amount int)";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "insert into Coupon (ProductName, Description, Amount) values ('IPhone X', 'IPhone Discount', 150);";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "insert into Coupon (ProductName, Description, Amount) values ('Samsung 10', 'Samsung Discount', 100);";
                cmd.ExecuteNonQuery();

                logger.LogInformation("Migrated postgresql database.");
            }
            catch (NpgsqlException ex)
            {
                logger.LogError(ex, "An error occurred while migrating the postgresql database");

                if (retryForAvailability < 50)
                {
                    retryForAvailability++;
                    Thread.Sleep(2000);
                    MigrateDatabase<TContext>(webApp, retryForAvailability);
                }
            }
            return webApp;
        }
    }
}
