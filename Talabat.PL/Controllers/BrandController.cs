using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.PL.DTOs;
using Talabat.PL.Errors;

namespace Talabat.PL.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BrandController : APIBaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public BrandController(IMapper mapper,IUnitOfWork UnitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = UnitOfWork;
        }

        //Get All Brands
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductBrand>>> GetAllBrands()
        {
            var brands = await _unitOfWork.Repository<ProductBrand>().GetAllAsync();
            return Ok(brands);
        }

        //Get Brand By Id
        [HttpGet("{Id}")]
        public async Task<ActionResult<ProductBrand>> GetBrandById(int Id)
        {
            var brand = await _unitOfWork.Repository<ProductBrand>().GetByIdAsync(Id);
            if (brand is null)
                return NotFound(new ApiResponse(404));
            return Ok(brand);
        }

        //Add Brand
        [HttpPost]
        public async Task<ActionResult<ProductBrand>> AddBrand(BrandDto brand)
        {
            if (brand is null)
                return BadRequest(new ApiResponse(400));

            var newBrand = _mapper.Map<BrandDto, ProductBrand>(brand);

            await _unitOfWork.Repository<ProductBrand>().AddAsync(newBrand);
            var result = await _unitOfWork.CompleteAsync();
            if (result > 0)
                return Ok(newBrand);
            return BadRequest(new ApiResponse(400));
        }

        //Update Brand
        [HttpPut("{Id}")]
        public async Task<ActionResult<ProductBrand>> UpdateBrand(int Id, BrandDto brand)
        {
            if (brand is null)
                return BadRequest(new ApiResponse(400));

            var ExistBrand = await _unitOfWork.Repository<ProductBrand>().GetByIdAsync(Id);

            if (ExistBrand is null)
                return NotFound(new ApiResponse(404));

            _mapper.Map(brand, ExistBrand);
            var result = await _unitOfWork.CompleteAsync();
            if (result > 0)
                return Ok(ExistBrand);

            return BadRequest(new ApiResponse(400));
        }

        //Delete Brand
        [HttpDelete("{Id}")]
        public async Task<ActionResult<ProductBrand>> DeleteBrand(int Id)
        {
            var ExistBrand = await _unitOfWork.Repository<ProductBrand>().GetByIdAsync(Id);
            if (ExistBrand is null)
                return NotFound(new ApiResponse(404));
            _unitOfWork.Repository<ProductBrand>().Delete(ExistBrand);
            var result = await _unitOfWork.CompleteAsync();
            if (result > 0)
                return Ok(new ApiResponse(200, "Brand Deleted Successfully"));
            return BadRequest(new ApiResponse(400));
        }

    }
}
