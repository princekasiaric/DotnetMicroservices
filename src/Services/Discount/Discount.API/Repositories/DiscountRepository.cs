using Dapper;
using Discount.API.Entities;
using Npgsql;
using System.Data;

namespace Discount.API.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly IConfiguration _config;

        public NpgsqlConnection Connection 
        {
            get
            {
                var conn = new NpgsqlConnection(_config.GetValue<string>("DatabaseSettings:ConnectionString"));
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                return conn;
            }
        }

        public DiscountRepository(IConfiguration config)
        {
            _config = config??throw new ArgumentNullException(nameof(config));
        }

        public async Task<Coupon> GetDiscount(string productName)
        {

            var coupon = await Connection.QuerySingleOrDefaultAsync<Coupon>("SELECT * FROM Coupon WHERE ProductName =@ProductName",
                new { ProductName = productName });
            await Connection.DisposeAsync();

            if (coupon == null)
                return new Coupon { ProductName = "No Discount", Amount = 0, Description = "No Discount Desc" };
            return coupon;
        }

        public async Task<bool> CreateDiscount(Coupon discount)
        {
            var affectedRow = await Connection.ExecuteAsync("INSERT INTO Coupon (ProductName,Description,Amount) " +
                "VALUES (@ProductName,@Description,@Amount)", new { discount.ProductName, discount.Description, discount.Amount });

            if (affectedRow < 0)
                return false;
            return true;
        }

        public async Task<bool> UpdateDiscount(Coupon discount)
        {
            var affectedRow = await Connection.ExecuteAsync("UPDATE Coupon SET ProductName =@ProductName, Description =@Description, Amount = @Amount WHERE Id =@Id", 
                new { discount.ProductName, discount.Description, discount.Amount, discount.Id });
            await Connection.DisposeAsync();

            if (affectedRow < 0)
                return false;
            return true;
        }

        public async Task<bool> DeleteDiscount(string productName)
        {
            var affectedRow = await Connection.ExecuteAsync("DELETE FROM Coupon WHERE ProductName =@ProductName", new { ProductName = productName });
            await Connection.DisposeAsync();

            if (affectedRow < 0)
                return false;

            return true;
        }
    }
}
