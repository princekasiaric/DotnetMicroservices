namespace Shopping.Aggregator.Models
{
    /// <summary>
    /// Basket Shopping Request Model
    /// </summary>
    public class BasketModel
    {
        public string UserName { get; set; }
        public List<BasketItemExtendedModel> ShoppingCartItems { get; set; } = new List<BasketItemExtendedModel>();
        public decimal TotalPrice { get; set; }
    }
}