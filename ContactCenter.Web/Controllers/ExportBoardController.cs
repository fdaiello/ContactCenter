using ContactCenter.Core.Models;
using ContactCenter.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactCenter.Web.Controllers
{

    /*
     * Controller para exportar os dados de um Board ( lista ) com campos customizados dos cartões ( CardFields )
     * Utiliza uma proceadure com uma Pivot Table: exportboardpivot
     * Faz conexão 'old-fashion' com o banco - não usamos Entity Framework porque não sabemos a estrutura resultante da procedure - os campos variam
     * Monta uma stream com os valores, e devolve como arquivo texto
     */
    public class ExportBoardController : Controller
	{
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _ApplicationDbContext;       //DbContext
        private readonly UserManager<ApplicationUser> _userManager;        //Admin and Agent Table Managenment

        public ExportBoardController (IConfiguration configuration, ApplicationDbContext ApplicationDbContext, UserManager<ApplicationUser> userManager )
		{
            _configuration = configuration;
            _userManager = userManager;
            _ApplicationDbContext = ApplicationDbContext;
        }

        [HttpGet]
        public IActionResult Index(int boardId, string sqlWhere)
        {
            // Busca o ID do grupo do usuário logado
            int groupID = _ApplicationDbContext.Users.Find(_userManager.GetUserId(User)).GroupId;

            // Confere se o boardId informado faz parte do grupo do usuario logado
            Board board = _ApplicationDbContext.Boards.Where(p => p.GroupId == groupID && p.Id == boardId).FirstOrDefault();
            if ( board==null)
            {
                return Unauthorized();
            }

            // String de conexão
            string connString = _configuration.GetConnectionString("DefaultConnection");
            SqlConnection conn = new SqlConnection(connString);
            // Abre conexão com o banco
            conn.Open();

            // Se não informado clausula where, passa string vazia para a proceadure
            if (string.IsNullOrEmpty(sqlWhere))
                sqlWhere = "''";

            // SQL executa a proceadure previamente construída no banco
            string sql = $"exec exportboardpivot @boardId={boardId}, @sqlWhere={sqlWhere}";

            // Abre um DataTable com o resultado da proceadure
            SqlDataAdapter sda = new SqlDataAdapter(sql, conn);
            DataSet ds = new DataSet();
            sda.Fill(ds);
            DataTable dt = ds.Tables[0];
            sda.Dispose();

            // Abre uma Stream em memória para escrever o conteudo
            var stream = new MemoryStream();
            Encoding encoding = Encoding.UTF8;
            using (var streamWriter = new StreamWriter(stream, encoding, -1, true))
            {
                // Cabeçalho do arquivo
                string header = string.Empty;

                // Varre as colunas
                foreach (DataColumn dataColumn in dt.Columns)
                {
                    // acrescenta separador - exceto na primeira vez
                    if (!string.IsNullOrEmpty(header))
                        header += ";";

                    // acrescenta o nome da coluna
                    header += dataColumn.ColumnName;
                }

                // escreve uma linha no Strem com o cabeçalho do arquivo
                streamWriter.WriteLine(header);

                // para todas as linhas do data table
                foreach (DataRow dr in dt.Rows)
                {
                    // string para montar o conteudo da linha do arquivo
                    string row = string.Empty;

                    // para todas as colunas
                    for (int index = 0; index < dr.ItemArray.Length; index++)
                    {
                        // acrescenta separador - exceto na primeira coluna
                        if (!string.IsNullOrEmpty(row))
                            row += ";";

                        // pega o valor
                        string value = $"{dr.ItemArray[index].ToString()}";

                        // confere se é data
                        if (DateTime.TryParse(value, out DateTime dateValue) == true)
                            value = dateValue.ToString("dd/MM/yyyy HH:mm");

                        // alimenta a string da linha com o valor do campo
                        row += value;
                    }

                    // escreve uma linha na Stream com os valores dos campos
                    streamWriter.WriteLine(row);
                }
            }

            // Fecha conexão
            conn.Close();

            // Volta a stream para posição inicial
            stream.Position = 0;

            // Devolve o conteudo da stream com header indicando que é um arquivo texto
            string fileName = board.Name.Replace(" ", "_") + ".csv";
            return File(stream, "text/csv", fileName);
        }
	}
}
