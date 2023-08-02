using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactCenter.Core.Models;
using ContactCenter.Data;
using ContactCenter.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ContactCenter.Web.Controllers.Public
{
	[Route("[controller]")]
	public class UnsubscribeController : Controller
	{
		private readonly ApplicationDbContext _context;

		public UnsubscribeController(ApplicationDbContext context) 
		{
			_context = context;
		}
		[HttpGet]
		[AllowAnonymous]
		public IActionResult AskEmail()
		{
			ViewData["askEmail"] = true;
			ViewData["message"] = string.Empty;
			return View("UnsubscribeView");
		}
		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> MarkEmail()
		{
			// Search e-mail at database
			string message = string.Empty;
			string email = Request.Form["email"];

			if (string.IsNullOrEmpty(email))
			{
				ViewData["askEmail"] = true;
				message = "Por favor, informe seu email.";
			}
			else if (!Utility.IsValidEmail(email))
			{
				ViewData["askEmail"] = true;
				message = $"Este não é um email válido: {email}";
			}
			else
			{
				// Procura pelos contatos com este email
				List<Contact> contacts = await _context.Contacts
										.Where(p => p.Email == email)
										.ToListAsync();

				// Se achou
				if (contacts.Any())
				{
					foreach ( Contact contact in contacts)
					{
						contact.OptStatus = OptStatus.OptOut;
						_context.Contacts.Update(contact);
					}
					await _context.SaveChangesAsync();

					ViewData["askEmail"] = false;
					message = "Seu email foi excluído da nossa base!";

				}
				else
				{
					ViewData["askEmail"] = true;
					message = $"Este email não foi localizado na nossa base: {email}";

				}
			}
			
			ViewData["message"] = message;
			return View("UnsubscribeView");
		}
	}
}
