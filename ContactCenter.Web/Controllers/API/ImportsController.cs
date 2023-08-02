using ContactCenter.Core.Models;
using ContactCenter.Data;
using ContactCenter.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactCenter.Infrastructure.Utilities;
using RestSharp;
using Azure.Storage.Queues;
using Newtonsoft.Json;

namespace ContactCenter.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ImportsController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public ImportsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        // GET: api/<ImportsController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImportDto>>> GetImports()
        {
            var Imports = await _context.Imports
                .Where(p => p.GroupId == AuthorizedGroupId())
                .Include(p => p.Board)
                .OrderByDescending( o=> o.ImportDate)
                .Select(q => new ImportDto(q))
                .ToListAsync();

            return Imports;
        }

        // GET api/<ImportsController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ImportDto>> GetImport(int id)
        {
            // Find Import
            ImportDto ImportDto = await _context.Imports
                        .Where(p => p.Id == id & p.GroupId == AuthorizedGroupId())
                        .Select(p => new ImportDto(p))
                        .FirstOrDefaultAsync();

            // If not found
            if (ImportDto == null)
            {
                return NotFound();
            }
            else
            {
                return ImportDto;
            }
        }

        // POST api/<ImportsController>
        [HttpPost]
        public async Task<ActionResult<ImportDto>> PostImport(Import import)
        {

            // Bind Group
            import.GroupId = AuthorizedGroupId();

            // Set last activity to now so Contact appears up at left sidebar
            import.ImportDate = Utility.HoraLocal();

            // Check if boardId was set to (-1), flag to create new board
            if ( import.BoardId == 0 )
            {
                // Create new board
                Board board = new Board { GroupId = AuthorizedGroupId(), Name = import.NewListName, Label=string.Empty };
                await _context.Boards.AddAsync(board);
                await _context.SaveChangesAsync();

                // Assign new boardId to import
                import.BoardId = board.Id;
            }

            // Add 
            _context.Imports.Add(import);


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ImportExists(import.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            // Query to get descriptors
            Import newImport = await _context.Imports
                                .Where(p => p.Id == import.Id)
                                .Include( b=> b.Board)
                                .FirstOrDefaultAsync();

            // Fire import function - queues import Object
            string storageConnStr = _configuration.GetValue<string>("BlobStorageConnStr");
            QueueClient queue = new QueueClient(storageConnStr, "imports");
            await queue.SendMessageAsync(Utility.Base64Encode(JsonConvert.SerializeObject(import, Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            })));

            // Return import Dto
            return CreatedAtAction("PostImport", new { id = import.Id }, new ImportDto(newImport));
        }

        // PUT api/<ImportsController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutImport(int id, Import Import)
        {
            // Check if Import has Id
            if (id != Import.Id | id==0)
            {
                string error = "Não foi informado um registro de importação válido com seu ID.";
                return BadRequest(error);
            }

            // Check if Import Id existe at database
            Import oldImport = await _context.Imports
                                .Where(p=> p.Id == id)
                                .AsNoTracking()
                                .FirstOrDefaultAsync();

            // Check if Import exists
            if (oldImport == null)
            {
                string error = "Import Id {id} não localizado na base.";
                return NotFound(error);
            }
            // Check if it has not been sent 
            else if ( oldImport.Status != ImportStatus.queued )
			{
                string error = $"Este registro de importação não pode mais ser alterado pois está com status {oldImport.Status.Description()}";
                return BadRequest(error);
            }

            // Bind Group
            Import.GroupId = AuthorizedGroupId();

            // Update Database
            _context.Update(Import);
            await _context.SaveChangesAsync();

            // Return
            return NoContent();
        }

        // DELETE api/<ImportsController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ImportDto>> Delete(int id)
        {
            var Import = await _context.Imports
                                .Where(p => p.Id == id & p.GroupId == AuthorizedGroupId())
                                .FirstOrDefaultAsync();

            if (Import == null)
            {
                return NotFound();
            }
            // Check if it has not been sent 
            else if (Import.Status == ImportStatus.importing)
            {
                string error = $"Esta importação não pode ser excluida pois está sendo processada.";
                return BadRequest(error);
            }

            _context.Imports.Remove(Import);
            await _context.SaveChangesAsync();

            return new ImportDto(Import);


        }

        private bool ImportExists(int id)
        {
            return _context.Imports.Any(e => e.Id == id);
        }

    }
}
