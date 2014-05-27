using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mandro.Blog.Worker.Model;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Mandro.Blog.Worker.Controllers
{
    public interface IFilesRepository
    {
        IEnumerable<BlogFile> GetAllFiles();

        void AddFile(Stream toArray, string name);
    }

    class FilesRepository : IFilesRepository
    {
        private CloudBlobContainer _blogFilesContainer;

        public FilesRepository()
        {
            
        }


        public IEnumerable<BlogFile> GetAllFiles()
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("BlogStorage"));
            var blogClient = storageAccount.CreateCloudBlobClient();
            _blogFilesContainer = blogClient.GetContainerReference("blogfiles");

            if (_blogFilesContainer.CreateIfNotExists())
            {
                // configure container for public access
                var permissions = _blogFilesContainer.GetPermissions();
                permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                _blogFilesContainer.SetPermissions(permissions);
            }

            return _blogFilesContainer.ListBlobs().Select(blob => new BlogFile { Uri = blob.Uri });
        }

        public void AddFile(Stream toArray, string name)
        {
            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("BlogStorage"));
            var blogClient = storageAccount.CreateCloudBlobClient();
            _blogFilesContainer = blogClient.GetContainerReference("blogfiles");

            var blob = _blogFilesContainer.GetBlockBlobReference(DateTime.Now.ToString("yyyy-MMMM-dd-") + Guid.NewGuid() + Path.GetExtension(name));
            blob.UploadFromStream(toArray);
        }
    }
}