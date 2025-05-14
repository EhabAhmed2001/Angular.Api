using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.PL.DTOs;
using Talabat.PL.Errors;
using Talabat.PL.Helper;

namespace Talabat.PL.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TypeController : APIBaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public TypeController(IMapper mapper, IUnitOfWork UnitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = UnitOfWork;
        }

        //Get All Types
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductType>>> GetAllTypes()
        {
            var types = await _unitOfWork.Repository<ProductType>().GetAllAsync();
            return Ok(types);
        }

        //Get Type By Id
        [HttpGet("{Id}")]
        public async Task<ActionResult<ProductType>> GetTypeById(int Id)
        {
            var type = await _unitOfWork.Repository<ProductType>().GetByIdAsync(Id);
            if (type is null)
                return NotFound(new ApiResponse(404));
            return Ok(type);
        }

        //Add Type
        [HttpPost]
        public async Task<ActionResult<TypeToReturnDto>> AddType([FromForm] TypeDto type)
        {
            if (type is null)
                return BadRequest(new ApiResponse(400));

            var ImagePath = AddPicFile.AddPic(type.Picture, "Types");
            var newType = _mapper.Map<TypeDto, ProductType>(type);
            newType.Image = ImagePath;

            await _unitOfWork.Repository<ProductType>().AddAsync(newType);
            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
                return BadRequest(new ApiResponse(400));

            var returnedType = _mapper.Map<ProductType, TypeToReturnDto>(newType);
            return Ok(returnedType);
        }

        //Update Type
        [HttpPut("{Id}")]
        public async Task<ActionResult<TypeToReturnDto>> UpdateType(int Id,[FromForm] TypeDto type)
        {
            if (type is null)
                return BadRequest(new ApiResponse(400));

            var ExistType = await _unitOfWork.Repository<ProductType>().GetByIdAsync(Id);
            if (ExistType is null)
                return NotFound(new ApiResponse(404));

            var OldImage= ExistType.Image;
            var NewImage = AddPicFile.AddPic(type.Picture, "Types");

            _mapper.Map(type, ExistType);
            ExistType.Image = NewImage;

            var result = await _unitOfWork.CompleteAsync();
            if (result <= 0)
                return BadRequest(new ApiResponse(400));

            AddPicFile.DeletePic(OldImage);
            var returnedType = _mapper.Map<ProductType, TypeToReturnDto>(ExistType);
            return Ok(returnedType);
        }

        //Delete Type
        [HttpDelete("{Id}")]
        public async Task<ActionResult<ProductType>> DeleteType(int Id)
        {
            var ExistType = await _unitOfWork.Repository<ProductType>().GetByIdAsync(Id);
            if (ExistType is null)
                return NotFound(new ApiResponse(404));

            _unitOfWork.Repository<ProductType>().Delete(ExistType);
            var result = await _unitOfWork.CompleteAsync();

            if (result > 0)
            {
                // Delete Old Image
                AddPicFile.DeletePic(ExistType.Image);
                return Ok(new ApiResponse(200, "Type Deleted Successfully"));
            }
            return BadRequest(new ApiResponse(400));
        }
    }
}
