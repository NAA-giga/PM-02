using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Helpers;
using API.Models;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories.Interfaces;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TechCardsController : ControllerBase
    {
        private readonly ITechCardRepository _techCardRepository;
        private readonly IUserRepository _userRepository;

        public TechCardsController(ITechCardRepository techCardRepository, IUserRepository userRepository)
        {
            _techCardRepository = techCardRepository;
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? productId, [FromQuery] string? status)
        {
            var cards = await _techCardRepository.GetAllAsync(productId, status);
            var dtos = cards.Select(c => new TechCardDto
            {
                Id = c.Id,
                ProductId = c.ProductId,
                Version = c.Version,
                Name = c.Name,
                Description = c.Description,
                Status = c.Status,
                ApprovedAt = c.ApprovedAt,
                ApprovedBy = c.ApprovedBy,
                CreatedBy = c.CreatedBy,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                Steps = new List<TechStepDto>()
            });
            return Ok(new ApiResponse<IEnumerable<TechCardDto>> { IsSuccess = true, Data = dtos });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var card = await _techCardRepository.GetTechCardDetailsAsync(id);
            if (card == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Карта не найдена" });
            return Ok(new ApiResponse<TechCardDto> { IsSuccess = true, Data = card });
        }

        [HttpPost]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> Create([FromBody] CreateTechCardDto dto)
        {
            var userId = User.GetUserId();
            var maxVersion = await _techCardRepository.GetMaxVersionForProductAsync(dto.ProductId);
            var nextVersion = maxVersion + 1;
            var createDto = new CreateTechCardDto
            {
                ProductId = dto.ProductId,
                Version = nextVersion,
                Name = dto.Name,
                Description = dto.Description,
                Steps = dto.Steps
            };
            var id = await _techCardRepository.CreateAsync(createDto, userId);
            return Ok(new ApiResponse<object> { IsSuccess = true, Data = new { Id = id } });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTechCardDto dto)
        {
            var existing = await _techCardRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Карта не найдена" });
            if (existing.Status != "draft")
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Редактировать можно только черновик" });
            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.UpdatedAt = DateTime.UtcNow;
            var success = await _techCardRepository.UpdateAsync(existing);
            if (!success)
                return StatusCode(500, new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Ошибка обновления" });
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var userId = User.GetUserId();
            try
            {
                var success = await _techCardRepository.ApproveAsync(id, userId);
                if (!success)
                    return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось утвердить карту" });
                return Ok(new ApiResponse<object> { IsSuccess = true });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = ex.Message });
            }
        }

        [HttpPost("{id}/archive")]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> Archive(int id)
        {
            var userId = User.GetUserId();
            var success = await _techCardRepository.ArchiveAsync(id, userId);
            if (!success)
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось архивировать карту" });
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }

        [HttpPost("{cardId}/steps")]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> AddStep(int cardId, [FromBody] CreateTechStepDto dto)
        {
            var card = await _techCardRepository.GetByIdAsync(cardId);
            if (card == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Карта не найдена" });
            if (card.Status != "draft")
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Шаги можно добавлять только в черновик" });
            var step = new TechStep
            {
                TechCardId = cardId,
                StepOrder = dto.StepOrder,
                StepName = dto.StepName,
                StepType = dto.StepType,
                EquipmentId = dto.EquipmentId,
                PlannedTempC = dto.PlannedTempC,
                PlannedPressureBar = dto.PlannedPressureBar,
                PlannedDurationMin = dto.PlannedDurationMin,
                PlannedSpeedRpm = dto.PlannedSpeedRpm,
                TempToleranceMin = dto.TempToleranceMin ?? 0,
                TempToleranceMax = dto.TempToleranceMax ?? 0,
                PressureToleranceMin = dto.PressureToleranceMin ?? 0,
                PressureToleranceMax = dto.PressureToleranceMax ?? 0,
                IsMandatory = dto.IsMandatory,
                Instruction = dto.Instruction,
                CreatedAt = DateTime.UtcNow
            };
            var success = await _techCardRepository.AddStepAsync(step);
            if (!success)
                return StatusCode(500, new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Ошибка добавления шага" });
            return Ok(new ApiResponse<object> { IsSuccess = true, Data = new { StepId = step.Id } });
        }

        [HttpPut("steps/{stepId}")]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> UpdateStep(int stepId, [FromBody] CreateTechStepDto dto)
        {
            var step = await _techCardRepository.GetStepByIdAsync(stepId);
            if (step == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Шаг не найден" });
            var card = await _techCardRepository.GetByIdAsync(step.TechCardId);
            if (card == null || card.Status != "draft")
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Шаги можно изменять только в черновике" });
            step.StepOrder = dto.StepOrder;
            step.StepName = dto.StepName;
            step.StepType = dto.StepType;
            step.EquipmentId = dto.EquipmentId;
            step.PlannedTempC = dto.PlannedTempC;
            step.PlannedPressureBar = dto.PlannedPressureBar;
            step.PlannedDurationMin = dto.PlannedDurationMin;
            step.PlannedSpeedRpm = dto.PlannedSpeedRpm;
            step.TempToleranceMin = dto.TempToleranceMin ?? 0;
            step.TempToleranceMax = dto.TempToleranceMax ?? 0;
            step.PressureToleranceMin = dto.PressureToleranceMin ?? 0;
            step.PressureToleranceMax = dto.PressureToleranceMax ?? 0;
            step.IsMandatory = dto.IsMandatory;
            step.Instruction = dto.Instruction;
            var success = await _techCardRepository.UpdateStepAsync(step);
            if (!success)
                return StatusCode(500, new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Ошибка обновления шага" });
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }

        [HttpDelete("steps/{stepId}")]
        [Authorize(Roles = "technologist,admin")]
        public async Task<IActionResult> DeleteStep(int stepId)
        {
            var step = await _techCardRepository.GetStepByIdAsync(stepId);
            if (step == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Шаг не найден" });
            var card = await _techCardRepository.GetByIdAsync(step.TechCardId);
            if (card == null || card.Status != "draft")
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Шаги можно удалять только в черновике" });
            var success = await _techCardRepository.DeleteStepAsync(stepId);
            if (!success)
                return StatusCode(500, new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Ошибка удаления шага" });
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }
    }
}