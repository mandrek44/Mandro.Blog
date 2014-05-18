using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;

using Mandro.Blog.Worker.Infrastructure;

using Microsoft.Owin;

namespace Mandro.Blog.Worker.Controllers
{
    public class Home
    {
        private readonly BlogPostsRepository _blogPostsRepository;

        public Home(BlogPostsRepository blogPostsRepository)
        {
            _blogPostsRepository = blogPostsRepository;
        }

        public dynamic GetIndex(dynamic environment)
        {
            var owinContext = environment.Context as IOwinContext;

            return new { Posts = _blogPostsRepository.GetPosts().OrderByDescending(post => post.Created).ToArray(), IsLogged = owinContext.IsSignedIn() };
        }

        public dynamic GetRss(dynamic environment)
        {
            var indexResponse = GetIndex(environment);
            var blogPosts = indexResponse.Posts as BlogPost[];

            var owinContext = environment.Context as IOwinContext;
            owinContext.Response.ContentType = "text/xml";

            var author = new SyndicationPerson("", "Marcin Drobik", "http://marcindrobik.pl");
            var category = new SyndicationCategory("Software Development");
            var markdown = new MarkdownSharp.Markdown();

            var syndicationItems = blogPosts.Select(
                post =>
                {
                    var uri = new Uri("http://marcindrobik.pl/Post/" + post.Permalink);

                    var item = new SyndicationItem();
                    item.Authors.Add(author);
                    item.Categories.Add(category);
                    item.Links.Add(SyndicationLink.CreateAlternateLink(uri));
                    item.Summary = TextSyndicationContent.CreatePlaintextContent(markdown.Transform(post.Content));
                    item.PublishDate = post.Created;
                    item.Title = TextSyndicationContent.CreatePlaintextContent(post.Title);
                    item.BaseUri = uri;

                    return item;
                });

            
            var feed = new SyndicationFeed();
            feed.Title = TextSyndicationContent.CreatePlaintextContent("marcindrobik.pl");
            feed.Description = TextSyndicationContent.CreatePlaintextContent("software journeyman notes");
            feed.Links.Add(SyndicationLink.CreateAlternateLink(new Uri("http://marcindrobik.pl")));
            feed.Authors.Add(author);
            feed.Categories.Add(category);
            feed.Language = "en-US";
            feed.Items = syndicationItems;

            var stringBuilder = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(stringBuilder, null);
            feed.SaveAsRss20(writer);
            writer.Flush();

            return new { Content = stringBuilder.ToString() };
        }
    }
}