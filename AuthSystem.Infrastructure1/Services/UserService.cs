using AuthSystem.Application.DTOs;
using AuthSystem.Application.Responses;
using AuthSystem.Application.Services;
using AuthSystem.Domain.Entities;
using AuthSystem.Infrastructure.Data;
using AuthSystem.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AuthSystem.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AuthDbContext _context;
        private readonly PasswordHasherService _hasher;
        private readonly ITokenService _tokenService;
        private readonly ILogger<UserService> _logger;
        private readonly RedisService _redis;
        private readonly EmailService _emailService;



        public UserService(AuthDbContext context, PasswordHasherService hasher, ITokenService tokenService, ILogger<UserService> logger, RedisService redis, EmailService emailService)
        {
            _context = context;
            _hasher = hasher;
            _tokenService = tokenService;
            _logger = logger;
            _redis = redis;
            _emailService = emailService;
        }

        public async Task Register(RegisterUserRequest request)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _hasher.HashPassword(user, request.Password);

            //var verificationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var verificationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            user.EmailVerificationToken = verificationToken;
            user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var verificationLink = $"https://localhost:7134/api/auth/verify-email?token={verificationToken}";
            await _emailService.SendEmailAsync(
                user.Email,
               "Verify your email",
               $"Click here to verify: {verificationLink}"
                );



        }

        public async Task<AuthResponse> Login(LogInRequest request)
        {
            //user fetch
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email);

            //null check
            if (user == null)
                throw new Exception("User not found");



            //email verified check
            if (!user.IsEmailVerified)
            {
                throw new Exception("Please verify your email first");
            }

            //roles fetch
            var roles = await _context.UserRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.Role.Name).ToListAsync();


            //tokens generate
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var refreshTokenValue = _tokenService.GenerateRefreshToken();

            await _redis.SetAsync(
                                $"refresh:{user.Id}",
                                  refreshTokenValue,
                                  TimeSpan.FromDays(7)
                                   );

            // DB entity banao
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshTokenValue,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            //  MAIN STEP
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            // response
            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue
            };
        }

        //refreshtoken without redis

        //public async Task<AuthResponse> RefreshTokenAsync(string token)
        //{
        //    var existingToken = await _context.RefreshTokens
        //        .FirstOrDefaultAsync(x => x.Token == token);

        //    // ❌ invalid cases
        //    if (existingToken == null ||
        //        existingToken.IsRevoked ||
        //        existingToken.ExpiresAt < DateTime.UtcNow)
        //    {
        //        throw new Exception("Invalid refresh token");
        //    }

        //    // 👇 user fetch karo
        //    var user = await _context.Users
        //        .FirstOrDefaultAsync(x => x.Id == existingToken.UserId);
        //    var roles = await _context.UserRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.Role.Name).ToListAsync();

        //    if (user == null)
        //        throw new Exception("User not found");

        //    // 🔥 TOKEN ROTATION (IMPORTANT)
        //    existingToken.IsRevoked = true;

        //    var newRefreshTokenValue = _tokenService.GenerateRefreshToken();

        //    var newRefreshToken = new RefreshToken
        //    {
        //        Id = Guid.NewGuid(),
        //        UserId = user.Id,
        //        Token = newRefreshTokenValue,
        //        ExpiresAt = DateTime.UtcNow.AddDays(7),
        //        IsRevoked = false
        //    };

        //    _context.RefreshTokens.Add(newRefreshToken);
        //    await _context.SaveChangesAsync();

        //    // 🆕 access token
        //    var newAccessToken = _tokenService.GenerateAccessToken(user, roles);

        //    return new AuthResponse
        //    {
        //        AccessToken = newAccessToken,
        //        RefreshToken = newRefreshTokenValue
        //    };
        //}


        //refreshtokenasync new


        public async Task<AuthResponse> RefreshTokenAsync(string userId, string refreshToken)
        {
            var storedToken = await _redis.GetAsync($"refresh:{userId}");

            if (storedToken != refreshToken)
            {
                throw new Exception("Invalid refresh token");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id.ToString() == userId);

            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            var newAccessToken = _tokenService.GenerateAccessToken(user, roles);

            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = refreshToken
            };
        }


        public async Task<string> VerifyEmail(string token)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.EmailVerificationToken == token);

            if (user == null)
                throw new Exception("Invalid token");

            if (user.EmailVerificationTokenExpiry < DateTime.UtcNow)
                throw new Exception("Token expired");

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;

            await _context.SaveChangesAsync();

            return "Email verified successfully";
        }


        public async Task ForgotPassword(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                throw new Exception("User not found");

            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            var resetLink = $"https://localhost:7134/api/auth/reset-password?token={token}";

            _logger.LogInformation("Password reset link: {link}", resetLink);
        }

        public async Task ResetPassword(ResetPasswordRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.PasswordResetToken == request.Token);

            if (user == null)
                throw new Exception("Invalid token");

            if (user.PasswordResetTokenExpiry < DateTime.UtcNow)
                throw new Exception("Token expired");

            user.PasswordHash = _hasher.HashPassword(user, request.NewPassword);

            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            await _context.SaveChangesAsync();
        }

        public async Task LogoutAsync(string token)
        {
            await _redis.SetAsync(
                $"blacklist:{token}",
                "true",
                TimeSpan.FromMinutes(15) // same as access token expiry
            );
        }

    }

}
