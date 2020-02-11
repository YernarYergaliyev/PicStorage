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

        // POST api/values
        [HttpPost("upload")]
        public async Task<IActionResult> OnPostUploadAsync(IFormFile file)
        {
            try
            {
                if (file != null && _blobContainer != null)
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

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            Sio.MemoryStream ms = new Sio.MemoryStream();
            if (await _blobContainer.ExistsAsync())
            {
                CloudBlob file = _blobContainer.GetBlobReference(fileName);

                if (await file.ExistsAsync())
                {
                    await file.DownloadToStreamAsync(ms);
                    Sio.Stream blobStream = file.OpenReadAsync().Result;
                    return File(blobStream, file.Properties.ContentType, file.Name);
                }
                else
                {
                    return Content("File does not exist");
                }
            }
            else
            {
                return Content("Container does not exist");
            }
        }


        [Route("delete/{fileName}")]
        [HttpGet]
        public async Task<bool> DeleteFile(string fileName)
        {
            try
            {
                if (await _blobContainer.ExistsAsync())
                {
                    CloudBlob file = _blobContainer.GetBlobReference(fileName);

                    if (await file.ExistsAsync())
                    {
                        await file.DeleteAsync();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        private string GetRandomBlobName(string filename)
        {
            string ext = Sio.Path.GetExtension(filename);
            return string.Format("{0:10}_{1}{2}", DateTime.Now.Ticks, Guid.NewGuid(), ext);
        }
    }
}
