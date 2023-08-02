using ContactCenter.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ContactCenter.Data;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Cors;

namespace ContactCenter.Controllers
{
    [EnableCors("AllowMyOrigins")]
    public class ApiControllerBase : ControllerBase
    {
        protected int AuthorizedGroupId()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var sGroupId = claimsIdentity.FindFirst(ClaimTypes.GroupSid)?.Value;

            if (Int32.TryParse(sGroupId, out int groupId))
                return groupId;
            else
                return 0;

        }
        protected string AuthenticatedUserId()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Sid)?.Value;

            return userId;

        }
        protected string AuthenticatedUserRole()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.Role)?.Value;

            return userId;

        }
        protected string GenerateToken(string username, string userId, int groupId, string jwtSecret, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Sid, userId),
                    new Claim(ClaimTypes.GroupSid, groupId.ToString()),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
