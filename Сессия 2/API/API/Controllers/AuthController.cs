using API.Models;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthController(IUserRepository userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Вход пользователя в систему
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new ApiResponse<object>
            {
                IsSuccess = false,
                ErrorMessage = "Не заполнены логин или пароль"
            });

        var user = await _userRepository.GetUserByUsernameAsync(request.Username);
        if (user == null)
            return Unauthorized(new ApiResponse<object>
            {
                IsSuccess = false,
                ErrorMessage = "Неверные учётные данные"
            });

        // Проверка пароля (временно прямое сравнение)
        bool passwordValid;
        try
        {
            passwordValid = (request.Password == user.PasswordHash);
        }
        catch
        {
            return Unauthorized(new ApiResponse<object>
            {
                IsSuccess = false,
                ErrorMessage = "Ошибка проверки пароля"
            });
        }

        if (!passwordValid)
            return Unauthorized(new ApiResponse<object>
            {
                IsSuccess = false,
                ErrorMessage = "Неверные учётные данные"
            });

        // Получаем имя роли для включения в токен
        var roleName = await _userRepository.GetRoleNameByUserIdAsync(user.Id);
        if (string.IsNullOrEmpty(roleName)) roleName = "User";

        // Генерируем JWT токен
        var token = _tokenService.GenerateToken(user.Username, user.Id, roleName);

        // Получаем полный профиль пользователя (включая фото в Base64)
        var profile = await _userRepository.GetUserProfileAsync(user.Id);
        if (profile == null)
        {
            // На случай, если профиль не собрался – создаём минимальный
            profile = new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                Email = user.Email,
                Role = roleName,
                Department = "",
                PhotoBase64 = null
            };
        }

        // Возвращаем токен и данные пользователя
        return Ok(new ApiResponse<object>
        {
            IsSuccess = true,
            Data = new
            {
                Token = token,
                User = profile
            }
        });
    }

    /// <summary>
    /// Регистрация нового пользователя (только для администратора)
    /// </summary>
    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new ApiResponse<object>
            {
                IsSuccess = false,
                ErrorMessage = "Логин и пароль обязательны"
            });

        var existing = await _userRepository.GetUserByUsernameAsync(request.Username);
        if (existing != null)
            return BadRequest(new ApiResponse<object>
            {
                IsSuccess = false,
                ErrorMessage = "Пользователь с таким логином уже существует"
            });

        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Email = request.Email,
            RoleId = request.RoleId,
            DepartmentId = request.DepartmentId,    
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var userId = await _userRepository.CreateUserAsync(user);
        return Ok(new ApiResponse<object>
        {
            IsSuccess = true,
            Data = new { UserId = userId }
        });
    }
}