using Catalog.API.Entities;
using MongoDB.Driver;

namespace Catalog.API.Data
{
    public class CatalogContext : ICatalogContext
    {
        public CatalogContext(IConfiguration confi)
        {
            var client = new MongoClient(confi.GetSection("DatabaseSettings:ConnectionString").Value);
            var database = client.GetDatabase(confi.GetSection("DatabaseSettings:DatabaseName").Value);

            Products = database.GetCollection<Product>(confi.GetSection("DatabaseSettings:DatabaseName").Value);
            CatalogContextSeed.SeedData(Products);
        }

        public IMongoCollection<Product> Products { get; }
    }
}
