using AutoMapper;
using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using EventBus.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Basket.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BasketController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IBasketRepository _repository;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly DiscountGrpcService _discountGrpcService;

        public BasketController(IMapper mapper, IBasketRepository repository, IPublishEndpoint publishEndpoint, DiscountGrpcService discountGrpcService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
            _discountGrpcService = discountGrpcService ?? throw new ArgumentNullException(nameof(discountGrpcService));
        }

        [HttpGet("{userName}", Name = "GetBasket")]
        [ProducesResponseType(typeof(ShoppingCart), StatusCodes.Status200OK)]
        public async Task<ActionResult<ShoppingCart>> GetBasket(string userName)
        {
            var basket = await _repository.GetBasket(userName);
            return Ok(basket??new ShoppingCart(userName));
        }

        [HttpPost()]
        [ProducesResponseType(typeof(ShoppingCart), StatusCodes.Status200OK)]
        public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody]ShoppingCart basket)
        {
            foreach (var item in basket.ShoppingCartItems)
            {
                var coupon = await _discountGrpcService.GetDiscount(item.ProductName);
                item.Price -= coupon.Amount;
            }
            return Ok(await _repository.UpdateBasket(basket));
        }

        [HttpDelete("{userName}", Name = "DeleteBasket")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteBasket(string userName)
        {
            await _repository.DeleteBasket(userName);
            return Ok();
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Checkout([FromBody]BasketCheckout basketCheckout)
        {
            // get existing basket with total price
            var basket = await _repository.GetBasket(basketCheckout.UserName);
            if (basket == null)
            {
                return BadRequest();
            }

            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
            // Set TotalPrice on basketCheckout eventMessage
            eventMessage.TotalPrice = basket.TotalPrice;
            // send checkout event to rabbitmq
            await _publishEndpoint.Publish(eventMessage);

            // remove the basket
            await _repository.DeleteBasket(basket.UserName);

            return Accepted();
        }
    }
}
