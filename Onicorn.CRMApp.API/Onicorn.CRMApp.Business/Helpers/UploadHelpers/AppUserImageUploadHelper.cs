﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Onicorn.CRMApp.Dtos.AppUserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onicorn.CRMApp.Business.Helpers.UploadHelpers
{
    public static class AppUserImageUploadHelper
    {
        public static async Task<string> Run(IHostingEnvironment hostingEnvironment, IFormFile file, IConfiguration configuration, CancellationToken cancellationToken)
        {
            var fileName = Path.GetFileNameWithoutExtension(file.FileName) + Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
            string path = Path.Combine(hostingEnvironment.WebRootPath, "UserImages", fileName);
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
                stream.Close();
            }
            var apiUrl = configuration["ApiSettings:ApiUrl"];
            var fileUrl = $"{apiUrl}/UserImages/{fileName}";

            return fileUrl;
        }
    }
}
