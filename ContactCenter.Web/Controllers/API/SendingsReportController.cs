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
    public class SendingsReportController : ApiControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SendingsReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/SendingsReport/<sendingId>
        [HttpGet("{id}")]
        public async Task<ActionResult<SendingReportView>> GetSendingReport(int id)
        {
            // Find Sending ReportView
            SendingReportView sendingReportView = await _context.SendingReportView
                        .Where(p => p.Id == id & p.GroupId == AuthorizedGroupId())
                        .FirstOrDefaultAsync();

            // If not found
            if (sendingReportView == null)
            {
                return new SendingReportView();
            }
            else
            {
                return sendingReportView;
            }
        }


    }
}
