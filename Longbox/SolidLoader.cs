using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SharpCompress.Archive;
using SharpCompress.Reader;

namespace Longbox
{
    class SolidLoader : IPageLoader
    {
        private const int Cachesize = 10;

        public int NumPages { get; private set; }

        private string _filename;
        private int _pageCursor;
        private Dictionary<int, Task<Image>> _cache;
        private List<int> _cachedPages;
        private IReader _reader;

        public static bool IsSolid(string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                var arch = ArchiveFactory.Open(stream);
                return arch.IsSolid;
            }
        }

        public void OpenComic(string filename)
        {
            _filename = filename;


            var stream = File.OpenRead(_filename);
            var arch = ArchiveFactory.Open(stream);

            NumPages = arch.Entries.Where(e => IsImage(e.FilePath)).
                OrderBy(e => e.FilePath).
                Count();

            ReopenFile();

            _cachedPages = new List<int>();
            _cache = new Dictionary<int, Task<Image>>();
        }

        public Task<Image> LoadPage(int num)
        {
            // If it's a bad page, do nothing:
            if (num < 0 || num >= NumPages) return NullTask();

            // If we have this cached, then return it (and mark it as most recently used):
            if (_cache.ContainsKey(num))
            {
                _cachedPages.Remove(num);
                _cachedPages.Add(num);
                return _cache[num];
            }

            // Otherwise we have to load it. We may need to reload the file to load it:
            if (num < _pageCursor) ReopenFile();

            // If we're under the cache limit already, then go ahead and load it in:
            if (_cachedPages.Count < Cachesize)
            {
                _cache[num] = PageTask(num);
                _cachedPages.Add(num);
                return _cache[num];
            }

            // Nope, we have to toss something. Toss the least-recently-used page:
            _cache.Remove(_cachedPages[0]);
            _cachedPages.RemoveAt(0);

            // Now we're under the cache limit again, so try again to load it:
            return LoadPage(num);
        }

        private void ReopenFile()
        {
            var stream = File.OpenRead(_filename);
            _reader = ReaderFactory.Open(stream);
            _reader.MoveToNextEntry();
            _pageCursor = 0;
        }

        private Task<Image> PageTask(int num)
        {
            var t = new Task<Image>(
                () =>
                {
                    lock (this)
                    {
                        if (num >= NumPages || num < 0) return null;

                        while (_pageCursor < num)
                        {
                            _reader.MoveToNextEntry();
                            if (IsImage(_reader.Entry.FilePath)) _pageCursor++;
                        }

                        using (var str = _reader.OpenEntryStream())
                        {
                            return Image.FromStream(str);
                        }
                    }
                });

            t.Start();
            return t;
        }

        private bool IsImage(string s)
        {
            return s.EndsWith(".jpg") ||
                    s.EndsWith(".jpeg") ||
                    s.EndsWith(".png") ||
                    s.EndsWith(".gif") ||
                    s.EndsWith(".bmp");
        }

        private Task<Image> NullTask()
        {
            var t = new Task<Image>(() => null);
            t.Start();
            return t;
        }
    }
}
