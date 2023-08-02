using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ContactCenter.Core.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ContactCenter.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FieldTypesController : ControllerBase
    {
        // GET: api/<FieldTypesController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return Enum.GetValues(typeof(FieldType)).Cast<FieldType>().Select(v => v.ToString());
        }

    }
}
