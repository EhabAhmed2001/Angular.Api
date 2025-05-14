using AutoMapper;
using Talabat.Core.Entities;
using Talabat.PL.DTOs;

namespace Talabat.PL.Helper
{
    public class TypePicUrlResolve : IValueResolver<ProductType, TypeToReturnDto, string>
    {
        private readonly IConfiguration _configuration;
        public TypePicUrlResolve(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string Resolve(ProductType source, TypeToReturnDto destination, string destMember, ResolutionContext context)
        {
            // check if the image is url
            if (source.Image.Contains("http"))
            {
                return source.Image;
            }
            if (!string.IsNullOrEmpty(source.Image))
            {
                return $"{_configuration["APIBaseUrl"]}{source.Image}";
            }
            return string.Empty;
        }
    }
}
