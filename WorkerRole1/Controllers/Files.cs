using Mandro.Owin.SimpleMVC;
using Mandro.Utils.Web;

using Microsoft.Owin;

namespace Mandro.Blog.Worker.Controllers
{
    public class Files
    {
        private readonly IFilesRepository _repository;

        public Files(IFilesRepository repository)
        {
            _repository = repository;
        }

        [Authorize]
        public dynamic GetIndex(dynamic context)
        {
            return new { Files = _repository.GetAllFiles() };
        }

        [Authorize]
        public dynamic GetUpload(dynamic context)
        {
            return new { };
        }

        [Authorize]
        public dynamic PostUpload(dynamic context)
        {
            var owinContext = context.Context as OwinContext;

            if (!owinContext.Request.ContentType.StartsWith("multipart/form-data"))
                return Redirect.To<Files>(controller => controller.GetIndex);

            var multiPartStream = new MultiPartFormDataStream(owinContext.Request.Body, owinContext.Request.ContentType);

            while (multiPartStream.SeekNextFile())
            {
                _repository.AddFile(multiPartStream, multiPartStream.CurrentFileName);
            }

            return Redirect.To<Files>(controller => controller.GetIndex);
        }
    }
}