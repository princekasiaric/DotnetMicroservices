using Discount.Grpc.Entities;

namespace Discount.Grpc.Repositories 
{
    public interface IDiscountRepository
    {
        Task<Coupon> GetDiscount(string productName);
        Task<bool> CreateDiscount(Coupon discount);
        Task<bool> UpdateDiscount(Coupon discount);
        Task<bool> DeleteDiscount(string productName);
    }
}
