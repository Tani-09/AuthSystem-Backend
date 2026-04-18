using AuthSystem.Application.DTOs;
using AuthSystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace AuthSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            await _userService.ForgotPassword(email);
            return Ok("Reset link sent (check logs for now)");
        }


        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            var result = await _userService.VerifyEmail(token);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-test")]
        public IActionResult AdminTest()
        {
            return Ok("Admin access granted");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserRequest request)
        {
            await _userService.Register(request);

            return Ok("User registered");


        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LogInRequest request)
        {
            var result = await _userService.Login(request);

            return Ok(result);
        }



        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(string UserId, RefreshTokenRequest request)
        {
            var result = await _userService.RefreshTokenAsync(
                request.UserId,
                request.RefreshToken
            );

            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Headers["Authorization"]
                .ToString()
                .Replace("Bearer ", "");

            await _userService.LogoutAsync(token);

            return Ok("Logged out successfully");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            await _userService.ResetPassword(request);
            return Ok("Password reset successful");
        }
    }
}
