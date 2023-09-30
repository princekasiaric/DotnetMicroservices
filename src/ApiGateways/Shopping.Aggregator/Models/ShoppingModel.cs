namespace Shopping.Aggregator.Models
{
    /// <summary>
    /// Gateway Shopping Request Model
    /// </summary>
    public class ShoppingModel
    {
        public string UserName { get; set; }
        public BasketModel BasketWithProducts { get; set; }
        public IEnumerable<OrderResponseModel> Orders { get; set; }
    }
}