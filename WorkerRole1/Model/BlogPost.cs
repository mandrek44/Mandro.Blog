using System;

using Mandro.Blog.Worker.Infrastructure;

namespace Mandro.Blog.Worker
{
    public class BlogPost : RandomIdTableEntity
    {
        public string Title { get; set; }

        public string Content { get; set; }

        public DateTime Created { get; set; }

        public string Permalink { get; set; }

        public string Category { get; set; }
    }
}