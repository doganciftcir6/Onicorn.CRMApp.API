﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Onicorn.CRMApp.Business.Services.Interfaces;
using Onicorn.CRMApp.Shared.ControllerBases;

namespace Onicorn.CRMApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : CustomBaseController
    {
        private readonly ITaskService _taskService;
        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetTasks()
        {
            return CreateActionResultInstance(await _taskService.GetTasks());
        }
    }
}