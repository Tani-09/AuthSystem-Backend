using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthSystem.Application.DTOs
{
    public class ResetPasswordRequest
    {

        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
