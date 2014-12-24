using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpCompress.Archive;

namespace Longbox
{

    class CBZLoader : IPageLoader
    {
        private IArchiveEntry[] Pages { get; set; }
        private int CurrentPageNumber { get; set; }
        private Task<Image> CurrentPage { get; set; }
        private Task<Image> PrevPage { get; set; }
        private Task<Image> NextPage { get; set; }
        public int NumPages { get { return Pages == null ? 0 : Pages.Length; } }

        public Task<Image> LoadPage(int num)
        {
            if (num < 0 || num >= NumPages) return NullTask();

            if (num == CurrentPageNumber) return CurrentPage;
            if (num == CurrentPageNumber - 1)
            {
                var prev = PrevPage;
                NextPage = CurrentPage;
                CurrentPage = PrevPage;
                PrevPage = PageTask(CurrentPageNumber - 2);
                CurrentPageNumber--;
                return prev;
            }
            if (num == CurrentPageNumber + 1)
            {
                var next = NextPage;
                PrevPage = CurrentPage;
                CurrentPage = NextPage;
                NextPage = PageTask(CurrentPageNumber + 2);
                CurrentPageNumber++;
                return next;
            }
            return PageTask(num);
        }

        private Task<Image> PageTask(int num)
        {
            var t = new Task<Image>(
                () =>
                {
                    lock (this)
                    {
                        if (num >= NumPages || num < 0) return null;
                        using (var str = Pages[num].OpenEntryStream())
                            return Image.FromStream(str);
                    }
                });

            t.Start();
            return t;
        }

        private Task<Image> NullTask()
        {
            var t = new Task<Image>(() => null);
            t.Start();
            return t;
        }

        public async void OpenComic(string filename)
        {
            var stream = System.IO.File.OpenRead(filename);
            var arch = ArchiveFactory.Open(stream);
            Func<string, bool> isImage = s => s.EndsWith(".jpg") ||
                                              s.EndsWith(".jpeg") ||
                                              s.EndsWith(".png") ||
                                              s.EndsWith(".gif") ||
                                              s.EndsWith(".bmp");

            Pages = arch.Entries.Where(e => isImage(e.FilePath)).
                OrderBy(e => e.FilePath).
                ToArray();

            CurrentPageNumber = 0;
            CurrentPage = PageTask(0);
            NextPage = PageTask(1);
        }
    }
}
