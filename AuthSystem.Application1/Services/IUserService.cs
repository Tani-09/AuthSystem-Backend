using AuthSystem.Application.DTOs;
using AuthSystem.Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthSystem.Application.Services
{
    public interface IUserService
    {
        Task Register(RegisterUserRequest request);
        Task<AuthResponse> Login(LogInRequest request);
        Task<AuthResponse> RefreshTokenAsync(string UserId, string refreshToken);
        Task<string> VerifyEmail(string token);
        Task ForgotPassword(string email);
        Task ResetPassword(ResetPasswordRequest request);
    }
}
