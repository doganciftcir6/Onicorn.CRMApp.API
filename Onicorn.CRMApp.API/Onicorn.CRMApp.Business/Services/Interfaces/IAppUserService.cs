﻿using Onicorn.CRMApp.Dtos.AppUserDtos;
using Onicorn.CRMApp.Shared.Utilities.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onicorn.CRMApp.Business.Services.Interfaces
{
    public interface IAppUserService
    {
        Task<CustomResponse<AppUserDto>> GetProfileAsync();
        Task<CustomResponse<IEnumerable<AppUserDto>>> GetAppUsersAsync();
        Task<CustomResponse<NoContent>> UpdateProfileAsync(UpdateAppUserDto updateAppUserDto, CancellationToken cancellationToken);
        Task<CustomResponse<NoContent>> ChangePasswordAsync(AppUserChangePasswordDto appUserChangePassword);
        Task<CustomResponse<List<RoleDto>>> GetRolesAsync(string userId);
        Task<CustomResponse<NoContent>> AssingRoleAsync(RoleAssingSendDto roleAssingSendDto);
    }
}
