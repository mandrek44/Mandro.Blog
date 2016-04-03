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

            return new { Posts = _blogPostsRepository.GetPosts().OrderByDescending(post => post.Created).Take(2).ToArray(), IsLogged = owinContext.IsSignedIn() };
        }

        public dynamic GetAll(dynamic environment)
        {
            var owinContext = environment.Context as IOwinContext;

            return new { Posts = _blogPostsRepository.GetPosts().OrderByDescending(post => post.Created).ToArray(), IsLogged = owinContext.IsSignedIn() };
        }

        public dynamic GetRss(dynamic environment)
        {
            var indexResponse = GetIndex(environment);
            var blogPosts = indexResponse.Posts as BlogPost[];

            var owinContext = environment.Context as IOwinContext;
            owinContext.Response.ContentType = "application/rss+xml; charset=utf-8";

            var author = new SyndicationPerson("admin@marcindrobik.pl", "Marcin Drobik", "http://marcindrobik.pl");

            var defaultCategory = new SyndicationCategory("Software Development");
            var dajsiepoznacCategory = new SyndicationCategory("dajsiepoznac");
            List<SyndicationCategory> categories = new List<SyndicationCategory>()
            {
                defaultCategory,
                dajsiepoznacCategory
            };

            var markdown = new MarkdownSharp.Markdown();

            var syndicationItems = blogPosts.Select(
                post =>
                {
                    var uri = new Uri("http://marcindrobik.pl/Post/" + post.Permalink);

                    var item = new SyndicationItem();
                    item.Authors.Add(author);

                    var existingCategory = categories.FirstOrDefault(c => c.Name == post.Category);
                    if (existingCategory == null)
                    {
                        if (!string.IsNullOrEmpty(post.Category))
                        {
                            existingCategory = new SyndicationCategory(post.Category);
                            categories.Add(existingCategory);
                        }
                        else
                        {
                            existingCategory = defaultCategory;
                        }
                    }


                    item.Categories.Add(existingCategory);
                    if (existingCategory.Name == "Stratosphere.NET")
                        item.Categories.Add(dajsiepoznacCategory);

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
            feed.Categories.Add(defaultCategory);
            feed.Language = "en-US";
            feed.Items = syndicationItems;

            using (var stream = new MemoryStream())
            {
                var settings = new XmlWriterSettings { Encoding = Encoding.UTF8, NewLineHandling = NewLineHandling.Entitize, NewLineOnAttributes = true, Indent = true };
                var writer = XmlWriter.Create(stream, settings);
                feed.SaveAsRss20(writer);
                writer.Flush();

                return stream.ToArray();
            }
        }
    }
}