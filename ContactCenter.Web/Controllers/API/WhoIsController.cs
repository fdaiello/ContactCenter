using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactCenter.Core.Models;
using ContactCenter.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ContactCenter.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class WhoIsController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WhoIsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/<WhoIsController>
        [HttpGet]
        public async Task<ActionResult<ApplicationUserDto>> GetAuthenticatedUser()
        {
            var applicationUser = await _context.ApplicationUsers.FindAsync(AuthenticatedUserId());

            if (applicationUser == null)
            {
                return NotFound();
            }
            else if (applicationUser.GroupId != AuthorizedGroupId())
            {
                return Unauthorized();
            }

            return new ApplicationUserDto(applicationUser);
        }
    }
}
