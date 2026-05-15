using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Helpers;
using API.Models;
using API.Models.DTOs;
using API.Repositories;
using System.Text.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "technologist,admin")]
    public class ExtruderProgramsController : ControllerBase
    {
        private readonly IExtruderProgramRepository _programRepository;
        private readonly IExtruderTelemetryRepository _telemetryRepository;

        public ExtruderProgramsController(
            IExtruderProgramRepository programRepository,
            IExtruderTelemetryRepository telemetryRepository)
        {
            _programRepository = programRepository;
            _telemetryRepository = telemetryRepository;
        }

        /// <summary>
        /// Получить все программы экструдера
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var programs = await _programRepository.GetAllAsync();
            var dtos = programs.Select(p => new ExtruderProgramDto
            {
                Id = p.Id,
                Name = p.Name,
                Version = p.Version,
                ProductionBatchId = p.ProductionBatchId,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                ZoneParameters = string.IsNullOrEmpty(p.ZoneParams)
                    ? new Dictionary<int, ZoneParams>()
                    : JsonSerializer.Deserialize<Dictionary<int, ZoneParams>>(p.ZoneParams) ?? new()
            });
            return Ok(new ApiResponse<IEnumerable<ExtruderProgramDto>> { IsSuccess = true, Data = dtos });
        }

        /// <summary>
        /// Получить программу по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var program = await _programRepository.GetByIdAsync(id);
            if (program == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Программа не найдена" });
            var dto = new ExtruderProgramDto
            {
                Id = program.Id,
                Name = program.Name,
                Version = program.Version,
                ProductionBatchId = program.ProductionBatchId,
                Status = program.Status,
                CreatedAt = program.CreatedAt,
                ZoneParameters = string.IsNullOrEmpty(program.ZoneParams)
                    ? new Dictionary<int, ZoneParams>()
                    : JsonSerializer.Deserialize<Dictionary<int, ZoneParams>>(program.ZoneParams) ?? new()
            };
            return Ok(new ApiResponse<ExtruderProgramDto> { IsSuccess = true, Data = dto });
        }

        /// <summary>
        /// Создать новую программу экструдера
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExtruderProgramDto dto)
        {
            var userId = User.GetUserId();
            var id = await _programRepository.CreateAsync(dto, userId);
            return Ok(new ApiResponse<object> { IsSuccess = true, Data = new { Id = id } });
        }

        /// <summary>
        /// Обновить программу экструдера
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateExtruderProgramDto dto)
        {
            var existing = await _programRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Программа не найдена" });
            var userId = User.GetUserId();
            var success = await _programRepository.UpdateAsync(id, dto, userId);
            if (!success)
                return StatusCode(500, new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Ошибка обновления" });
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }

        /// <summary>
        /// Удалить программу экструдера
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _programRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Программа не найдена" });
            var success = await _programRepository.DeleteAsync(id);
            if (!success)
                return StatusCode(500, new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Ошибка удаления" });
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }

        /// <summary>
        /// Привязать программу к партии
        /// </summary>
        [HttpPost("{programId}/assign/{batchId}")]
        public async Task<IActionResult> AssignToBatch(int programId, int batchId)
        {
            var success = await _programRepository.AssignToBatchAsync(programId, batchId);
            if (!success)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Программа или партия не найдены" });
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }
    }
}