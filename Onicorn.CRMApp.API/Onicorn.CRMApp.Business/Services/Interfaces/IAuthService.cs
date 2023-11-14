﻿using Onicorn.CRMApp.Dtos.AppUserDtos;
using Onicorn.CRMApp.Shared.Utilities.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onicorn.CRMApp.Business.Services.Interfaces
{
    public interface IAuthService
    {
        Task<CustomResponse<string>> RegisterWithRoleAsync(AppUserRegisterDto appUserRegisterDto);
        Task<CustomResponse<NoContent>> LoginAsync(AppUserLoginDto appUserLoginDto);
    }
}
