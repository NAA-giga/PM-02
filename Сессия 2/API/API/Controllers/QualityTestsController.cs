using API.Helpers;
using API.Models;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QualityTestsController : ControllerBase
{
    private readonly IQualityTestRepository _testRepository;

    public QualityTestsController(IQualityTestRepository testRepository)
    {
        _testRepository = testRepository;
    }

    [HttpGet("batch/{batchId}")]
    public async Task<IActionResult> GetByBatch(int batchId)
    {
        var tests = await _testRepository.GetTestsByBatchIdAsync(batchId);
        return Ok(new ApiResponse<IEnumerable<QualityTest>> { IsSuccess = true, Data = tests });
    }

    [HttpGet("{testId}")]
    public async Task<IActionResult> GetById(int testId)
    {
        var test = await _testRepository.GetTestWithResultsAsync(testId);
        if (test == null)
            return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Испытание не найдено" });
        return Ok(new ApiResponse<QualityTest> { IsSuccess = true, Data = test });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQualityTestDto dto)
    {
        var userId = User.GetUserId();
        var testId = await _testRepository.CreateTestAsync(dto, userId);
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = new { TestId = testId } });
    }

    [HttpPost("{testId}/results")]
    public async Task<IActionResult> EnterResults(int testId, [FromBody] EnterTestResultDto dto)
    {
        if (dto.TestId != testId)
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Несоответствие ID испытания" });

        var userId = User.GetUserId();
        var success = await _testRepository.EnterResultsAsync(dto, userId);
        if (!success)
            return StatusCode(500, new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Ошибка сохранения результатов" });
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Результаты сохранены" });
    }

    [HttpPost("{testId}/complete")]
    public async Task<IActionResult> Complete(int testId)
    {
        var success = await _testRepository.CompleteTestAsync(testId);
        if (!success)
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось завершить испытание" });
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Испытание завершено" });
    }
}