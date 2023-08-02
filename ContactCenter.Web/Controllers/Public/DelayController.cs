using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactCenter.Core.Models;
using ContactCenter.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace ContactCenter.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[AllowAnonymous]
	public class DelayController : ControllerBase
	{
		[HttpGet]
		public string Delay(int ms)
		{
			Thread.Sleep(ms);
			return $"Delay {ms} ms.";
		}
	}
}
