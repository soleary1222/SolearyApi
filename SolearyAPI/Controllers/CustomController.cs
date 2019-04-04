using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using SolearyAPI.Models;
using SolearyAPI.Lib;
using RestSharp;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;

namespace SolearyAPI.Controllers
{

    [RoutePrefix("api/Custom")]
    public class CustomController : ApiController
    {

        private ServiceContext db = new ServiceContext();

        [Route("NotifyUser")]
        [HttpPost]
        public async Task<IHttpActionResult> NotifyUser(NotifcationBindingModel nbm)
        {
            IRestResponse res = Mailer.SendSimpleMessage(nbm.Email, nbm.NotificationType);
            if (res.IsSuccessful)
            {
                return Ok();
            } else
            {
                return BadRequest(res.ErrorMessage);
            }
                
        }

        [Route("UploadProfileImage/{id}")]
        [HttpPost]
        public async Task<IHttpActionResult> UploadProfileImage([FromUri] string id)
        {

            AzureStorage az = new AzureStorage("profile-images");
            CloudBlobContainer container = new CloudBlobContainer(new Uri(az.GetContainerSasUri("profile-images")));
            string imageurl = "";

            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.UnsupportedMediaType));
            }

            Request.Content.LoadIntoBufferAsync().Wait();
            imageurl = await Request.Content.ReadAsMultipartAsync(new MultipartMemoryStreamProvider()).ContinueWith(async (task) =>
            {

                MultipartMemoryStreamProvider provider = task.Result;

                foreach (HttpContent content in provider.Contents)
                {
                    if (content.Headers.ContentType == null)
                    {
                        continue;
                    }
                    Stream image = content.ReadAsStreamAsync().Result;
                    string filename = content.Headers.ContentDisposition.FileName.Trim().Replace("\"", "");
                    CloudBlockBlob blob = container.GetBlockBlobReference(filename);

                    string ext = filename.Substring(filename.LastIndexOf('.') + 1);
                    switch (ext.ToLower())
                    {
                        case "pdf":
                            blob.Properties.ContentType = "application/pdf";
                            break;
                        case "js":
                            blob.Properties.ContentType = "text/javascript";
                            break;
                        case "css":
                            blob.Properties.ContentType = "text/css";
                            break;
                        case "png":
                            blob.Properties.ContentType = "image/png";
                            break;
                        case "gif":
                            blob.Properties.ContentType = "image/gif";
                            break;
                        case "jpg":
                            blob.Properties.ContentType = "image/jpg";
                            break;
                        case "bmp":
                            blob.Properties.ContentType = "image/bmp";
                            break;
                        case "html":
                            blob.Properties.ContentType = "text/html";
                            break;
                        default:
                            blob.Properties.ContentType = "application/octet-stream";
                            break;

                    }
                    try
                    {
                        await blob.UploadFromStreamAsync(image);
                        imageurl = blob.Uri.AbsoluteUri;

                        //save to db
                        Guid uid = Guid.Parse(id);
                        User user = db.Users.FirstOrDefault(x => x.UserID == uid);
                        if (user != null)
                        {
                            user.ProfileImageUrl = imageurl;
                        }
                        db.SaveChanges();

                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine ("Write operation failed for SAS " + sasurl);
                        //Console.WriteLine ("Additional error information: " + e.Message);
                        //Console.WriteLine ();
                    }
                }
                return imageurl;
            }).Result;

            return Ok(imageurl);
        }

    }
}
