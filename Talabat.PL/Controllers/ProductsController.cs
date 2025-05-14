using System;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Repository;
using Talabat.Core.Specifications;
using Talabat.PL.DTOs;
using Talabat.PL.Errors;
using Talabat.PL.Helper;

namespace Talabat.PL.Controllers
{
	public class ProductsController : APIBaseController
	{

		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;

		public ProductsController(IMapper mapper,
									IUnitOfWork UnitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = UnitOfWork;
		}

		//Get All Products
		[CachedAttribute(300)]
		[HttpGet]
		public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetAllProducts([FromQuery] ProductSpecParams Params)
		{
			var Spec = new ProductWithBrandAndTypeSpec(Params);
			var Products = await _unitOfWork.Repository<Product>().GetAllWithSpecAsync(Spec);
			var MappedProduct = _mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(Products);
			var Count = Spec.Count;
			return Ok(new Pagination<ProductToReturnDto>(Params.PageSize, Params.index, MappedProduct, Count));
		}

		// Get Product Using Id
		[HttpGet("{Id}")]
		[ProducesResponseType(typeof(ProductToReturnDto), 200)]
		[ProducesResponseType(typeof(ApiResponse), 404)]
		public async Task<ActionResult<Product>> GetProductById(int Id)
		{
			//var Spec = new ProductWithBrandAndTypeSpec(Id);
			//var product = await _unitOfWork.Repository<Product>().GetByIdWithSpecAsync(Spec);
			var includes =
				new List<Expression<Func<Product, object>>> {   o => o.Brand,
																o => o.Type };
			var product = await _unitOfWork.Repository<Product>().GetByIdAsync(Id, includes);
			if (product is null)
				return NotFound(new ApiResponse(404));
			var MappedProduct = _mapper.Map<Product, ProductToReturnDto>(product);
			return Ok(MappedProduct);
		}


		// Get All Brands
		[HttpGet("Brands")]
		public async Task<ActionResult<IEnumerable<ProductBrand>>> GetAllBrands()
		{
			var brands = await _unitOfWork.Repository<ProductBrand>().GetAllAsync();
			return Ok(brands);
		}


		// Get All Types
		[HttpGet("Types")]
		public async Task<ActionResult<IEnumerable<ProductType>>> GetAllTypes()
		{
			var type = await _unitOfWork.Repository<ProductType>().GetAllAsync();
			return Ok(type);
		}

		[HttpPost]
		public async Task<ActionResult<ProductToReturnDto>> CreateProduct([FromForm] ProductDto product)
		{
			string imagePath = AddPicFile.AddPic(product.Picture, "Products");

			var mappedProduct = _mapper.Map<ProductDto, Product>(product);
			mappedProduct.PictureUrl = imagePath;
			await _unitOfWork.Repository<Product>().AddAsync(mappedProduct);
			var result = await _unitOfWork.CompleteAsync();

			if (result <= 0)
				return BadRequest(new ApiResponse(400, "Failed to create product"));
			var mappedResult = _mapper.Map<Product, ProductToReturnDto>(mappedProduct);
			return CreatedAtAction(nameof(GetProductById), new { Id = mappedResult.Id }, mappedResult);
		}

		[HttpPut("{Id}")]
		public async Task<ActionResult<ProductToReturnDto>> UpdateProduct(int Id, [FromForm] ProductDto product)
		{
			var existingProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(Id);
			if (existingProduct is null)
				return NotFound(new ApiResponse(404));


			var oldImagePath = existingProduct.PictureUrl;

            string imagePath = AddPicFile.AddPic(product.Picture, "Products");

            //var mappedProduct = _mapper.Map<ProductDto, Product>(product);
            //existingProduct = mappedProduct;
            //existingProduct.Id = Id;
            _mapper.Map(product, existingProduct);
            existingProduct.PictureUrl = imagePath;

            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
                return BadRequest(new ApiResponse(400, "Failed to update product"));

            AddPicFile.DeletePic(oldImagePath);
            var mappedResult = _mapper.Map<Product, ProductToReturnDto>(existingProduct);
            return Ok(mappedResult);
        }

        [HttpDelete("{Id}")]
        public async Task<ActionResult<ProductToReturnDto>> DeleteProduct(int Id)
		{
            var existingProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(Id);
            if (existingProduct is null)
                return NotFound(new ApiResponse(404));

            _unitOfWork.Repository<Product>().Delete(existingProduct);
            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
                return BadRequest(new ApiResponse(400, "Failed to delete product"));

            AddPicFile.DeletePic(existingProduct.PictureUrl);
            return Ok(new ApiResponse(200, "Product Deleted Successfully"));
        }
    }

} 
