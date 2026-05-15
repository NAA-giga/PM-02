using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Models.Entities;
using API.Repositories;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExtruderTelemetryController : ControllerBase
    {
        private readonly IExtruderTelemetryRepository _telemetryRepository;

        public ExtruderTelemetryController(IExtruderTelemetryRepository telemetryRepository)
        {
            _telemetryRepository = telemetryRepository;
        }

        /// <summary>
        /// Получить телеметрию по ID партии
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetByBatchId([FromQuery] int batchId)
        {
            var telemetry = await _telemetryRepository.GetByBatchIdAsync(batchId);
            return Ok(new ApiResponse<IEnumerable<ExtruderTelemetry>> { IsSuccess = true, Data = telemetry });
        }

        /// <summary>
        /// Добавить запись телеметрии для партии
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "operator,technologist")]
        public async Task<IActionResult> AddTelemetry([FromBody] ExtruderTelemetry telemetry)
        {
            if (telemetry == null || telemetry.ProductionBatchId <= 0)
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Некорректные данные" });
            telemetry.RecordedAt = DateTime.UtcNow;
            var success = await _telemetryRepository.AddTelemetryAsync(telemetry);
            if (!success)
                return StatusCode(500, new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Ошибка сохранения" });
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }

        /// <summary>
        /// Добавить несколько записей телеметрии (пакетно)
        /// </summary>
        [HttpPost("batch")]
        [Authorize(Roles = "operator,technologist")]
        public async Task<IActionResult> AddTelemetryBatch([FromBody] List<ExtruderTelemetry> telemetryList)
        {
            if (telemetryList == null || !telemetryList.Any())
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Нет данных" });
            foreach (var t in telemetryList)
                t.RecordedAt = DateTime.UtcNow;
            var success = await _telemetryRepository.AddBatchAsync(telemetryList);
            if (!success)
                return StatusCode(500, new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Ошибка сохранения" });
            return Ok(new ApiResponse<object> { IsSuccess = true });
        }
    }
}