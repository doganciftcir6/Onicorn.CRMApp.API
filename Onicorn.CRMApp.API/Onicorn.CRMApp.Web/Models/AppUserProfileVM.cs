﻿namespace Onicorn.CRMApp.Web.Models
{
    public class AppUserProfileVM
    {
        public int Id { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? ImageURL { get; set; }

        public string? Gender { get; set; }
    }
}