using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ContactCenter.Helpers;
using ContactCenter.Infrastructure.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ContactCenter.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FilesController : ApiControllerBase
    {

        private readonly BlobContainerClient _blobContainerClient;
        private readonly IConfiguration _configuration;

        public FilesController(BlobContainerClient blobContainerClient, IConfiguration configuration)
        {
            _blobContainerClient = blobContainerClient;
            _configuration = configuration;
        }

        // Faz o upload de um novo arquivo para o Storage, gerando um nome unico, e devolvendo o nome
        [HttpPost]
        public async Task<ActionResult> Post(IFormFile file)
        {

            string groupFileName = string.Empty;
            string fileName = string.Empty;

            // Confere se recebeu arquivo
            if (file != null)
            {
                // Cria um stream em memoria
                using (Stream memoryStream = new MemoryStream())
                {
                    // Copia o arquivo para o stream
                    await file.CopyToAsync(memoryStream).ConfigureAwait(false);

                    // Gera nome unico
                    groupFileName = Utility.UniqueFileName(file.FileName.ToLower(), AuthorizedGroupId());

                    // Get Content Type
                    string contentType = Utility.GetContentType(file.FileName);

                    // Copia o stream para o Blob Storage
                    if (_blobContainerClient != null)
                    {
                        memoryStream.Position = 0;
                        var blobHttpHeader = new BlobHttpHeaders();
                        blobHttpHeader.ContentType = contentType;
                        BlobClient blob = _blobContainerClient.GetBlobClient(groupFileName);
                        await blob.DeleteIfExistsAsync().ConfigureAwait(false);
                        await blob.UploadAsync(memoryStream, blobHttpHeader).ConfigureAwait(false);
                    }

                }

                // Devolve OK com o nome do arquivo, tirando o folder do grupo
                if (groupFileName.Contains("/"))
                    fileName = groupFileName.Split("/")[1];
                else
                    fileName = groupFileName;

                return Ok(new { fileName });

            }
            else
            {
                string error = "Não foi enviado o arquivo. Faça a requisição com Form-Data no Body e um campo com nome 'file' contendo o arquivo.";
                return BadRequest( new {error});
            }

        }

        // Faz o upload de um arquivo para o Storage, mas mantem o nome, e deleta se tiver outro antigo com o mesmo nome
        [HttpPut]
        public async Task<ActionResult> Put(IFormFile file)
        {

            string groupFileName = string.Empty;
            string fileName = string.Empty;

            // Confere se recebeu arquivo
            if (file != null)
            {
                // Cria um stream em memoria
                using (Stream memoryStream = new MemoryStream())
                {
                    // Copia o arquivo para o stream
                    await file.CopyToAsync(memoryStream).ConfigureAwait(false);

                    // Gera nome unico
                    groupFileName = AuthorizedGroupId() + "/" + file.FileName.ToLower();

                    // Copia o stream para o Blob Storage
                    if (_blobContainerClient != null)
                    {
                        memoryStream.Position = 0;
                        BlobClient blob = _blobContainerClient.GetBlobClient(groupFileName);
                        await blob.DeleteIfExistsAsync().ConfigureAwait(false);
                        await blob.UploadAsync(memoryStream).ConfigureAwait(false);
                    }

                }

                // Devolve OK com o nome do arquivo, tirando o folder do grupo
                if (groupFileName.Contains("/"))
                    fileName = groupFileName.Split("/")[1];
                else
                    fileName = groupFileName;

                return Ok(new { fileName });

            }
            else
            {
                string error = "Não foi enviado o arquivo. Faça a requisição com Form-Data no Body e um campo com nome 'file' contendo o arquivo.";
                return BadRequest(error);
            }

        }

        [HttpDelete]
        public async Task<ActionResult> Delete(string fileName)
        {

            // Converte espaços para sinal de + ( codificação HTTP )
            fileName = HttpUtility.UrlEncode(fileName);

            // Confere se recebeu arquivo
            if (string.IsNullOrEmpty(fileName))
            {
                string error = "Nome do arquivo não foi informado.";
                return BadRequest(error);
            }
            else
            {
                // Monta o nome, com o caminho do grupo
                string groupFileName = $"{AuthorizedGroupId()}/{fileName}";

                // Confere se temos aceso ao blobContainer
                if (_blobContainerClient != null)
                {
                    // Aponta para o arquivo
                    BlobClient blob = _blobContainerClient.GetBlobClient(groupFileName);
                    // Confere se o arquivo existe
                    if (await blob.ExistsAsync())
                    {
                        // Exclui o arquivo
                        await blob.DeleteIfExistsAsync().ConfigureAwait(false);
                        return Ok();
                    }
                    else
                        // Erro que não achou
                        return NotFound();
                }
                else
                    // Erro na inicialização do Blob Storage
                    throw (new SystemException("_blobContainerClient nulo"));
            }

        }
        [HttpGet]
        public async Task<ActionResult> GetUrl(string fileName)
        {
            // Converte espaços para sinal de + ( codificação HTTP )
            fileName = HttpUtility.UrlEncode(fileName);

            // Confere se recebeu arquivo
            if (string.IsNullOrEmpty(fileName))
            {
                string error = "Nome do arquivo não foi informado.";
                return BadRequest(error);
            }
            else
            {
                // Monta o nome, com o caminho do grupo
                string groupFileName = $"{AuthorizedGroupId()}/{fileName}";

                // Confere se temos aceso ao blobContainer
                if (_blobContainerClient != null)
                {
                    // Aponta para o arquivo
                    BlobClient blob = _blobContainerClient.GetBlobClient(groupFileName);
                    // Confere se o arquivo existe
                    if (await blob.ExistsAsync())
                    {
                        // Monta a URL
                        string url = Utility.CombineUrlsToString(_configuration.GetValue<string>("FileContainerUrl"), groupFileName);
                        return Ok(new { url });
                    }
                    else
                        // Erro que não achou
                        return NotFound();
                }
                else
                    // Erro na inicialização do Blob Storage
                    throw (new SystemException("_blobContainerClient nulo"));
            }

        }
    }
}