﻿using AutoMapper;
using FluentValidation;
using Onicorn.CRMApp.Business.Services.Interfaces;
using Onicorn.CRMApp.DataAccess.Repositories.Interfaces;
using Onicorn.CRMApp.DataAccess.UnitOfWork;
using Onicorn.CRMApp.Dtos.TaskDtos;
using Onicorn.CRMApp.Shared.Utilities.Response;
using Onicorn.CRMApp.Shared.Utilities.Services;
using Task = Onicorn.CRMApp.Entities.Task;

namespace Onicorn.CRMApp.Business.Services.Concrete
{
    public class TaskService : ITaskService
    {
        private readonly IUow _uow;
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;
        private readonly ISharedIdentityService _sharedIdentityService;
        private readonly IValidator<TaskCreateDto> _taskCreateDtoValidator;
        public TaskService(IUow uow, ITaskRepository taskRepository, IMapper mapper, ISharedIdentityService sharedIdentityService, IValidator<TaskCreateDto> taskCreateDtoValidator)
        {
            _uow = uow;
            _taskRepository = taskRepository;
            _mapper = mapper;
            _sharedIdentityService = sharedIdentityService;
            _taskCreateDtoValidator = taskCreateDtoValidator;
        }

        public async Task<CustomResponse<TaskDto>> GetTaskAsync(int taskId)
        {
            Task task = await _taskRepository.GetByFilterAsync(x => x.Id == taskId && x.Status == true);
            if (task != null)
            {
                TaskDto taskDto = _mapper.Map<TaskDto>(task);
                return CustomResponse<TaskDto>.Success(taskDto, ResponseStatusCode.OK);
            }
            return CustomResponse<TaskDto>.Fail("Task not found", ResponseStatusCode.NOT_FOUND);
        }

        public async Task<CustomResponse<IEnumerable<TasksDto>>> GetTasksAsync()
        {
            IEnumerable<TasksDto> tasksDtos = _mapper.Map<IEnumerable<TasksDto>>(await _taskRepository.GetAllFilterAsync(x => x.Status == true));
            return CustomResponse<IEnumerable<TasksDto>>.Success(tasksDtos, ResponseStatusCode.OK);
        }

        public async Task<CustomResponse<IEnumerable<TasksDto>>> GetTasksByUserAsync()
        {
            IEnumerable<TasksDto> tasksDtos = _mapper.Map<IEnumerable<TasksDto>>(await _taskRepository.GetAllFilterAsync(x => x.Status == true && x.AppUserId == _sharedIdentityService.GetUserId));
            return CustomResponse<IEnumerable<TasksDto>>.Success(tasksDtos, ResponseStatusCode.OK);
        }

        public async Task<CustomResponse<NoContent>> InsertTaskAsync(TaskCreateDto taskCreateDto)
        {
            var validationResult = _taskCreateDtoValidator.Validate(taskCreateDto);
            if (validationResult.IsValid)
            {
                Task task = _mapper.Map<Task>(taskCreateDto);
                task.Status = true;
                await _uow.GetRepository<Task>().InsertAsync(task);
                await _uow.SaveChangesAsync();
                return CustomResponse<NoContent>.Success(ResponseStatusCode.OK);
            }
            return CustomResponse<NoContent>.Fail(validationResult.Errors.Select(x => x.ErrorMessage).ToList(), ResponseStatusCode.BAD_REQUEST);
        }
    }
}
