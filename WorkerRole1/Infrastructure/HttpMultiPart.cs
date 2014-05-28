using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Mandro.Utils;

namespace Mandro.Blog.Worker.Infrastructure
{
    public class HttpMultiPart
    {
        public static IEnumerable<File> GetFiles(Stream baseStream, string contentType)
        {
            if (!contentType.StartsWith("multipart/form-data"))
                yield break;

            var multiPartBoundary = Regex.Match(contentType, @"boundary=""?(?<boundary>[^\n\;\"" ]*)").Groups["boundary"].Value;

            var splitableStream = new PatternLimitedStream(baseStream);
            var streamReader = new PatternLimitedStreamReader(splitableStream, Encoding.UTF8);

            streamReader.ReadUntil(multiPartBoundary);
            streamReader.ReadUntil("\n");

            string name = string.Empty;
            while (true)
            {
                var header = streamReader.ReadUntil("\n").Trim();

                if (string.IsNullOrEmpty(header))
                {
                    break;
                }

                if (header.StartsWith("Content-Disposition", StringComparison.CurrentCultureIgnoreCase))
                {
                    name = Regex.Match(header, @"filename=""?(?<name>[^\""]*)", RegexOptions.IgnoreCase).Groups["name"].Value;
                }
            }

            splitableStream.SetPattern("\r\n--" + multiPartBoundary);
            yield return new File { Content = splitableStream, Name = name };
        }

        public class File
        {
            public string Name { get; set; }

            public Stream Content { get; set; }
        }
    }
}