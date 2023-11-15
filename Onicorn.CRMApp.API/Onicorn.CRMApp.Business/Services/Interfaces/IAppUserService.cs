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
        Task<CustomResponse<AppUserDto>> UpdateProfileAsync(UpdateAppUserDto updateAppUserDto);
    }
}