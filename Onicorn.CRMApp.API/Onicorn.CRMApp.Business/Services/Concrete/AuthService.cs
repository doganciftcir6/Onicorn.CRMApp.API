﻿using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Onicorn.CRMApp.Business.Helpers.Messages;
using Onicorn.CRMApp.Business.Helpers.UploadHelpers;
using Onicorn.CRMApp.Business.Services.Interfaces;
using Onicorn.CRMApp.Dtos.AppUserDtos;
using Onicorn.CRMApp.Dtos.TokenDtos;
using Onicorn.CRMApp.Entities;
using Onicorn.CRMApp.Shared.Utilities.Response;
using Onicorn.CRMApp.Shared.Utilities.Security.JWT;

namespace Onicorn.CRMApp.Business.Services.Concrete
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IValidator<AppUserRegisterDto> _AppUserRegisterDtoValidator;
        private readonly IValidator<AppUserLoginDto> _AppUserLoginDtoValidator;
        private readonly IMapper _mapper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        public AuthService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IValidator<AppUserRegisterDto> appUserRegisterDtoValidator, IValidator<AppUserLoginDto> appUserLoginDtoValidator, IMapper mapper, IHostingEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _AppUserRegisterDtoValidator = appUserRegisterDtoValidator;
            _AppUserLoginDtoValidator = appUserLoginDtoValidator;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }

        public async Task<CustomResponse<TokenResponseDto>> LoginAsync(AppUserLoginDto appUserLoginDto)
        {
            var validationResult = _AppUserLoginDtoValidator.Validate(appUserLoginDto);
            if (validationResult.IsValid)
            {
                AppUser user = await _userManager.FindByNameAsync(appUserLoginDto.UserName);
                if (user != null)
                {
                    var checkPassword = await _userManager.CheckPasswordAsync(user, appUserLoginDto.Password);
                    if (checkPassword)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        AppUserDto appUserDto = _mapper.Map<AppUserDto>(user);
                        var token = JwtTokenGenerator.GenerateToken(appUserDto, roles);
                        return CustomResponse<TokenResponseDto>.Success(token, ResponseStatusCode.OK);
                    }
                }
                return CustomResponse<TokenResponseDto>.Fail(
                    AppUserMessages.LOGİN_FAİLED, ResponseStatusCode.BAD_REQUEST);
            }
            return CustomResponse<TokenResponseDto>.Fail(validationResult.Errors.Select(x => x.ErrorMessage).ToList(), ResponseStatusCode.BAD_REQUEST);
        }

        public async Task<CustomResponse<string>> RegisterWithRoleAsync(AppUserRegisterDto appUserRegisterDto, CancellationToken cancellationToken)
        {
            var validationResult = _AppUserRegisterDtoValidator.Validate(appUserRegisterDto);
            if (validationResult.IsValid)
            {
                var appUser = _mapper.Map<AppUser>(appUserRegisterDto);
                if (appUserRegisterDto.ImageURL != null && appUserRegisterDto.ImageURL.Length > 0)
                {
                    string createdFileName = await AppUserImageUploadHelper.Run(_hostingEnvironment, appUserRegisterDto.ImageURL, _configuration, cancellationToken);
                    appUser.ImageURL = createdFileName;
                }

                var registerResult = await _userManager.CreateAsync(appUser, appUserRegisterDto.Password);
                if (registerResult.Succeeded)
                {
                    var memberRole = await _roleManager.FindByNameAsync("Member");
                    if (memberRole == null)
                    {
                        //eğer db'de Member rolü daha önce yoksa oluştursun
                        await _roleManager.CreateAsync(new()
                        {
                            Name = "Member",
                        });
                    }
                    //register olan kullanıcıya default member rolünü ekle
                    await _userManager.AddToRoleAsync(appUser, "Member");
                    return CustomResponse<string>.Success(AppUserMessages.SUCCESS_REGİSTER, ResponseStatusCode.CREATED);
                }
                return CustomResponse<string>.Fail(registerResult.Errors.Select(x => x.Description).ToList(), ResponseStatusCode.BAD_REQUEST);
            }
            return CustomResponse<string>.Fail(validationResult.Errors.Select(x => x.ErrorMessage).ToList(), ResponseStatusCode.BAD_REQUEST);
        }
    }
}
