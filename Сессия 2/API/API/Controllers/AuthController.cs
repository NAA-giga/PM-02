using API.Models;
using API.Models.DTOs;
using API.Models.Entities;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
namespace API.Controllers
{
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
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            // ВРЕМЕННО: для тестирования JWT и API
            if (request.Username == "admin" && request.Password == "admin")
            {
                var token = _tokenService.GenerateToken("admin", 1, "Admin");
                return Ok(new ApiResponse<object> { IsSuccess = true, Data = new { Token = token } });
            }
                
            // Оригинальная проверка (закомментирована до создания реального пользователя в БД)
            // var user = await _userRepository.GetUserByUsernameAsync(request.Username);
            // if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            //     return Unauthorized(...);
            // var token = _tokenService.GenerateToken(user.Username, user.Id, "Operator");

            return Unauthorized(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Неверные учётные данные" });
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            // Проверка существования пользователя
            var existing = await _userRepository.GetUserByUsernameAsync(request.Username);
            if (existing != null)
                return BadRequest(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Учетная запись уже существует" });

            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FullName = request.FullName,
                Email = request.Email,
                RoleId = request.RoleId,          // обязательно
                DepartmentId = request.DepartmentId, // обязательно
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var userId = await _userRepository.CreateUserAsync(user);
            return Ok(new ApiResponse<object> { IsSuccess = true, Data = new { UserId = userId } });
        }
    }
}
    
