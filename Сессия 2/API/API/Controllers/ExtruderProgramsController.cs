using API.Helpers;
using API.Models;
using API.Models.DTOs;
using API.Repositories;
using API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Technologist,Admin")]
    public class ExtruderProgramsController : ControllerBase
    {
        private readonly IExtruderProgramRepository _repository;

        public ExtruderProgramsController(IExtruderProgramRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Получить список всех программ экструдера
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var programs = await _repository.GetAllAsync();
            return Ok(new ApiResponse<IEnumerable<ExtruderProgramDto>>
            {
                IsSuccess = true,
                Data = programs.Select(p => new ExtruderProgramDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Version = p.Version,
                    ProductionBatchId = p.ProductionBatchId,
                    Status = p.Status,
                    ZoneParameters = !string.IsNullOrEmpty(p.ZoneParams)
                        ? JsonSerializer.Deserialize<Dictionary<int, ZoneParams>>(p.ZoneParams) ?? new Dictionary<int, ZoneParams>()
                        : new Dictionary<int, ZoneParams>()
                })
            });
        }

        /// <summary>
        /// Получить программу экструдера по ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var program = await _repository.GetByIdAsync(id);
            if (program == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Программа не найдена" });

            var dto = new ExtruderProgramDto
            {
                Id = program.Id,
                Name = program.Name,
                Version = program.Version,
                ProductionBatchId = program.ProductionBatchId,
                Status = program.Status,
                ZoneParameters = !string.IsNullOrEmpty(program.ZoneParams)
                    ? JsonSerializer.Deserialize<Dictionary<int, ZoneParams>>(program.ZoneParams) ?? new Dictionary<int, ZoneParams>()
                    : new Dictionary<int, ZoneParams>()
            };

            return Ok(new ApiResponse<ExtruderProgramDto> { IsSuccess = true, Data = dto });
        }

        /// <summary>
        /// Создать новую программу экструдера
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ExtruderProgramDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Название программы обязательно" });

            var userId = User.GetUserId();
            var id = await _repository.CreateAsync(dto, userId);
            return Ok(new ApiResponse<object> { IsSuccess = true, Data = new { Id = id } });
        }

        /// <summary>
        /// Обновить существующую программу экструдера
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ExtruderProgramDto dto)
        {
            if (dto == null || id != (dto.Id ?? 0))
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Несоответствие ID программы" });

            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Программа не найдена" });

            var userId = User.GetUserId();
            var success = await _repository.UpdateAsync(id, dto, userId);
            if (!success)
                return StatusCode(500, new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Ошибка при обновлении программы" });

            return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Программа обновлена" });
        }

        /// <summary>
        /// Удалить программу экструдера
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Программа не найдена" });

            var success = await _repository.DeleteAsync(id);
            if (!success)
                return StatusCode(500, new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Ошибка при удалении программы" });

            return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Программа удалена" });
        }
    }
}