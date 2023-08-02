using ContactCenter.Core.Models;
using ContactCenter.Data;
using ContactCenter.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactCenter.Infrastructure.Utilities;
using RestSharp;

namespace ContactCenter.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChattingLogsController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChattingLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/ChattingLogs/<sendingId>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChattingLogDto>>> GetChattingLogs(int sendingId)
        {
            // query Chatting Logs
            List<ChattingLogDto> chattingLogs = await _context.ChattingLogs
                                        .Where(p => p.SendingId == sendingId && p.GroupId == AuthorizedGroupId())
                                        .Select ( p=> new ChattingLogDto(p))
                                        .ToListAsync();
                                        
            return chattingLogs;
        }


    }
}
