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
using ContactCenter.Core.Models;
using Microsoft.Extensions.Configuration;

namespace ContactCenter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ApiControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        // POST Login
        [HttpPost]
        public async Task <IActionResult> PostLogin([FromBody] LoginViewModel login)
        {
            // Check if got login information
            if (login == null || string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.Password))
                return BadRequest("Invalid request");

            // Searchs Username
            var user = await _userManager.FindByNameAsync(login.Username).ConfigureAwait(false);
            if (user == null)
                return Unauthorized();

            // Check if password matches
            if (!await _userManager.CheckPasswordAsync(user, login.Password))
                return Unauthorized();

			else
			{
                List<string> roles = (List<string>)await _userManager.GetRolesAsync(user);
                string role = roles.FirstOrDefault();
                return Ok(new { Token = GenerateToken(user.UserName, user.Id.ToString(), user.GroupId, _configuration.GetValue<string>("JwtSecret"), role) });
            }

        }
    }
}
