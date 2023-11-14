﻿using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Onicorn.CRMApp.Business.Services.Interfaces;
using Onicorn.CRMApp.DataAccess.UnitOfWork;
using Onicorn.CRMApp.Dtos.AppUserDtos;
using Onicorn.CRMApp.Entities;
using Onicorn.CRMApp.Shared.Utilities.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onicorn.CRMApp.Business.Services.Concrete
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IValidator<AppUserRegisterDto> _AppUserRegisterDtoValidator;
        private readonly IValidator<AppUserLoginDto> _AppUserLoginDtoValidator;
        public AuthService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, SignInManager<AppUser> signInManager, IValidator<AppUserRegisterDto> appUserRegisterDtoValidator, IValidator<AppUserLoginDto> appUserLoginDtoValidator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _AppUserRegisterDtoValidator = appUserRegisterDtoValidator;
            _AppUserLoginDtoValidator = appUserLoginDtoValidator;
        }

        public async Task<CustomResponse<NoContent>> LoginAsync(AppUserLoginDto appUserLoginDto)
        {
            var validationResult = _AppUserLoginDtoValidator.Validate(appUserLoginDto);
            if (validationResult.IsValid)
            {
                var user = await _userManager.FindByNameAsync(appUserLoginDto.UserName);
                var signInResult = await _signInManager.PasswordSignInAsync(appUserLoginDto.UserName, appUserLoginDto.Password, appUserLoginDto.RememberMe, true);
                if (signInResult.Succeeded)
                {
                    return CustomResponse<NoContent>.Success(ResponseStatusCode.CREATED);
                }
                else if (signInResult.IsLockedOut)
                {
                    //ne zamana kadar kilitlendiğinin bilgisini almak yani locked zamanını ayarlamak.
                    //13.69 - 14.02 minutes
                    var lockOutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    var message = $"Your account has been suspended for {(lockOutEnd.Value.UtcDateTime - DateTime.UtcNow).Minutes} minutes.";
                    return CustomResponse<NoContent>.Fail(message, ResponseStatusCode.BAD_REQUEST);
                }
                else
                {
                    //kaç kere hatalı giriş yapıldığının bilgisini çekelim.
                    var message = string.Empty;

                    if (user != null)
                    {
                        var failedCount = await _userManager.GetAccessFailedCountAsync(user);
                        message = $"If you fail {_userManager.Options.Lockout.MaxFailedAccessAttempts - failedCount} more times, your account will be temporarily locked.";
                    }
                    else
                    {
                        message = "Invalid username or password!";
                    }
                    return CustomResponse<NoContent>.Fail(message, ResponseStatusCode.BAD_REQUEST);
                }
            }
            return CustomResponse<NoContent>.Fail(validationResult.Errors.Select(x => x.ErrorMessage).ToList(), ResponseStatusCode.BAD_REQUEST);
        }

        public async Task<CustomResponse<NoContent>> RegisterWithRoleAsync(AppUserRegisterDto appUserRegisterDto)
        {
            var validationResult = _AppUserRegisterDtoValidator.Validate(appUserRegisterDto);
            if (validationResult.IsValid)
            {
                AppUser appUser = new AppUser()
                {
                    Firstname = appUserRegisterDto.Firstname,
                    Lastname = appUserRegisterDto.Lastname,
                    Email = appUserRegisterDto.Email,
                    UserName = appUserRegisterDto.Username,
                    PhoneNumber = appUserRegisterDto.PhoneNumber,
                    ImageURL = appUserRegisterDto.ImageURL,
                    GenderId = appUserRegisterDto.GenderId,
                };
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
                    return CustomResponse<NoContent>.Success(ResponseStatusCode.CREATED);
                }
                return CustomResponse<NoContent>.Fail(registerResult.Errors.Select(x => x.Description).ToList(), ResponseStatusCode.BAD_REQUEST);
            }
            return CustomResponse<NoContent>.Fail(validationResult.Errors.Select(x => x.ErrorMessage).ToList(), ResponseStatusCode.BAD_REQUEST);
        }
    }
}
