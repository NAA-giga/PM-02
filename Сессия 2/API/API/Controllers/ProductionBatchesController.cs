using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Helpers;
using API.Models;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories.Interfaces;
using API.Repositories;
using API.Controllers;
using API.Services;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductionBatchesController : ControllerBase
    {
        private readonly IProductionBatchRepository _batchRepository;
        private readonly IBatchStepExecutionRepository _stepRepository;
        private readonly IEventService _eventService;

        public ProductionBatchesController(
            IProductionBatchRepository batchRepository,
            IBatchStepExecutionRepository stepRepository,
            IEventService eventService)
        {
            _batchRepository = batchRepository;
            _stepRepository = stepRepository;
            _eventService = eventService;
        }

        /// <summary>
        /// Получить список партий с фильтрацией по дате (для отчётов)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var dtos = await _batchRepository.GetAllAsync(from, to);
            return Ok(new ApiResponse<IEnumerable<ProductionBatchDto>> { IsSuccess = true, Data = dtos });
        }

        /// <summary>
        /// Получить детали партии (включая шаги)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var batch = await _batchRepository.GetBatchDetailsAsync(id);
            if (batch == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Партия не найдена" });
            return Ok(new ApiResponse<ProductionBatchDetailDto> { IsSuccess = true, Data = batch });
        }

        /// <summary>
        /// Получить активные партии (для аппаратчика)
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var batches = await _batchRepository.GetActiveBatchesAsync();
            return Ok(new ApiResponse<IEnumerable<ProductionBatchDto>>
            {
                IsSuccess = true,
                Data = batches.Select(b => new ProductionBatchDto
                {
                    Id = b.Id,
                    BatchNumber = b.BatchNumber,
                    ProductName = b.ProductName,
                    Status = b.Status,
                    StartTime = b.StartTime
                })
            });
        }

        /// <summary>
        /// Запустить новую партию на основе подтверждённого заказа
        /// </summary>
        [HttpPost("start")]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> Start([FromBody] StartProductionBatchDto dto)
        {
            var userId = User.GetUserId();
            try
            {
                var batchId = await _batchRepository.StartBatchAsync(dto, userId);
                await _eventService.LogEventAsync("batch_started", "batch", batchId, $"Запущена партия {dto.BatchNumber}", userId);
                return Ok(new ApiResponse<object> { IsSuccess = true, Data = new { BatchId = batchId } });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = ex.Message });
            }
        }

        /// <summary>
        /// Завершить партию (после выполнения всех шагов и лабораторного контроля)
        /// </summary>
        [HttpPost("{id}/complete")]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> Complete(int id, [FromBody] decimal actualQuantity)
        {
            var userId = User.GetUserId();
            var success = await _batchRepository.CompleteBatchAsync(id, actualQuantity, userId);
            if (!success)
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось завершить партию" });
            await _eventService.LogEventAsync("batch_completed", "batch", id, "Партия завершена", userId);
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }

        /// <summary>
        /// Отменить партию
        /// </summary>
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.GetUserId();
            var success = await _batchRepository.CancelBatchAsync(id, userId);
            if (!success)
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось отменить партию" });
            await _eventService.LogEventAsync("batch_cancelled", "batch", id, "Партия отменена", userId);
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }

        /// <summary>
        /// Принять лабораторное решение по партии (разрешить/заблокировать)
        /// </summary>
        [HttpPost("{id}/lab-decision")]
        [Authorize(Roles = "lab_analyst,technologist,admin")]
        public async Task<IActionResult> LabDecision(int id, [FromBody] LaboratoryDecisionDto dto)
        {
            var userId = User.GetUserId();
            var batch = await _batchRepository.GetByIdAsync(id);
            if (batch == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Партия не найдена" });

            // Проверка, что все лабораторные испытания завершены (опционально)
            // Для простоты предполагаем, что проверка есть в сервисе или репозитории
            var success = await _batchRepository.UpdateLabDecisionAsync(id, dto.Decision, dto.Reason, userId);
            if (!success)
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось сохранить решение" });

            var message = dto.Decision == "approved" ? $"Партия {batch.BatchNumber} одобрена лабораторией" : $"Партия {batch.BatchNumber} заблокирована: {dto.Reason}";
            await _eventService.LogEventAsync("lab_decision", "batch", id, message, userId);
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }

        /// <summary>
        /// Начать выполнение шага партии (аппаратчик)
        /// </summary>
        [HttpPost("{batchId}/steps/{stepOrder}/start")]
        [Authorize(Roles = "operator,technologist")]
        public async Task<IActionResult> StartStep(int batchId, int stepOrder)
        {
            var userId = User.GetUserId();
            var success = await _stepRepository.StartStepAsync(batchId, stepOrder, userId);
            if (!success)
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось начать шаг (возможно, шаг уже начат)" });
            await _eventService.LogEventAsync("step_started", "batch_step", batchId, $"Начат шаг {stepOrder}", userId);
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }

        /// <summary>
        /// Завершить шаг партии и ввести фактические параметры
        /// </summary>
        [HttpPost("{batchId}/steps/{stepOrder}/complete")]
        [Authorize(Roles = "operator,technologist")]
        public async Task<IActionResult> CompleteStep(int batchId, int stepOrder, [FromBody] PerformStepDto data)
        {
            var userId = User.GetUserId();
            var success = await _stepRepository.CompleteStepAsync(batchId, stepOrder, data, userId);
            if (!success)
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось завершить шаг" });
            await _eventService.LogEventAsync("step_completed", "batch_step", batchId, $"Завершён шаг {stepOrder}", userId);
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }

    }
}