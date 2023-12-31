﻿using Microsoft.AspNetCore.Http;

namespace Onicorn.CRMApp.Dtos.AppUserDtos
{
    public class AppUserRegisterDto
    {
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public IFormFile? ImageURL { get; set; }

        public int GenderId { get; set; }
    }
}
