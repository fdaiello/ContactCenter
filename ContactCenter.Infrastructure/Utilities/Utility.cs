using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using ContactCenter.Core.Models;
using System.Text;
using Microsoft.AspNetCore.StaticFiles;

namespace ContactCenter.Infrastructure.Utilities
{
	// Utility Methods
	public static class Utility
	{
		// Valida um Email
		public static bool IsValidEmail(string email)
		{
			return Regex.IsMatch(email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
		}
		// Valida um Celular
		public static bool IsValidPhone(string phone)
		{
			return ClearStringNumber(phone).Length >= 10 && ClearStringNumber(phone).Length < 14;
		}
		// Formata um celular para ser exibido para o usuario
		public static string UserFriendlyCelular ( string celular)
		{
			if (string.IsNullOrEmpty(celular))
				return celular;
			else
			{
				celular = PadronizaCelular(celular);
				return "(" + celular.Substring(0, 2) + ") " + celular.Substring(2, 5) + "-" + celular.Substring(7);
			}
		}


		// Formata um Celular - verifica se tem 9 digitos + DDD + DDI - e nao é nextel ( 77, 78, 79, 70 ) nem fixo ( 2, 3, 4, 5 )
		// Se for Brasil, retira o 55 da frente
		public static string FormataCelular(string phone)
		{
			phone = PadronizaCelular(phone);
			if (phone.Substring(0, 2) == "55" & phone.Length >= 12)
				return phone.Substring(2);
			else
				return phone;
		}
		// Padroniza um Celular - verifica se tem 9 digitos + DDD + DDI - e nao é nextel ( 77, 78, 79, 70 ) nem fixo ( 2, 3, 4, 5 )
		public static string PadronizaCelular(string phone)
		{
			if (string.IsNullOrEmpty(phone))
				return phone;

			phone = ClearStringNumber(phone);
			if (phone.Substring(0, 1) == "0")
				phone = phone.Substring(1);
			if (phone.Substring(0, 1) == "0")
				phone = phone.Substring(1);

			if (phone.Substring(0, 2) == "55" & phone.Length == 12)
				if (phone.Substring(4, 2) != "70" & phone.Substring(4, 2) != "77" & phone.Substring(4, 2) != "78" & phone.Substring(4, 2) != "79" & phone.Substring(4, 1) != "2" & phone.Substring(4, 1) != "3" & phone.Substring(4, 1) != "4" & phone.Substring(4, 1) != "5")
					return phone.Substring(0, 4) + "9" + phone.Substring(4);
				else
					return phone;
			else if ( phone.Length == 10)
				if (phone.Substring(2, 2) != "70" & phone.Substring(2, 2) != "77" & phone.Substring(2, 2) != "78" & phone.Substring(2, 2) != "79" & phone.Substring(2, 1) != "2" & phone.Substring(2, 1) != "3" & phone.Substring(2, 1) != "4" & phone.Substring(2, 1) != "5")
					return phone.Substring(0, 2) + "9" + phone.Substring(2);
				else
					return phone;
			else
				return phone;
		}
		// Verifica se o numero é um CPF - não faz analise do digito verificador, so confere o formato
		public static bool LooksLikeValidCPF(string cpf)
		{
			cpf = cpf.Replace(".", "").Replace("/", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
			return ClearStringNumber(cpf).Length == 11;
		}
		// Verifica se foi digitado um CNPJ - não faz analise do digito verificador, só confere o formato
		public static bool LooksLikeValidCNPJ(string cnpj)
		{
			cnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
			return ClearStringNumber(cnpj).Length == 14;
		}
		// Valida um CNJP - com digitos verificadores
		public static bool IsCnpj(string cnpj)
		{
			int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
			int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
			int soma;
			int resto;
			string digito;
			string tempCnpj;
			cnpj = cnpj.Trim();
			cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");
			if (cnpj.Length != 14)
				return false;
			tempCnpj = cnpj.Substring(0, 12);
			soma = 0;
			for (int i = 0; i < 12; i++)
				soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
			resto = (soma % 11);
			if (resto < 2)
				resto = 0;
			else
				resto = 11 - resto;
			digito = resto.ToString();
			tempCnpj += digito;
			soma = 0;
			for (int i = 0; i < 13; i++)
				soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];
			resto = (soma % 11);
			if (resto < 2)
				resto = 0;
			else
				resto = 11 - resto;
			digito += resto.ToString();
			return cnpj.EndsWith(digito);
		}
		// Valida um CPF - com digito verificador
		public static bool IsCpf(string cpf)
		{
			int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
			int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
			string tempCpf;
			string digito;
			int soma;
			int resto;
			cpf = cpf.Trim();
			cpf = cpf.Replace(".", "").Replace("-", "");
			if (cpf.Length != 11)
				return false;
			tempCpf = cpf.Substring(0, 9);
			soma = 0;

			for (int i = 0; i < 9; i++)
				soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
			resto = soma % 11;
			if (resto < 2)
				resto = 0;
			else
				resto = 11 - resto;
			digito = resto.ToString();
			tempCpf = tempCpf + digito;
			soma = 0;
			for (int i = 0; i < 10; i++)
				soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
			resto = soma % 11;
			if (resto < 2)
				resto = 0;
			else
				resto = 11 - resto;
			digito = digito + resto.ToString();
			return cpf.EndsWith(digito);
		}
		// Limpa um nome de possível saudação
		public static string CleanName(string name)
		{
			name = name.ToUpperInvariant().Replace("MEU NOME É ", "").Replace("MEU NOME E ", "").Replace("EU SOU O ", "").Replace("SOU O ", "").Replace("ME CHAMO ", "").Replace("SOU ", "").Replace("AQUI É ","").Replace("AQUI E ", "").Replace("AQUI ", "");

			return name;
		}
		// Valida um Nome
		public static bool IsValidName(string name)
		{
			name = name.ToLower();
			if (name.Length < 2 || name == "oi" || name.StartsWith("nao") || name.StartsWith("sim") || name.StartsWith("não") || name == "cancelar" || name == "sair" || name == "voltar" || name == "menu" || name == "cancel" || name == "*cancelar*" || name.StartsWith("oi ") || name.StartsWith("sim ") || name == "sim" || name.Contains("gostaria") || name.Contains("testar"))
				return false;
			else
				return Regex.IsMatch(name, @"^[\p{L}\p{M}' \.\-]+$");
		}
		// Remove separadores de um numero como string
		public static string ClearStringNumber(string stringnumber)
		{
			if (string.IsNullOrEmpty(stringnumber))
				return string.Empty;
			else
				return stringnumber.Replace(" ", "").Replace(".", "").Replace("(", "").Replace(")", "").Replace("-", "").Replace("+", "").Replace("/", "").Replace("\t","");
		}
		// Hora do brasil
		public static DateTime HoraLocal()
		{
			int fusohorario = -3;
			string displayName = "(GMT-03:00) Brasília";
			string standardName = "Horário de Brasília";
			TimeSpan offset = new TimeSpan(fusohorario, 00, 00);
			TimeZoneInfo tzBrasilia = TimeZoneInfo.CreateCustomTimeZone(standardName, offset, displayName, standardName);
			return TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, tzBrasilia);
		}
		// Valida um CEP - tem que estar formatado 00000-000
		public static bool IsValidCEP(string cep)
		{
			Regex Rgx = new Regex(@"^\d{5}-\d{3}$");

			return Rgx.IsMatch(cep);
		}
		// Formata um CEP - 00000-000
		public static string FormataCEP(string cep)
		{
			string fCep = ClearStringNumber(cep);
			if (fCep.Length == 8)
				fCep = fCep.Substring(0, 5) + "-" + fCep.Substring(5);
			return fCep;
		}
		// Primeira palavra do nome
		public static string FirstName(string fullName)
		{
			if (string.IsNullOrEmpty(fullName))
				return string.Empty;
			else
			{
				var names = fullName.Split(' ');
				return names[0];
			}
		}
		// Remove acentos
		public static string RemoveAcentos(this string text)
		{
			if (string.IsNullOrEmpty(text))
				return String.Empty;

			StringBuilder sbReturn = new StringBuilder();
			var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
			foreach (char letter in arrayText)
			{
				if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
					sbReturn.Append(letter);
			}
			return sbReturn.ToString();
		}
		public static string UrlCombine(Uri baseurl, string pathandfile)
		{
			pathandfile = pathandfile.TrimStart('/');
			return string.Format(new CultureInfo("en-US"), "{0}/{1}", baseurl.ToString().TrimEnd('/'), pathandfile);
		}
		// Return if a date is Holiday
		public static bool IsHoliday(DateTime date)
		{
			var Holidays = new List<DateTime>();

			// Fixos no Brasil
			Holidays.Add(new DateTime(DateTime.Now.Year, 1, 1));    // Ano Novo
			Holidays.Add(new DateTime(DateTime.Now.Year, 4, 21));   // Tira Dentes
			Holidays.Add(new DateTime(DateTime.Now.Year, 5, 1));    // Dia do trabalho
			Holidays.Add(new DateTime(DateTime.Now.Year, 9, 7));    // 7 de Setembro
			Holidays.Add(new DateTime(DateTime.Now.Year, 10, 12));  // Nossa Senhora Aparecida
			Holidays.Add(new DateTime(DateTime.Now.Year, 11, 2));   // Finados
			Holidays.Add(new DateTime(DateTime.Now.Year, 11, 15));  // Proclamação da República
			Holidays.Add(new DateTime(DateTime.Now.Year, 12, 25));  // Natal

			// Moveis Brasil 2021
			Holidays.Add(new DateTime(2021, 2, 15));   // Carnaval
			Holidays.Add(new DateTime(2021, 2, 16));   // Carnaval
			Holidays.Add(new DateTime(2021, 2, 17));   // Carnaval
			Holidays.Add(new DateTime(2021, 4, 2));    // Sexta feira santa

			return Holidays.Contains(date);
		}
		// Return true or false if a date is weend or not
		public static bool IsWeekend(DateTime date)
		{
			return date.DayOfWeek == DayOfWeek.Saturday
				|| date.DayOfWeek == DayOfWeek.Sunday;
		}
		// Return next business day after given date
		public static DateTime GetNextWorkingDay(DateTime date)
		{
			do
			{
				date = date.AddDays(1);
			} while (IsHoliday(date) || IsWeekend(date));
			return date;
		}
		public static string BomTurno()
		{
			return HoraLocal().Hour > 19 ? "Boa noite!" : HoraLocal().Hour > 12 ? "Boa tarde!" : HoraLocal().Hour > 5 ? "Bom dia!" : "Boa noite!";
		}
		public static string CombineUrlsToString(string @base, string relativeOrAbsolute)
		{
			string url = string.Empty;

			if ( !relativeOrAbsolute.StartsWith("data:"))
			{ 
				try
				{
					url = new Uri(new Uri(@base), relativeOrAbsolute).ToString();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}

			}
			return url;
		}
		public static string UniqueFileName(string filename, int groupId)
		{
			DateTimeOffset dto = DateTime.UtcNow;

			filename = groupId.ToString(CultureInfo.InvariantCulture) + "/" + dto.ToString("o", new CultureInfo("en-US")).Replace(".","") + "-" + RemoveAcentos(filename);
			filename = filename.Replace(":", "-", StringComparison.Ordinal).Replace(" ", "-", StringComparison.Ordinal).Replace("+", "", StringComparison.Ordinal);

			return filename;
		}
		public static ChatMsgType GetChatMsgType(string filename)
		{
			if (string.IsNullOrEmpty(filename))
				return ChatMsgType.Text;

			else if (
				filename.ToUpperInvariant().EndsWith("OGG", StringComparison.InvariantCulture) ||
				filename.ToUpperInvariant().EndsWith("OGA", StringComparison.InvariantCulture) ||
				filename.ToUpperInvariant().EndsWith("WMA", StringComparison.InvariantCulture) ||
				filename.ToUpperInvariant().EndsWith("MP3", StringComparison.InvariantCulture) ||
				filename.ToUpperInvariant().EndsWith("WAV", StringComparison.InvariantCulture)
			)
				return ChatMsgType.Voice;

			else if (
				filename.ToUpperInvariant().EndsWith("PNG", StringComparison.InvariantCulture) ||
				filename.ToUpperInvariant().EndsWith("JPG", StringComparison.InvariantCulture) ||
				filename.ToUpperInvariant().EndsWith("JPEG", StringComparison.InvariantCulture) ||
				filename.ToUpperInvariant().EndsWith("BMP", StringComparison.InvariantCulture) ||
				filename.ToUpperInvariant().EndsWith("GIF", StringComparison.InvariantCulture)
			)
				return ChatMsgType.Image;

			else if (filename.ToUpperInvariant().EndsWith("PDF", StringComparison.InvariantCulture))
				return ChatMsgType.PDF;

			else if (
			   filename.ToUpperInvariant().EndsWith("DOC", StringComparison.InvariantCulture) ||
			   filename.ToUpperInvariant().EndsWith("DOCX", StringComparison.InvariantCulture)
			)
				return ChatMsgType.Word;

			else if (
			   filename.ToUpperInvariant().EndsWith("XLS", StringComparison.InvariantCulture) ||
			   filename.ToUpperInvariant().EndsWith("XLSX", StringComparison.InvariantCulture) ||
			   filename.ToUpperInvariant().EndsWith("CSV", StringComparison.InvariantCulture)
			)
				return ChatMsgType.Excel;

			else
				return ChatMsgType.File;
		}

		public static string Description(this Enum value)
		{
			// get field of type
			var field = value.GetType().GetField(value.ToString());

			// return description
			return field?.Name ?? "";
		}

		public static string CleanHTML ( string text )
        {
			if ( text != null )
            {
				// Div- used as line break by EmojiOn editor - multiple combinations possible
				if (text.Contains("<div>"))
					text = text.Replace("<div><br></div>", "\n").Replace("<div></div>", "\n").Replace("<div>", "\n").Replace("</div>", "");

				// Bold sign
				if (text.Contains("**"))
					text = text.Replace("**", "*");

				// HTML line break
				if (text.Contains("<br>"))
					text = text.Replace("<br>", "\n");

				// HTML spaces
				if (text.Contains("&nbsp;"))
					text = text.Replace("&nbsp;", " ");

				// Greater and Lower
				if (text.Contains("&gt;") | text.Contains("&lt;"))
					text = text.Replace("&gt;", ">").Replace("&lt;", "<");

				// Span
				if (text.Contains("<span>"))
					text = text.Replace("<span>", " ").Replace("</span>", " ");
			}
			return text;
		}
		/* 
         * Ao inserir emojis, o plug-in insere codigos HTML
         * A função remove o codigo HTML e deixa apenas o unicode do Emoji
         */
		public static string CleanEmojiHTML(string msgText)
		{

			string cleanText = msgText;

			if (!string.IsNullOrEmpty(msgText) && msgText.Contains("emoji"))
			{
				int p1 = msgText.IndexOf("<img");
				if (p1 >= 0)
				{
					int p2 = msgText.Substring(p1).IndexOf(">");
					int p4 = msgText.IndexOf("alt=\"");
					int p5 = msgText.Substring(p4 + 5).IndexOf("\"");
					string antes = msgText.Substring(0, p1).Replace("&nbsp;", " ");
					string emoji = msgText.Substring(p4 + 5, p5);
					string depois = msgText.Substring(p2 + p1 + 1).Replace("&nbsp;", " ");
					cleanText = antes + emoji + depois;

					if (cleanText.Contains("emoji"))
						cleanText = CleanEmojiHTML(cleanText);
				}
			}

			return cleanText;
		}

		// Devolve o Content Type baseado no nome do arquivo
		public static string GetContentType(string fileName)
		{
			var provider = new FileExtensionContentTypeProvider();
			provider.Mappings.Remove(".csv");
			provider.Mappings.Add(".csv", "text/csv");
			string contentType;
			if (!provider.TryGetContentType(fileName, out contentType))
			{
				contentType = "application/octet-stream";
			}
			return contentType;
		}
		// Check if its a valid URI
		public static bool IsValidURI(string uri)
		{
			if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
				return false;
			Uri tmp;
			if (!Uri.TryCreate(uri, UriKind.Absolute, out tmp))
				return false;
			return tmp.Scheme == Uri.UriSchemeHttp || tmp.Scheme == Uri.UriSchemeHttps;
		}
		// Capitalize first letter
		public static string UppercaseWords(string value)
		{
			value = value.ToLower();

			char[] array = value.ToCharArray();
			// Handle the first letter in the string.
			if (array.Length >= 1)
			{
				if (char.IsLower(array[0]))
				{
					array[0] = char.ToUpper(array[0]);
				}
			}
			// Scan through the letters, checking for spaces.
			// ... Uppercase the lowercase letters following spaces.
			for (int i = 1; i < array.Length; i++)
			{
				if (array[i - 1] == ' ')
				{
					if (char.IsLower(array[i]))
					{
						array[i] = char.ToUpper(array[i]);
					}
				}
			}
			return new string(array);
		}
		// Base 64 encoding
		public static string Base64Encode(string plainText)
		{
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
			return System.Convert.ToBase64String(plainTextBytes);
		}
	}
}

