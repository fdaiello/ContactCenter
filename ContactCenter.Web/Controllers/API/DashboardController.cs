using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ContactCenter.Core.Models;
using ContactCenter.Data;
using ContactCenter.Helpers;
using ContactCenter.Infrastructure.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace ContactCenter.Controllers.API
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	public class DashboardController : ApiControllerBase
	{
		private readonly ApplicationDbContext _context;
		private readonly IConfiguration _configuration;
		private readonly IMemoryCache _cache;

		public DashboardController(ApplicationDbContext context, IConfiguration configuration, IMemoryCache cache)
		{
			_context = context;
			_configuration = configuration;
			_cache = cache;
		}

		// Devolve os dados para o Dashboard dos Agentes
		// Usa cache de memória
		[HttpGet("Agents")]
		public async Task<ActionResult<IEnumerable<DashboardAgentView>>> GetDashboardAgents(string dateStart, string dateEnd)
		{
			// Name of Chache entry - concatenated with date period
			string cacheName = $"DashboardAgents-{dateStart}to{dateEnd}-g{AuthorizedGroupId()}";

			// Use memory Cache to return Dashboard
			var cacheEntry = _cache.GetOrCreate(cacheName, async entry =>
			{
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
				entry.SetPriority(CacheItemPriority.High);

				return await DashboardAgents(dateStart, dateEnd);
			});

			return await cacheEntry;
		}
		// Devolve os dados para o Dashboard dos Agentes
		public async Task<ActionResult<IEnumerable<DashboardAgentView>>> DashboardAgents(string dateStart, string dateEnd)
		{
			// Check Parameters
			if (String.IsNullOrEmpty(dateStart))
				return BadRequest("É preciso informar a data de inicio");
			else if (string.IsNullOrEmpty(dateEnd))
				return BadRequest("É preciso informar a data final");
			else if (!DateTime.TryParse(dateStart, out _))
				return BadRequest($"dateStart {dateStart} inválida.");
			else if (!DateTime.TryParse(dateEnd, out _))
				return BadRequest($"dateEnd {dateEnd} inválida.");

			// Converte as data para o formato americano
			string d1 = DateTime.Parse(dateStart, new CultureInfo("pt-BR")).ToString("yyyy/MM/dd");
			string d2 = DateTime.Parse(dateEnd, new CultureInfo("pt-BR")).ToString("yyyy/MM/dd");

			List<DashboardAgentView> dashboardAgents;

			try
            {
				string sQl = $"select * from v_dashboard_agents ({AuthorizedGroupId()}, '{d1}', '{d2}')";

				dashboardAgents = await _context.DashboardAgentViews
						.FromSqlRaw(sQl)
						.AsNoTracking()
						.OrderBy(p => p.Contacts)
						.ToListAsync();

				if (dashboardAgents is null)
				{
					dashboardAgents = new List<DashboardAgentView>();
				}
				else
				{
					// Temos que adicionar a url base antes de entregar o Json, porque no banco está o caminho relativo da imagem, dentro do FileStorage
					foreach (DashboardAgentView dashboardAgent in dashboardAgents)
					{
						if (dashboardAgent.Id == "bot-id")
							dashboardAgent.PictureFile = _configuration.GetValue<string>("WebPushR:Icon");

						else if (!string.IsNullOrEmpty(dashboardAgent.PictureFile))
							dashboardAgent.PictureFile = Utility.CombineUrlsToString(_configuration.GetValue<string>("FileContainerUrl"), dashboardAgent.PictureFile);
						else
							dashboardAgent.PictureFile = string.Empty;
					}
				}

			}
			catch
            {
				dashboardAgents = new List<DashboardAgentView>();
			}


			// Devolve o Json para montar o quadro resumo dos Agentes;
			return dashboardAgents;
		}
		// Devolve os dados para o Dashboard dos Agentes
		// Usa cache de memória
		[HttpGet("ContactsBySource")]
		public async Task<ActionResult<IEnumerable<DashboardContactsBySourceView>>> GetDashboardContactsBySource(string dateStart, string dateEnd)
		{
			// Name of Chache entry - concatenated with date period
			string cacheName = $"DashboardContactsBySource-{dateStart}to{dateEnd}";

			// Use memory Cache to return Dashboard
			var cacheEntry = _cache.GetOrCreate(cacheName, async entry =>
			{
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
				entry.SetPriority(CacheItemPriority.High);

				return await DashboardAgentsDashboardContactsBySource(dateStart, dateEnd);
			});

			return await cacheEntry;
		}

		// Devolve os dados para a tabela do Dashboard que mostra ContactsBySource - DashboardContactsBySourceView
		public async Task<ActionResult<IEnumerable<DashboardContactsBySourceView>>> DashboardAgentsDashboardContactsBySource(string dateStart, string dateEnd)
		{
			// Check Parameters
			if (String.IsNullOrEmpty(dateStart))
				return BadRequest("É preciso informar a data de inicio");
			else if (string.IsNullOrEmpty(dateEnd))
				return BadRequest("É preciso informar a data final");
			else if (!DateTime.TryParse(dateStart, out _))
				return BadRequest($"dateStart {dateStart} inválida.");
			else if (!DateTime.TryParse(dateEnd, out _))
				return BadRequest($"dateEnd {dateEnd} inválida.");

			// Converte as data para o formato americano
			string d1 = DateTime.Parse(dateStart, new CultureInfo("pt-BR")).ToString("yyyy/MM/dd");
			string d2 = DateTime.Parse(dateEnd, new CultureInfo("pt-BR")).ToString("yyyy/MM/dd");

			List<DashboardContactsBySourceView> contactsBySourceView;

			try
			{
				string sQl = $"select * from v_dashboard_contactsbysource ({AuthorizedGroupId()}, '{d1}', '{d2}')";

				contactsBySourceView = await _context.DashboardContactsBySourceView
						.FromSqlRaw(sQl)
						.AsNoTracking()
						.OrderBy(p => p.ChannelName)
						.ThenBy(p=>p.SourceDescription)
						.ToListAsync();

			}
			catch ( Exception ex)
			{
				Console.WriteLine(ex.Message);
				if (ex.InnerException != null)
					Console.WriteLine(ex.InnerException.Message);
				contactsBySourceView = new List<DashboardContactsBySourceView>();
			}


			// Devolve o Json para montar o quadro resumo dos Agentes;
			return contactsBySourceView;
		}

	}
}
