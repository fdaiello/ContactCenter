using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using ContactCenter.Core.Models;
using ContactCenter.Data;
using Microsoft.Extensions.Configuration;

namespace ContactCenter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TokenController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public TokenController(IConfiguration configuration, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _context = context;
            _userManager = userManager;
        }

        // Get Token
        [HttpGet]
        public async Task<IActionResult> Token()
        {
            string userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);
            string userName = _userManager.GetUserName(User);
            int groupId = user.GroupId;
            string secret = _configuration.GetValue<string>("JwtSecret");

            List<string> roles = (List<string>)await _userManager.GetRolesAsync(user);
            string role = roles.FirstOrDefault();

            return Ok(new { Token = GenerateToken(userName, userId, groupId, secret, role) });
        }
    }
}
