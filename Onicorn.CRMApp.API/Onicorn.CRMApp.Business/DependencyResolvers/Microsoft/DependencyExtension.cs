﻿using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Onicorn.CRMApp.Business.CustomDescriber;
using Onicorn.CRMApp.Business.Services.Concrete;
using Onicorn.CRMApp.Business.Services.Interfaces;
using Onicorn.CRMApp.Business.ValidationRules.FluentValidation.AppUserValidations;
using Onicorn.CRMApp.Business.ValidationRules.FluentValidation.CommunicationValidations;
using Onicorn.CRMApp.Business.ValidationRules.FluentValidation.CustomerValidations;
using Onicorn.CRMApp.Business.ValidationRules.FluentValidation.ProjectValidations;
using Onicorn.CRMApp.Business.ValidationRules.FluentValidation.SaleValidations;
using Onicorn.CRMApp.Business.ValidationRules.FluentValidation.TaskValidations;
using Onicorn.CRMApp.DataAccess.Contexts.EntityFramework;
using Onicorn.CRMApp.DataAccess.Repositories.Concrete;
using Onicorn.CRMApp.DataAccess.Repositories.Interfaces;
using Onicorn.CRMApp.DataAccess.UnitOfWork;
using Onicorn.CRMApp.Dtos.AppUserDtos;
using Onicorn.CRMApp.Dtos.CommunicationDtos;
using Onicorn.CRMApp.Dtos.CustomerDtos;
using Onicorn.CRMApp.Dtos.ProjectDtos;
using Onicorn.CRMApp.Dtos.SaleDtos;
using Onicorn.CRMApp.Dtos.TaskDtos;
using Onicorn.CRMApp.Entities;
using Onicorn.CRMApp.Shared.Utilities.Security.JWT;
using Onicorn.CRMApp.Shared.Utilities.Services;
using System.Reflection;
using System.Text;

namespace Onicorn.CRMApp.Business.DependencyResolvers.Microsoft
{
    public static class DependencyExtension
    {
        public static void AddDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            //Identity
            services.AddIdentity<AppUser, AppRole>(opt =>
            {
                opt.Password.RequireDigit = true;
                opt.Password.RequiredLength = 1;
                opt.Password.RequireUppercase = true;
                opt.Password.RequireLowercase = true;
                opt.Password.RequireNonAlphanumeric = true;
                opt.Lockout.MaxFailedAccessAttempts = 5;
            }).AddErrorDescriber<CustomErrorDescriber>().AddEntityFrameworkStores<AppDbContext>();

            //Context
            services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseSqlServer(configuration.GetConnectionString("SqlServerConnection"));
            });

            //Scopes,Singletons,Transients
            services.AddScoped<IUow, Uow>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<ISharedIdentityService, SharedIdentityService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAppUserService, AppUserService>();
            services.AddScoped<ICommunicationService, CommunicationService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<ISaleService, SaleService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IGenderService, GenderService>();
            services.AddScoped<ICommunicationTypeService, CommunicationTypeService>();
            services.AddScoped<ISaleSituationService, SaleSituationService>();
            services.AddScoped<ITaskSituationService, TaskSituationService>();
            services.AddScoped<IStatisticService, StatisticService>();

            services.AddScoped<ICommunicationRepository, CommunicationRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<ISaleRepository, SaleRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            //FluentValidations
            services.AddScoped<IValidator<AppUserRegisterDto>, AppUserRegisterDtoValidator>();
            services.AddScoped<IValidator<AppUserLoginDto>, AppUserLoginDtoValidator>();
            services.AddScoped<IValidator<UpdateAppUserDto>, UpdateAppUserDtoValidator>();
            services.AddScoped<IValidator<AppUserChangePasswordDto>, AppUserChangePasswordDtoValidator>();
            services.AddScoped<IValidator<RoleAssingSendDto>, RoleAssingSendDtoValidator>();
            services.AddScoped<IValidator<CommunicationCreateDto>, CommunicationCreateDtoValidator>();
            services.AddScoped<IValidator<CommunicationUpdateDto>, CommunicationUpdateDtoValidator>();
            services.AddScoped<IValidator<CustomerCreateDto>, CustomerCreateDtoValidator>();
            services.AddScoped<IValidator<CustomerUpdateDto>, CustomerUpdateDtoValidator>();
            services.AddScoped<IValidator<ProjectCreateDto>, ProjectCreateDtoValidator>();
            services.AddScoped<IValidator<ProjectUpdateDto>, ProjectUpdateDtoValidator>();
            services.AddScoped<IValidator<SaleCreateDto>, SaleCreateDtoValidator>();
            services.AddScoped<IValidator<SaleUpdateDto>, SaleUpdateDtoValidator>();
            services.AddScoped<IValidator<TaskCreateDto>, TaskCreateDtoValidator>();
            services.AddScoped<IValidator<TaskUpdateDto>, TaskUpdateDtoValidator>();
            //AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            //JWT register (Auto şema)
            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(opt =>
            {
                opt.RequireHttpsMetadata = false;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = JwtTokenDefaults.ValidIssuer,
                    ValidAudience = JwtTokenDefaults.ValidAudience,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtTokenDefaults.Key)),
                };
            });

        }
    }
}
