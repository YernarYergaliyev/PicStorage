using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Web;
using Sio = System.IO;
using Microsoft.AspNetCore.Http;

namespace PicStorage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PicturesController : ControllerBase
    {
        static CloudBlobClient _blobClient;
        const string _blobContainerName = "yerpicstorage";
        static CloudBlobContainer _blobContainer;
        public PicturesController()
        {

            string storageConnectionString = "DefaultEndpointsProtocol=https;" +
                "AccountName=yerpicstorage;" +
                "AccountKey=6kdKoTr1idDOomStGKI+vyIMOHEst5i1RRGs9IUyTP7JdSfM3MkRITf2lOAmDv19LCKzVVLgRrFKmPGjQk32TA==;" +
                "EndpointSuffix=core.windows.net";
            CloudStorageAccount storageAccount;

            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                // Create a blob client for interacting with the blob service.
                _blobClient = storageAccount.CreateCloudBlobClient();
                _blobContainer = _blobClient.GetContainerReference(_blobContainerName);
                if (_blobContainer.CreateIfNotExistsAsync().Result)
                {
                    _blobContainer.SetPermissionsAsync(new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    });
                }
            }
        }
        //// GET api/values
        //[HttpGet]
        //public ActionResult<IEnumerable<string>> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        // GET api/values/5
        [HttpGet("{filename}")]
        public async Task<IActionResult> Get(string filename)
        {
            try
            {
                if (filename == null) return BadRequest();
                CloudBlockBlob blob = _blobContainer.GetBlockBlobReference(filename);
                byte[] buf = new byte[4096];

                var dfile = new Sio.MemoryStream(blob.)
                await blob.DownloadToStreamAsync(file);
                return File(file);
            }
            catch
            {
                return NotFound();
            }
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> OnPostUploadAsync(IFormFile file)
        {
            try
            {
                if (file != null)
                {
                    CloudBlockBlob blob = _blobContainer.GetBlockBlobReference(file.FileName);
                    await blob.UploadFromStreamAsync(file.OpenReadStream());
                }
                else
                {
                    return BadRequest("File not loaded!");
                }
                return Ok("Файл загружен!");
            }
            catch
            {
                return NotFound();
            }
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private string GetRandomBlobName(string filename)
        {
            string ext = Sio.Path.GetExtension(filename);
            return string.Format("{0:10}_{1}{2}", DateTime.Now.Ticks, Guid.NewGuid(), ext);
        }
    }
}
