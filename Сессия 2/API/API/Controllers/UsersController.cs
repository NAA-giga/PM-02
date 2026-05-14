using API.Helpers;
using API.Models;
using API.Models.DTOs;
using API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.GetUserId(); // расширяющий метод из Helpers
            var profile = await _userRepository.GetUserProfileAsync(userId);
            if (profile == null)
                return NotFound(new ApiResponse<object> { IsSuccess = false, ErrorMessage = "Пользователь не найден" });

            return Ok(new ApiResponse<UserProfileDto> { IsSuccess = true, Data = profile });
        }
    }
}
