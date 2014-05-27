using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mandro.Blog.Worker.Infrastructure
{
    /// <summary>
    /// Class that exposes methods to read consecutive data from PatternLimitedStream.
    /// </summary>
    public class PatternLimitedStreamReader
    {
        private readonly PatternLimitedStream _patternLimitedStream;
        private readonly Encoding _encoding;

        public PatternLimitedStreamReader(PatternLimitedStream patternLimitedStream, Encoding encoding)
        {
            _patternLimitedStream = patternLimitedStream;
            _encoding = encoding;
        }

        public byte[] ReadUntil(byte[] pattern)
        {
            _patternLimitedStream.SetPattern(pattern);

            return ReadFully(_patternLimitedStream);
        }

        public string ReadUntil(string pattern)
        {
            return _encoding.GetString(ReadUntil(_encoding.GetBytes(pattern)));
        }

        private static byte[] ReadFully(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }

    /// <summary>
    /// Represents stream that when pattern is found.
    /// To continue reading call SearchAgain or SetPattern.
    /// </summary>
    public class PatternLimitedStream : Stream
    {
        private readonly Stream _baseStream;
        private byte[] _pattern;
        private readonly List<byte> _searchBuffer;

        private bool _finished;

        public PatternLimitedStream(Stream baseStream)
        {
            _baseStream = baseStream;
            _searchBuffer = new List<byte>();
            _pattern = null;
            _finished = false;
        }

        public void SetPattern(byte[] pattern)
        {
            _pattern = pattern;
            _finished = false;
        }

        public void SetPattern(string pattern)
        {
            SetPattern(Encoding.UTF8.GetBytes(pattern));
        }

        public void SearchAgain()
        {
            _finished = false;
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int requestedCount)
        {
            var matchPattern = _pattern != null && _pattern.Length != 0;
            if (!matchPattern && _searchBuffer.Count == 0)
            {
                return _baseStream.Read(buffer, offset, requestedCount);
            }

            if (matchPattern && _finished) return 0;

            var patternLength = matchPattern ? _pattern.Length : 0;

            var realRequestedCount = requestedCount + patternLength - _searchBuffer.Count;
            var internalBuffer = new byte[realRequestedCount];
            var bytesRead = _baseStream.Read(internalBuffer, 0, realRequestedCount);
            if (bytesRead == 0 && _searchBuffer.Count == 0) return 0;

            _searchBuffer.AddRange(internalBuffer.Take(bytesRead));

            var find = matchPattern ? _searchBuffer.IndexOf(_pattern) : -1;
            if (find == -1)
            {
                var bytesToCopy = Math.Min(_searchBuffer.Count - patternLength, requestedCount);

                _searchBuffer.CopyTo(0, buffer, offset, bytesToCopy);
                _searchBuffer.RemoveRange(0, bytesToCopy);

                return bytesToCopy;
            }
            else
            {
                _finished = true;
                _searchBuffer.CopyTo(0, buffer, offset, find);
                _searchBuffer.RemoveRange(0, find + patternLength);                
                return find;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead
        {
            get
            {
                return _baseStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                return _baseStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return _baseStream.Position;
            }
            set
            {
                _baseStream.Position = value;
            }
        }
    }
}