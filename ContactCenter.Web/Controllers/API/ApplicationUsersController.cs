using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContactCenter.Data;
using ContactCenter.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace ContactCenter.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

    public class ApplicationUsersController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApplicationUsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ApplicationUser
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApplicationUserDto>>> GetApplicationUsers()
        {

            return await _context.ApplicationUsers
                .Where(p=>p.GroupId == AuthorizedGroupId())
                .Select(q=> new ApplicationUserDto(q))
                .ToListAsync();

        }

        // GET: api/ApplicationUser/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationUserDto>> GetApplicationUser(string id)
        {
            var applicationUser = await _context.ApplicationUsers.FindAsync(id);

            if (applicationUser == null)
            {
                return NotFound();
            }
            else if (applicationUser.GroupId != AuthorizedGroupId())
            {
                return Unauthorized();
            }

            ApplicationUserDto applicationUserDto = new ApplicationUserDto(applicationUser);
            // We have to get email and phone from Identity

            return applicationUserDto;
        }

        //// PUT: api/ApplicationUser/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to, for
        //// more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutAgentView(string id, AgentView agentView)
        //{
        //    if (id != agentView.Id)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(agentView).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!AgentViewExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        //// POST: api/ApplicationUser
        //// To protect from overposting attacks, enable the specific properties you want to bind to, for
        //// more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        //[HttpPost]
        //public async Task<ActionResult<AgentView>> PostAgentView(AgentView agentView)
        //{
        //    _context.AgentView.Add(agentView);
        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException)
        //    {
        //        if (AgentViewExists(agentView.Id))
        //        {
        //            return Conflict();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return CreatedAtAction("GetAgentView", new { id = agentView.Id }, agentView);
        //}

        //// DELETE: api/ApplicationUser/5
        //[HttpDelete("{id}")]
        //public async Task<ActionResult<AgentView>> DeleteAgentView(string id)
        //{
        //    var agentView = await _context.AgentView.FindAsync(id);
        //    if (agentView == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.AgentView.Remove(agentView);
        //    await _context.SaveChangesAsync();

        //    return agentView;
        //}

        //private bool AgentViewExists(string id)
        //{
        //    return _context.AgentView.Any(e => e.Id == id);
        //}
    }
}
