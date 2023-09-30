using System.Collections.Generic;

namespace AspnetRunBasics.Models
{
    public class BasketModel
    {
        public string UserName { get; set; }
        public List<BasketItemModel> ShoppingCartItems { get; set; } = new List<BasketItemModel>();
        public decimal TotalPrice { get; set; }
    }
}