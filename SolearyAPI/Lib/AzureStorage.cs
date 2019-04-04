using System;
using System.Collections.Generic;
using System.Configuration;

using System.IO;
using System.Net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;


namespace SolearyAPI.Lib
{
    class AzureStorage
    {
        // Variables for the cloud storage objects.
        CloudStorageAccount cloudStorageAccount;
        CloudBlobClient blobClient;
        CloudBlobContainer blobContainer;
        BlobContainerPermissions containerPermissions;
        //CloudBlob blob;

        private string _container;

        string distributionPath = ConfigurationManager.AppSettings["CDNUrl"]; 
        string connectionstring = ConfigurationManager.AppSettings["AzureConnectionString"];

        public AzureStorage(string container)
        {
            //string s = "DefaultEndpointsProtocol=http;AccountName=" + accountName + ";AccountKey=" + accessKey;
            cloudStorageAccount = CloudStorageAccount.Parse(connectionstring);

            // Create the blob client, which provides
            // authenticated access to the Blob service.
            blobClient = cloudStorageAccount.CreateCloudBlobClient();

            // Get the container reference.
            blobContainer = blobClient.GetContainerReference(container.ToLower());
            this._container = container.ToLower();
        }
        
        public string UploadFile(byte[] content, string key) //key can be anything you use...file+extension is fine
        {
            // Create the container if it does not exist.
            blobContainer.CreateIfNotExists(BlobContainerPublicAccessType.Container);

            // Set permissions on the container.
            containerPermissions = new BlobContainerPermissions();

            // This sample sets the container to have public blobs. Your application
            // needs may be different. See the documentation for BlobContainerPermissions
            // for more information about blob container permissions.
            containerPermissions.PublicAccess = BlobContainerPublicAccessType.Blob;
            blobContainer.SetPermissions(containerPermissions);

            // Get a reference to the blob.
            //blob = blobContainer.GetBlobReference(key);

            CloudBlockBlob blob = blobContainer.GetBlockBlobReference(key);

            blob.DeleteIfExists();

            //blob.UploadFile(filePath);
            //blob.UploadFromStream(filestream);
            blob.Properties.ContentType = "application/octet-stream";


            /** PUTBLOCK START **/

            int maxSize = 1 * 1024 * 1024; // 4 MB

            if (content.Length > maxSize)
            {
                byte[] data = content;

                int id = 0;
                int byteslength = (int)content.Length;
                int bytesread = 0;
                int index = 0;
                List<string> blocklist = new List<string>();
                int numBytesPerChunk = 250 * 1024; //250KB per block

                do
                {
                    byte[] buffer = new byte[numBytesPerChunk];
                    int limit = index + numBytesPerChunk;
                    for (int loops = 0; index < limit; index++)
                    {
                        buffer[loops] = data[index];
                        loops++;
                    }
                    bytesread = index;
                    string blockIdBase64 = Convert.ToBase64String(System.BitConverter.GetBytes(id));

                    blob.PutBlock(blockIdBase64, new MemoryStream(buffer, true), null);
                    blocklist.Add(blockIdBase64);
                    id++;
                } while (byteslength - bytesread > numBytesPerChunk);

                int final = byteslength - bytesread;
                byte[] finalbuffer = new byte[final];
                for (int loops = 0; index < byteslength; index++)
                {
                    finalbuffer[loops] = data[index];
                    loops++;
                }
                string blockId = Convert.ToBase64String(System.BitConverter.GetBytes(id));
                blob.PutBlock(blockId, new MemoryStream(finalbuffer, true), null);
                blocklist.Add(blockId);

                blob.PutBlockList(blocklist);
            }
            else
            {
                blob.UploadFromByteArray(content, 0, content.Length);
                //blob.UploadFromStream(filestream);
            }

            /** PUTBLOCK END **/

            string ext = key.Substring(key.LastIndexOf('.') + 1);

            //string ext = key.Split('.')[1];

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
            blob.SetProperties();
            return ConfigurationManager.AppSettings["CDNUrl"] + "/" + _container + "/" + key;

        }
        
        public int DownloadFileCDN(string downloadTo, string key)
        {
            int x = 0;
            try
            {
                string sURL;
                sURL = distributionPath + _container + "/" + key;

                WebRequest wrGETURL;
                wrGETURL = WebRequest.Create(sURL);
                WebProxy myProxy = new WebProxy("myproxy", 80);
                myProxy.BypassProxyOnLocal = true;
                wrGETURL.Proxy = WebProxy.GetDefaultProxy();
                Stream s;
                s = wrGETURL.GetResponse().GetResponseStream();

                //Creating file on computer
                using (FileStream fs = File.Create(downloadTo))
                {
                    const int BUFSIZE = 4096;
                    byte[] buf = new byte[BUFSIZE];
                    int n = 1;
                    while (n != 0)
                    {
                        n = s.Read(buf, 0, BUFSIZE);
                        if (n == 0) break;
                        x += n;
                        fs.Write(buf, 0, n);
                    }
                    s.Close();
                    fs.Close();
                }
            }
            catch (NullReferenceException)
            {
                throw new NullReferenceException("Problems connecting to web server.");
            }
            catch (System.Net.WebException)
            {
                throw new Exception("Unable to connect to web server.");
            }
            return x;
        }


        public string GetContainerSasUri(string con)
        {
            try
            {
                //Get a reference to a blob within the container.
                CloudBlobContainer container = blobClient.GetContainerReference(con.ToLower());
                container.CreateIfNotExists();

                //Set the expiry time and permissions for the blob.
                //In this case the start time is specified as a few minutes in the past, to mitigate clock skew.
                //The shared access signature will be valid immediately.
                SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
                sasConstraints.SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5);
                sasConstraints.SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24);
                sasConstraints.Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write;

                //Generate the shared access signature on the blob, setting the constraints directly on the signature.
                string sasBlobToken = container.GetSharedAccessSignature(sasConstraints);

                //Return the URI string for the container, including the SAS token.
                return container.Uri + sasBlobToken;
            }
            catch (Exception e)
            {
                return "";
            }
        }

    }
}