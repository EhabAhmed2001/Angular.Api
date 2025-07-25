﻿using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Services;
using Talabat.PL.DTOs;
using Talabat.PL.Errors;
using Talabat.PL.Extensions;

namespace Talabat.PL.Controllers
{
    public class AccountsController : APIBaseController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _token;
        private readonly IMapper _mapper;

        public AccountsController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService token,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _token = token;
            _mapper = mapper;
        }

        // Register



        [HttpPost("Register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto, string? Role = "Customer")
        {

            if (CheckIfUserExist(registerDto.Email).Result.Value)
                return BadRequest(new ApiResponse(400, "This Email Is Already Exist"));

            var user = new AppUser()
            {
                DisplayNams = registerDto.DisplayName,
                Email = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                UserName = registerDto.Email.Split('@')[0],
                Address = new Address()
                {
                    Street = registerDto.Street,
                    City = registerDto.City,
                    Country = registerDto.Country,
                    FirstName = registerDto.DisplayName.Split()[0],
                    LastName = registerDto.DisplayName.Split()[1]
                }
            };
            var Result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!Result.Succeeded) return BadRequest(new ApiResponse(400, Result.Errors.FirstOrDefault().Description));

            if (Role == "Customer")
            {
                Result = await _userManager.AddToRoleAsync(user, "Customer");
            }
            else if (Role == "Admin")
            {
                Result = await _userManager.AddToRoleAsync(user, "Admin");
            }
            else
            {
                return BadRequest(new ApiResponse(400, "This Role Is Not Exist"));
            }


            var ReturnedUser = new UserDto()
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                Role = Role,
                Token = await _token.CreateTokenAsync(user)
            };

            return Ok(ReturnedUser);

        }


        // Login

        [HttpPost("Login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var User = await _userManager.FindByEmailAsync(loginDto.Email);
            if (User is null) return Unauthorized(new ApiResponse(401, "User Not Found"));

            var Result = await _signInManager.CheckPasswordSignInAsync(User, loginDto.Password, false);
            if (!Result.Succeeded) return Unauthorized(new ApiResponse(401, "Password Is Wrong"));

            var UserRoles = await _userManager.GetRolesAsync(User);
            if (UserRoles.Count == 0) return Unauthorized(new ApiResponse(401, "User Not Found"));


            return Ok(new UserDto()
            {
                Email = User.Email,
                DisplayName = User.DisplayNams,
                Role = UserRoles[0],
                Token = await _token.CreateTokenAsync(User)
            });

        }


        // Get Current User
        [Authorize]
        [HttpGet("GetCurrentUser")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var Email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(Email);
            if (user is null) return Unauthorized(new ApiResponse(401, "User Not Found"));
            var UserRoles = await _userManager.GetRolesAsync(user);

            var MappedUser = new UserDto()
            {
                Email = user.Email,
                DisplayName = user.DisplayNams,
                Role = UserRoles[0],
                Token = await _token.CreateTokenAsync(user),
                PhoneNumber = user.PhoneNumber,


            };

            return Ok(MappedUser);
        }


        // Get User Address
        [Authorize]
        [HttpGet("Address")]
        public async Task<ActionResult<AddressDto>> GetUserAddress()
        {
            var user = await _userManager.GetUserWithAddressAsync(User);

            var MappedAddress = _mapper.Map<Address, AddressDto>(user.Address);

            return Ok(MappedAddress);
        }


        // Update User Address
        [Authorize]
        [HttpPut("UpdateUserAddress")]
        public async Task<ActionResult<AddressDto>> UpdateUserAddress(AddressDto UpdatedAddress)
        {
            var user = await _userManager.GetUserWithAddressAsync(User);
            var MappedAddress = _mapper.Map<AddressDto, Address>(UpdatedAddress);
            MappedAddress.Id = user.Address.Id;
            user.Address = MappedAddress;
            var Result = await _userManager.UpdateAsync(user);
            if (!Result.Succeeded) return BadRequest(new ApiResponse(400));
            return Ok(UpdatedAddress);

        }


        // Ckeck If User Is Exist Or Not
        [HttpGet("IsUserExist")]
        public async Task<ActionResult<bool>> CheckIfUserExist(string Email)
        {
            return await _userManager.FindByEmailAsync(Email) is not null;
        }

    }
}
