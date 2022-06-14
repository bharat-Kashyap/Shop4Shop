using Npgsql;

namespace Discount.API.Extentions
{
    public static class HostExtentions
    {
        public static IHost MigrateDatabse<TContext>(this IHost host, int? retry = 0)
        {
            var retryForAvailability = retry.Value;

            using (var scope = host.Services.CreateScope())
            {
                var services =  scope.ServiceProvider;
                var configuration  =  services.GetRequiredService<IConfiguration>();
                var logger = services.GetRequiredService<ILogger<TContext>>();
                try
                {
                    logger.LogInformation("Migrating Postgres database");

                    using var connection = new NpgsqlConnection(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
                    connection.Open();

                    using var command = new NpgsqlCommand
                    {
                        Connection = connection
                    };
                    command.CommandText = "DROP TABLE IF EXISTS COUPON";
                    command.ExecuteNonQuery();


                    command.CommandText = @"Create Table Coupon(
                                                ID SERIAL PRIMARY KEY  NOT NULL,
                                                ProductName VARCHAR(24) NOT NULL, 
                                                Description TEXT, 
                                                Amount INT
                                            )";
                    command.ExecuteNonQuery();

                    command.CommandText = "Insert Into Coupon(ProductName, description, amount) values('IPHONE X', 'Iphone description', 150);";
                    command.ExecuteNonQuery();

                    command.CommandText = "Insert Into Coupon(ProductName, description, amount) values('Samsung 10', 'Samsung description', 170);";
                    command.ExecuteNonQuery();

                    logger.LogInformation("Migrated postgress database");                    

                }
                catch (NpgsqlException ex)
                {

                    logger.LogWarning(ex, "An error occured while migraing the postgressql database");
                    if (retryForAvailability < 50)
                    {
                        retryForAvailability++; 
                        System.Threading.Thread.Sleep(2000);
                        MigrateDatabse<TContext>(host, retryForAvailability);
                    }
                    
                }
            }

            return host; 
        }
    }
}
