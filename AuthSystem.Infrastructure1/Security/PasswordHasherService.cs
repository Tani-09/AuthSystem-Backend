using AuthSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthSystem.Infrastructure.Security
{
    public class PasswordHasherService
    {

        private readonly PasswordHasher<User> _hasher = new();

        public string HashPassword(User user, string password)
        {
            return _hasher.HashPassword(user, password);
        }

        public bool VerifyPassword(User user, string hash, string password)
        {

            var result = _hasher.VerifyHashedPassword(user, hash, password);
            return result == PasswordVerificationResult.Success;

        }
    }
}
