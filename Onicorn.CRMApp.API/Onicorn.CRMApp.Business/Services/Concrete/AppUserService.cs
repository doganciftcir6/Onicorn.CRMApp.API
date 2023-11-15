﻿using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Onicorn.CRMApp.Business.Helpers.UploadHelpers;
using Onicorn.CRMApp.Business.Services.Interfaces;
using Onicorn.CRMApp.Dtos.AppUserDtos;
using Onicorn.CRMApp.Dtos.TokenDtos;
using Onicorn.CRMApp.Entities;
using Onicorn.CRMApp.Shared.Utilities.Response;
using Onicorn.CRMApp.Shared.Utilities.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onicorn.CRMApp.Business.Services.Concrete
{
    public class AppUserService : IAppUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ISharedIdentityService _sharedIdentityService;
        private readonly IMapper _mapper;
        private readonly IValidator<UpdateAppUserDto> _updateAppUserValidator;
        private readonly IValidator<AppUserChangePasswordDto> _changePasswordValidator;
        private readonly IHostingEnvironment _hostingEnvironment;
        public AppUserService(UserManager<AppUser> userManager, ISharedIdentityService sharedIdentityService, IMapper mapper, IValidator<UpdateAppUserDto> updateAppUserValidator, IHostingEnvironment hostingEnvironment, IValidator<AppUserChangePasswordDto> changePasswordValidator)
        {
            _userManager = userManager;
            _sharedIdentityService = sharedIdentityService;
            _mapper = mapper;
            _updateAppUserValidator = updateAppUserValidator;
            _hostingEnvironment = hostingEnvironment;
            _changePasswordValidator = changePasswordValidator;
        }

        public async Task<CustomResponse<NoContent>> ChangePasswordAsync(AppUserChangePasswordDto appUserChangePassword)
        {
            var validationResult = _changePasswordValidator.Validate(appUserChangePassword);
            if (validationResult.IsValid)
            {
                var currentUser = await _userManager.FindByIdAsync(_sharedIdentityService.GetUserId.ToString());
                if (currentUser != null)
                {
                    var checkPassword = await _userManager.CheckPasswordAsync(currentUser, appUserChangePassword.CurrentPassword);
                    if (checkPassword)
                    {
                        await _userManager.ChangePasswordAsync(currentUser, appUserChangePassword.CurrentPassword, appUserChangePassword.NewPassword);
                        return CustomResponse<NoContent>.Success(ResponseStatusCode.OK);
                    }
                    return CustomResponse<NoContent>.Fail("Wrong current password!", ResponseStatusCode.BAD_REQUEST);
                }
                return CustomResponse<NoContent>.Fail("User not found", ResponseStatusCode.NOT_FOUND);
            }
            return CustomResponse<NoContent>.Fail(validationResult.Errors.Select(x => x.ErrorMessage).ToList(), ResponseStatusCode.BAD_REQUEST);
        }

        public async Task<CustomResponse<AppUserDto>> GetProfileAsync()
        {
            var userInfo = await _userManager.Users.Include(u => u.Gender).FirstOrDefaultAsync(u => u.Id == _sharedIdentityService.GetUserId);
            if (userInfo != null)
            {
                var appUserDto = _mapper.Map<AppUserDto>(userInfo);
                return CustomResponse<AppUserDto>.Success(appUserDto, ResponseStatusCode.OK);
            }
            return CustomResponse<AppUserDto>.Fail("User not found!", ResponseStatusCode.NOT_FOUND);
        }

        public async Task<CustomResponse<NoContent>> UpdateProfileAsync(UpdateAppUserDto updateAppUserDto, CancellationToken cancellationToken)
        {
            var validationResult = _updateAppUserValidator.Validate(updateAppUserDto);
            if (validationResult.IsValid)
            {
                var appUser = await _userManager.FindByIdAsync(_sharedIdentityService.GetUserId.ToString());
                var oldImage = appUser.ImageURL;
                if (appUser != null)
                {
                    _mapper.Map(updateAppUserDto, appUser);

                    if (updateAppUserDto.ImageURL != null && updateAppUserDto.ImageURL.Length > 0)
                    {
                        await AppUserImageUploadHelper.Run(_hostingEnvironment, updateAppUserDto.ImageURL, cancellationToken);
                        appUser.ImageURL = Path.GetFileNameWithoutExtension(updateAppUserDto.ImageURL.FileName) + Guid.NewGuid().ToString("N") + Path.GetExtension(updateAppUserDto.ImageURL.FileName);
                    }
                    else
                    {
                        appUser.ImageURL = oldImage;
                    }

                    var updateResult = await _userManager.UpdateAsync(appUser);

                    if (updateResult.Succeeded)
                    {
                        return CustomResponse<NoContent>.Success(ResponseStatusCode.OK);
                    }
                    return CustomResponse<NoContent>.Fail(updateResult.Errors.Select(x => x.Description).ToList(), ResponseStatusCode.BAD_REQUEST);
                }
                return CustomResponse<NoContent>.Fail("User not found!", ResponseStatusCode.NOT_FOUND);
            }
            return CustomResponse<NoContent>.Fail(validationResult.Errors.Select(x => x.ErrorMessage).ToList(), ResponseStatusCode.BAD_REQUEST);
        }
    }
}
