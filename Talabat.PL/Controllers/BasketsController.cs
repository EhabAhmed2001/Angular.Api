using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Repository;
using Talabat.PL.DTOs;
using Talabat.PL.Errors;

namespace Talabat.PL.Controllers
{
	[Authorize]
    public class BasketsController : APIBaseController
	{
		private readonly IBasketRepository _basket;
        private readonly IFavouriteRepository _favourite;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public BasketsController(IBasketRepository basket, IFavouriteRepository favourite, IMapper mapper, UserManager<AppUser> userManager)
        {
			_basket = basket;
            this._favourite = favourite;
            _mapper = mapper;
            _userManager = userManager;
        }


		// Get or Recreate Basket
		[HttpGet("basket")]
        public async Task<ActionResult<CustomerBasket>> GetCustomerBasket ()
		{
            string id = $"Cart-{_userManager.GetUserId(User)}";

            var UserBasket = await _basket.GetBasketAsync(id);
			return UserBasket is null? new CustomerBasket(id) : Ok(UserBasket);
		}


		// Update Or Create Basket
		// cart-Uid
		// wishing-uid
		[HttpPost("basket")]
		public async Task<ActionResult<CustomerBasket>> UpdateCustomerBasket (BasketItemDto Item)
		{
            string userId = $"Cart-{_userManager.GetUserId(User)}";
            var UserBasket = await _basket.GetBasketAsync(userId);
            if (UserBasket is null)
                UserBasket = new CustomerBasket(userId);

            // check if item is exists in the basket
            var existingItem = UserBasket.Items.FirstOrDefault(x => x.Id == Item.Id);
            if (existingItem == null)
            {
                // Add the new item to the basket
                var newItem = _mapper.Map<BasketItemDto, BasketItem>(Item);
                UserBasket.Items.Add(newItem);
            }
            else
            {
                // Update the existing item
                existingItem.Quantity = Item.Quantity;
            }


            var UpdateOrCreateBasket = await _basket.UpdateBasketAsync(UserBasket);


            return UpdateOrCreateBasket is null?  BadRequest(new ApiResponse(400)) : Ok(UpdateOrCreateBasket);
            //         string userId = $"Cart-{_userManager.GetUserId(User)}";
            //customerBasket.Id = userId;
            //         var MappedCustomerBasket = _mapper.Map<CustomerBasketDto, CustomerBasket>(customerBasket);
            //var UpdateOrCreateBasket = await _basket.UpdateBasketAsync(MappedCustomerBasket);
        }


		// Delete Basket
		[HttpDelete("basket/{id}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            string userId = $"Cart-{_userManager.GetUserId(User)}";

            var basket = await _basket.GetBasketAsync(userId);
            var existingItem = basket.Items.FirstOrDefault(x => x.Id == id);
            if (existingItem != null)
            {
                basket.Items.Remove(existingItem);
            }
            if(basket.Items.Count == 0)
            {
                await _basket.DeleteBasketAsync(userId);
                return Ok(new { success = true });
            }

            var result = await _basket.UpdateBasketAsync(basket);

            return Ok(new { success = true });
        }

        // =============================================== Favourite ===============================================================

        [HttpGet("favourite")]
        public async Task<ActionResult<CustomerFavourite>> GetCustomerFavourite()
        {
            //var userId = _userManager.GetUserId(User);
            string id = $"Favourite-{_userManager.GetUserId(User)}";
            var UserFavourite = await _favourite.GetBasketAsync(id);
            return UserFavourite is null ? new CustomerFavourite(id) : Ok(UserFavourite);

        }

        [HttpPost("favourite")]
        public async Task<ActionResult<CustomerBasket>> UpdateCustomerFavourite([FromBody] FavouriteItemDto customerFavourite)
        {
            string userId = $"Favourite-{_userManager.GetUserId(User)}";
            var UserFavourite = await _favourite.GetBasketAsync(userId);

            if (UserFavourite is null)
                UserFavourite = new CustomerFavourite(userId);

            // check if item is exists in the favourite
            var existingItem = UserFavourite.Items.FirstOrDefault(x => x.Id == customerFavourite.Id);

            if (existingItem is null)
            {
                // Add the new item to the favourite
                var newItem = _mapper.Map<FavouriteItemDto, FavouriteItem>(customerFavourite);
                UserFavourite.Items.Add(newItem);
            }

            var UpdateOrCreateBasket = await _favourite.UpdateBasketAsync(UserFavourite);

            return UpdateOrCreateBasket is null ? BadRequest(new ApiResponse(400)) : Ok(UpdateOrCreateBasket);

        }


        [HttpDelete("favourite/{id}")]
        public async Task<IActionResult> DeleteFavourite(int id)
        {
            string userId = $"Favourite-{_userManager.GetUserId(User)}";
            var favourite = await _basket.GetBasketAsync(userId);
            var existingItem = favourite.Items.FirstOrDefault(x => x.Id == id);

            if (existingItem != null)
            {
                favourite.Items.Remove(existingItem);
            }
            if (favourite.Items.Count == 0)
            {
                await _favourite.DeleteBasketAsync(userId);
                return Ok(new { success = true });
            }
            var result = await _basket.UpdateBasketAsync(favourite);

            return Ok(new { success = true });
        }


    }
}
