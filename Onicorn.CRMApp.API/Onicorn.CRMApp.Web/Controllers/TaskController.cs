﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Onicorn.CRMApp.Shared.Utilities.Response;
using Onicorn.CRMApp.Web.Models;
using System.Net;
using System.Text.Json;

namespace Onicorn.CRMApp.Web.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public TaskController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var token = User.Claims.FirstOrDefault(x => x.Type == "accessToken")?.Value;
            if (token != null)
            {
                var _httpClient = _httpClientFactory.CreateClient("MyApiClient");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("Task/GetTasks");
                if (response.IsSuccessStatusCode)
                {
                    CustomResponse<IEnumerable<TasksVM>> tasksResponse = await response.Content.ReadFromJsonAsync<CustomResponse<IEnumerable<TasksVM>>>();
                    return View(tasksResponse.Data);
                }
            }
            return View();
        }

        public async Task<IActionResult> MyTasks()
        {
            var token = User.Claims.FirstOrDefault(x => x.Type == "accessToken")?.Value;
            if (token != null)
            {
                var _httpClient = _httpClientFactory.CreateClient("MyApiClient");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("Task/GetTasksByUser");
                if (response.IsSuccessStatusCode)
                {
                    CustomResponse<IEnumerable<TasksVM>> tasksResponse = await response.Content.ReadFromJsonAsync<CustomResponse<IEnumerable<TasksVM>>>();
                    return View(tasksResponse.Data);
                }
            }
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> InsertTask()
        {
            TaskCreateInput taskCreateInput = new TaskCreateInput();
            var token = User.Claims.FirstOrDefault(x => x.Type == "accessToken")?.Value;
            if (token != null)
            {
                var _httpClient = _httpClientFactory.CreateClient("MyApiClient");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var taskSituationsResponse = await _httpClient.GetAsync("TaskSituation/GetTaskSituations");
                var appUsersResponse = await _httpClient.GetAsync("AppUser/GetAppUsers");
                if (taskSituationsResponse.IsSuccessStatusCode && appUsersResponse.IsSuccessStatusCode)
                {
                    CustomResponse<IEnumerable<TaskSituationVM>> taskSituations = await taskSituationsResponse.Content.ReadFromJsonAsync<CustomResponse<IEnumerable<TaskSituationVM>>>();
                    CustomResponse<IEnumerable<AppUserProfileVM>> customers = await appUsersResponse.Content.ReadFromJsonAsync<CustomResponse<IEnumerable<AppUserProfileVM>>>();

                    taskCreateInput.AppUsers = new SelectList(customers.Data, "Id", "Fullname");
                    taskCreateInput.TaskSituations = new SelectList(taskSituations.Data, "Id", "Definition");
                }
            }
            return View(taskCreateInput);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> InsertTask(TaskCreateInput taskCreateInput)
        {
            if (ModelState.IsValid)
            {
                var token = User.Claims.FirstOrDefault(x => x.Type == "accessToken")?.Value;
                if (token != null)
                {
                    var _httpClient = _httpClientFactory.CreateClient("MyApiClient");
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    var response = await _httpClient.PostAsJsonAsync("Task/InsertTask", taskCreateInput);
                    if (response is null || response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        ModelState.AddModelError("", "Server error. Please try again later.");
                        return View(taskCreateInput);
                    }

                    CustomResponse<NoContent> taskCreateResponse = await response.Content.ReadFromJsonAsync<CustomResponse<NoContent>>();
                    if (taskCreateResponse.IsSuccessful)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    foreach (var error in taskCreateResponse.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }
            }
            var appUsersData = TempData["appUsers"]?.ToString();
            var taskSituationsData = TempData["taskSituations"]?.ToString();
            if (appUsersData != null || taskSituationsData != null)
            {
                var appUsers = JsonSerializer.Deserialize<List<SelectListItem>>(appUsersData);
                var taskSituations = JsonSerializer.Deserialize<List<SelectListItem>>(taskSituationsData);
                taskCreateInput.AppUsers = new SelectList(appUsers, "Value", "Text");
                taskCreateInput.TaskSituations = new SelectList(taskSituations, "Value", "Text");
            }
            return View(taskCreateInput);
        }
    }
}
