using System.Collections.Generic;
using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.Core.Repository;
using Talabat.Core.Services;
using Talabat.PL.DTOs;
using Talabat.PL.Errors;

namespace Talabat.PL.Controllers
{

	public class OrderController : APIBaseController
	{
		private readonly IOrderService _orderSercive;
		private readonly IMapper _mapper;
        private readonly IBasketRepository _basket;

        public OrderController(IOrderService OrderSercive, IMapper Mapper, IBasketRepository basket)
		{
			_orderSercive = OrderSercive;
			_mapper = Mapper;
            _basket = basket;
        }

		[ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
		[HttpPost]
		[Authorize]
		public async Task<ActionResult<OrderToReturnDto>> CreateOrder(OrderDto orderDto)
		{
			var BuyerEmail = User.FindFirstValue(ClaimTypes.Email);
			var MappedAddress = _mapper.Map<AddressDto, OrderAddress>(orderDto.ShippingAddress);
			var order = await _orderSercive.CreateOrderAsync(BuyerEmail, orderDto.BasketId, orderDto.DeliveryMethodId, MappedAddress);
			if (order is null)
				return BadRequest(new ApiResponse(400, "There Is a Problem With Your Order"));

            var OrderMapped = _mapper.Map<Order, OrderToReturnDto>(order);

            // delete the basket after creating the order
            var basket = await _basket.GetBasketAsync(orderDto.BasketId);
            if (basket != null)
                await _basket.DeleteBasketAsync(orderDto.BasketId);


            return Ok(OrderMapped);
		}


		[ProducesResponseType(typeof(IReadOnlyList<OrderToReturnDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IReadOnlyList<OrderToReturnDto>>> GetOrdersForUser()
		{
			var UserEmail = User.FindFirstValue(ClaimTypes.Email);
			var orders = await _orderSercive.GetOrderForUserAsync(UserEmail);

			var OrderMapped = _mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderToReturnDto>>(orders);


			return orders is null ? NotFound(new ApiResponse(404, "There Is No Order For This User")) : Ok(OrderMapped);
		}


		[ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
		[HttpGet("{id}")]
		[Authorize]
		public async Task<ActionResult<OrderToReturnDto>> GetOrdersByIdForUser(int id)
		{
			var UserEmail = User.FindFirstValue(ClaimTypes.Email);
			var order = await _orderSercive.GetOrderByIdForUserAsync(UserEmail, id);
			
			var OrderMapped = _mapper.Map<Order, OrderToReturnDto>(order);

			return order is null ? NotFound(new ApiResponse(404, $"There Is No Order With Id = {id} For This User")) : Ok(OrderMapped);
		}


		// GetAllOrders
		[HttpGet("GetAllOrders")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderToReturnDto>> GetOrders()
        {
            var order = await _orderSercive.GetAllOrders();

            var OrderMapped = _mapper.Map<IReadOnlyList<Order>, IReadOnlyList<OrderToReturnDto>>(order);

            return order is null ? Ok(new ApiResponse(200, "No Oredrs Yet")) : Ok(OrderMapped);
        }


        [HttpGet("DeliveryMethods")]
		public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetAllDeliveryMethods()
		=> Ok(await _orderSercive.GetAllDeliveryMethodsAsync());

	}
}
