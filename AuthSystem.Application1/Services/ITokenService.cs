using AuthSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthSystem.Application.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user, List<string> roles);
        string GenerateRefreshToken();
    }
}
