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
public class TechCardsController : ControllerBase
{
    private readonly ITechCardRepository _techCardRepository;

    public TechCardsController(ITechCardRepository techCardRepository)
    {
        _techCardRepository = techCardRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var cards = await _techCardRepository.GetAllAsync();
        return Ok(new ApiResponse<IEnumerable<TechCard>> { IsSuccess = true, Data = cards });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var card = await _techCardRepository.GetDetailsAsync(id);
        if (card == null)
            return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Технологическая карта не найдена" });
        return Ok(new ApiResponse<TechCardResponseDto> { IsSuccess = true, Data = card });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTechCardDto dto)
    {
        var userId = User.GetUserId();
        var cardId = await _techCardRepository.CreateAsync(dto, userId);
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = new { TechCardId = cardId } });
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var userId = User.GetUserId();
        try
        {
            var success = await _techCardRepository.ApproveAsync(id, userId);
            if (!success)
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось утвердить карту" });
            return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Карта утверждена" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = ex.Message });
        }
    }

    [HttpPost("{id}/archive")]
    public async Task<IActionResult> Archive(int id)
    {
        var userId = User.GetUserId();
        var success = await _techCardRepository.ArchiveAsync(id, userId);
        if (!success)
            return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Не удалось архивировать карту" });
        return Ok(new ApiResponse<object> { IsSuccess = true, Data = "Карта заархивирована" });
    }
}