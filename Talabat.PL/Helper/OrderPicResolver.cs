using AutoMapper;
using Talabat.Core.Entities.Order_Aggregate;
using Talabat.PL.DTOs;

namespace Talabat.PL.Helper
{
	public class OrderPicResolver : IValueResolver<OrderItem, OrderItemDto, string>
	{
		private readonly IConfiguration _configuration;

		public OrderPicResolver(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		public string Resolve(OrderItem source, OrderItemDto destination, string destMember, ResolutionContext context)
		{
			// check if the image is url
			if(source.Product.PictureUrl.Contains("http"))
            {
                return source.Product.PictureUrl;
            }

            if (!string.IsNullOrEmpty(source.Product.PictureUrl))
			{
				return $"{_configuration["APIBaseUrl"]}{source.Product.PictureUrl}";
			}

			return string.Empty;
		}
	}
}
