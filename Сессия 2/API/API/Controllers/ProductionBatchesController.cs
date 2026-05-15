using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Helpers;
using API.Models;
using API.Models.DTOs;
using API.Repositories.Interfaces;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductionBatchesController : ControllerBase
    {
        private readonly IProductionBatchRepository _batchRepository;
        private readonly IBatchStepExecutionRepository _stepRepository;
        private readonly IEventRepository _eventRepository;

        public ProductionBatchesController(
            IProductionBatchRepository batchRepository,
            IBatchStepExecutionRepository stepRepository,
            IEventRepository eventRepository)
        {
            _batchRepository = batchRepository;
            _stepRepository = stepRepository;
            _eventRepository = eventRepository;
        }

        /// <summary>
        /// Получить список партий (с фильтрацией по дате)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var batches = await _batchRepository.GetAllAsync(from, to);
            var dtos = batches.Select(b => new ProductionBatchDto
            {
                Id = b.Id,
                BatchNumber = b.BatchNumber,
                OrderId = b.OrderId,
                ProductId = b.ProductId,
                RecipeId = b.RecipeId,
                TechCardId = b.TechCardId,
                Status = b.Status,
                PlannedQuantityKg = b.PlannedQuantityKg,
                ActualQuantityKg = b.ActualQuantityKg,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                LabDecision = b.LabDecision
            });
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
            return Ok(new ApiResponse<IEnumerable<ProductionBatchDto>> { IsSuccess = true, Data = batches.Select(b => new ProductionBatchDto { Id = b.Id, BatchNumber = b.BatchNumber, Status = b.Status }) });
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
                await _eventRepository.CreateEventAsync(new CreateEventDto
                {
                    EventType = "batch_started",
                    SourceType = "batch",
                    SourceId = batchId,
                    Message = $"Запущена партия {dto.BatchNumber}",
                    UserId = userId
                });
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
            await _eventRepository.CreateEventAsync(new CreateEventDto
            {
                EventType = "batch_completed",
                SourceType = "batch",
                SourceId = id,
                Message = "Партия завершена",
                UserId = userId
            });
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
            await _eventRepository.CreateEventAsync(new CreateEventDto
            {
                EventType = "batch_cancelled",
                SourceType = "batch",
                SourceId = id,
                Message = "Партия отменена",
                UserId = userId
            });
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
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось начать шаг" });
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
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }
    }
}