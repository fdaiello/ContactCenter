using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ContactCenter.Core.Models;
using ContactCenter.Data;
using ContactCenter.Helpers;
using ContactCenter.Infrastructure.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactCenter.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class LandingsController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobContainerClient _blobContainerClient;

        public LandingsController(ApplicationDbContext context, BlobContainerClient blobContainerClient)
        {
            _context = context;
            _blobContainerClient = blobContainerClient;
        }
        // GET: api/<LandingsController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LandingDto>>> GetLandings()
        {
            return await _context.Landings
                .Where(p => p.GroupId == AuthorizedGroupId())
                .Select(q => new LandingDto(q))
                .ToListAsync();
        }

        // GET api/<LandingsController>/Templates
        [HttpGet("templates")]
        public async Task<ActionResult<IEnumerable<LandingDto>>> GetTemplates()
        {
            return await _context.Landings
                .Where(p => p.GroupId == 1)
                .Select(q => new LandingDto(q))
                .ToListAsync();
        }

        // GET api/Landings/Templates/Id
        [HttpGet("templates/{id}")]
        public async Task<ActionResult<LandingDto>> GetTemplate(int id)
        {
            // Find Landing
            LandingDto LandingDto = await _context.Landings
                        .Where(p => p.Id == id & p.GroupId == 1)
                        .Select(p => new LandingDto(p))
                        .FirstOrDefaultAsync();

            // If not found
            if (LandingDto == null)
            {
                return NotFound();
            }
            else
            {
                return LandingDto;
            }
        }

        // GET api/Landings/Copy/Id
        [HttpGet("Copy/{id}")]
        public async Task<ActionResult<LandingDto>> CopyLanding(int id)
        {
            // Find Landing
            Landing landing = await _context.Landings
                        .Where(p => p.Id == id & p.GroupId == AuthorizedGroupId())
                        .FirstOrDefaultAsync();

            // If not found
            if (landing == null)
            {
                return NotFound();
            }
            else
            {
                // Makes a clone
                Landing newLanding = new Landing();
                newLanding.CopyFrom(landing);
                newLanding.Id = 0;
                newLanding.Title += " - Copy";
                newLanding.PageViews = 0;
                newLanding.Leads = 0;

                // Get Last Landing index used at messages
                int Index = _context.Landings.Select(p => p.Index).Max() ?? 0;

                // Generate next code
                Index++;
                newLanding.Code = newLanding.CodeIndex(Index);
                newLanding.Index = Index;

                // Insert into database
                _context.Landings.Add(newLanding);
                await _context.SaveChangesAsync();

                // Return Dto from landing
                return new LandingDto(newLanding);
            }
        }

        // GET api/Landings/Resset/Id
        [HttpGet("Resset/{id}")]
        public async Task<ActionResult<LandingDto>> RessetLanding(int id)
        {
            // Find Landing
            Landing landing = await _context.Landings
                        .Where(p => p.Id == id & p.GroupId == AuthorizedGroupId())
                        .FirstOrDefaultAsync();

            // If not found
            if (landing == null)
            {
                return NotFound();
            }
            else
            {
                // Resset statistic
                landing.PageViews = 0;
                landing.Leads = 0;

                // Update
                _context.Landings.Update(landing);
                await _context.SaveChangesAsync();

                // Return Dto from landing
                return new LandingDto(landing);
            }
        }

        // GET api/<LandingsController>/code/index
        [HttpGet("code/{index}")]
        public ActionResult GetCode(int index)
        {
            Landing message = new Landing();
            return Ok(message.CodeIndex(index));
        }

        // GET api/<LandingsController>/uncode/code
        [HttpGet("uncode/{code}")]
        public ActionResult GetIndex(string code)
        {
            Landing message = new Landing();
            return Ok(message.UnCodeIndex(code));
        }


        // GET api/<LandingsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LandingDto>> GetLanding(int id)
        {
            // Find Landing
            LandingDto LandingDto = await _context.Landings
                        .Where(p => p.Id == id & p.GroupId == AuthorizedGroupId())
                        .Select(p => new LandingDto(p))
                        .FirstOrDefaultAsync();

            // If not found
            if (LandingDto == null)
            {
                return NotFound();
            }
            else
            {
                return LandingDto;
            }
        }

        // POST api/<LandingsController>
        [HttpPost]
        public async Task<ActionResult<LandingDto>> PostLanding(Landing landing)
        {
            // Bind Group
            landing.GroupId = AuthorizedGroupId();

            // Set last activity to now so Contact appears up at left sidebar
            landing.CreatedDate = Utility.HoraLocal();

            // Get Last Landing index used at messages
            int Index = _context.Landings.Select(p => p.Index).Max()??0;

            // Generate next code
            Index++;
            landing.Code = landing.CodeIndex(Index);
            landing.Index = Index;

            // Add 
            _context.Landings.Add(landing);


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (LandingExists(landing.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("PostLanding", new { id = landing.Id }, new LandingDto(landing));
        }

        // PUT api/<LandingsController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLanding(int id, Landing landing)
        {
            // Check if Landing has Id
            if (id != landing.Id | id==0)
            {
                string error = "Não foi informado uma Landing Page válida com seu ID.";
                return BadRequest(error);
            }

            // Check if Landing Id existe at database - and belongs to authenticated group
            Landing oldLanding = await _context.Landings
                                .Where(p => p.Id == id && p.GroupId == AuthorizedGroupId())
                                .AsNoTracking()
                                .FirstOrDefaultAsync();

            if (oldLanding == null)
            {
                string error = $"Landing {id} não localizada na base.";
                return NotFound(error);
            }

            // Bind Group
            landing.GroupId = AuthorizedGroupId();

            // Update Database
            _context.Update(landing);
            await _context.SaveChangesAsync();

            // Return
            return NoContent();
        }

        // DELETE api/<LandingsController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<LandingDto>> Delete(int id)
        {
            var message = await _context.Landings
                                .Where(p => p.Id == id & p.GroupId == AuthorizedGroupId())
                                .FirstOrDefaultAsync();

            if (message == null)
            {
                return NotFound();
            }

            try
            {
                _context.Landings.Remove(message);
                await _context.SaveChangesAsync();
                return new LandingDto(message);
            }
            catch ( Exception ex)
            {
                return BadRequest(ex);
            }
        }

        private bool LandingExists(int id)
        {
            return _context.Landings.Any(e => e.Id == id);
        }

    }
}
