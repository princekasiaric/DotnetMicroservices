namespace Basket.API.Entities
{
    public class ShoppingCart
    {
        private List<ShoppingCartItem> _items;

        public string UserName { get; set; }
        public IList<ShoppingCartItem> ShoppingCartItems
        {
            get => _items;

            set => _items = (List<ShoppingCartItem>)value;
        }

        public ShoppingCart()
        {
            _items= new List<ShoppingCartItem>();
        }

        public ShoppingCart(string userName)
        {
            UserName = userName;
            _items = new List<ShoppingCartItem>();
        }

        public decimal TotalPrice
        {
            get
            {
                decimal totalPrice = 0;
                if (_items.Any())
                {
                    foreach (var item in _items)
                        totalPrice += item.Price * item.Quantity;
                }
                return totalPrice;
            }
        }
    }
}
