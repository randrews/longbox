using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpCompress.Archive;

namespace Longbox
{
    public partial class PageView : UserControl
    {
        private bool Dragging;
        private int DragStartX;
        private int DragOffset;
        private int CurrentDragOffset;

        private string Filename { get; set; }
        private IArchiveEntry[] Pages { get; set; }
        private int CurrentPageNumber { get; set; }
        private Image CurrentPage { get; set; }
        private Task<Image> NextPage { get; set; }
        private Task<Image> PrevPage { get; set; }
        public MainWindow Window { get; set; }

        public PageView()
        {
            InitializeComponent();
            ResizeRedraw = true;
            ResetDrag();
        }

        public async void OpenComic(string filename)
        {
            Filename = filename;
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
            CurrentPage = await LoadPage(0);
            NextPage = LoadPage(1);
            SetPage(0);
            Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (CurrentPage != null)
            {
                e.Graphics.DrawImage(CurrentPage, FindRect(CurrentPage));
            }
        }

        private Task<Image> LoadPage(int num)
        {
            var t = new Task<Image>(
                () =>
                {
                    lock (this)
                    {
                        if (num >= Pages.Length || num < 0) return null;
                        using (var str = Pages[num].OpenEntryStream())
                            return Image.FromStream(str);
                    }
                });

            t.Start();
            return t;
        }

        private async void SetPage(int newPageNumber)
        {
            if (Pages == null || newPageNumber < 0 || newPageNumber >= Pages.Length) return;
            Window.setPageLabel(string.Format("{0} / {1}", newPageNumber + 1, Pages.Length));
            if (newPageNumber == CurrentPageNumber) return;
            Image curr = CurrentPage;
            ResetDrag();

            // We're just going to the next page
            if (newPageNumber == CurrentPageNumber + 1)
            {
                Image next = await NextPage;
                if (next == null) return;
                PrevPage = new Task<Image>( () => curr ); PrevPage.Start();
                CurrentPage = next;
                Refresh();
                CurrentPageNumber = newPageNumber;
                NextPage = LoadPage(CurrentPageNumber + 1);
                return;
            }

            // we're going to the previous page
            if (newPageNumber == CurrentPageNumber - 1)
            {
                Image prev = await PrevPage;
                NextPage = new Task<Image>( () => curr ); NextPage.Start();
                if (prev == null) return;
                CurrentPage = prev;
                Refresh();
                CurrentPageNumber = newPageNumber;
                PrevPage = LoadPage(CurrentPageNumber - 1);
                return;
            }

            // Nope, we're just going to an arbitrary page.
            var newCurrent = LoadPage(newPageNumber);
            NextPage = LoadPage(newPageNumber + 1);
            PrevPage = LoadPage(newPageNumber - 1);
            CurrentPage = await newCurrent;
            Refresh();
            CurrentPageNumber = newPageNumber;
        }

        public void TurnPage()
        {
            SetPage(CurrentPageNumber+1);
        }

        public void TurnPageBack()
        {
            SetPage(CurrentPageNumber-1);
        }

        private void HandleMouseClick(object sender, MouseEventArgs e)
        {
            if (DragHappened(e.X)) return;
            float x = (float) (e.X) / Width;
            float y = (float) (e.Y) / Height;

            if(x <= 0.25) TurnPageBack();
            else if (x >= 0.75) TurnPage();
            else Window.ToggleMenuBar();
        }

        private Rectangle FindRect(Image img)
        {
            var aspectRatio = (float)img.Width / img.Height;

            if (aspectRatio <= 1)
                return SinglePageRectangle(img);
            return MultiPageRectangle(img);
        }

        private Rectangle MultiPageRectangle(Image img)
        {
            var aspectRatio = (float)img.Width / img.Height;
            var size = new SizeF(Height * aspectRatio, Height);

            int left;
            left = Width > size.Width ? (int) (Width - size.Width)/2 : OffsetForDisplay();

            return new Rectangle(left, 0, (int)size.Width, (int)size.Height);
        }

        private Rectangle SinglePageRectangle(Image img)
        {
            var aspectRatio = (float)img.Width / img.Height;
            var heightScale = (float)img.Height / Height;
            var widthScale = (float)img.Width / Width;

            var size = heightScale >= widthScale
                ? new SizeF(Height * aspectRatio, Height)
                : new SizeF(Width, Width / aspectRatio);

            var left = (int)(Width - size.Width) / 2;
            var top = (int)(Height - size.Height) / 2;
            return new Rectangle(left, top, (int)size.Width, (int)size.Height);
        }

        private void PageView_MouseDown(object sender, MouseEventArgs e)
        {
            StartDrag(e.X);
        }

        private void PageView_MouseUp(object sender, MouseEventArgs e)
        {
            EndDrag();
        }

        private void PageView_MouseMove(object sender, MouseEventArgs e)
        {
            if (Dragging) UpdateDrag(e.X);
        }

        private void StartDrag(int x)
        {
            DragStartX = x;
            CurrentDragOffset = 0;
            Dragging = true;
        }

        private void EndDrag()
        {
            Dragging = false;
            DragOffset = OffsetForDisplay();
        }

        private void UpdateDrag(int x)
        {
            CurrentDragOffset = x - DragStartX;
            Refresh();
        }

        private void ResetDrag()
        {
            Dragging = false;
            DragOffset = 0;
            CurrentDragOffset = 0;
            DragStartX = 0;
        }
        private bool DragHappened(int x)
        {
            return Math.Abs(x - DragStartX) >= 10;
        }

        private int MaxOffset()
        {
            if (CurrentPage == null) return 0;
            var aspectRatio = (float)CurrentPage.Width / CurrentPage.Height;
            var size = new SizeF(Height * aspectRatio, Height);

            return Math.Max(0, (int)size.Width - Width);
        }

        private int OffsetForDisplay()
        {
            int off = DragOffset + CurrentDragOffset;
            off = Math.Min(0, off);
            off = Math.Max(-MaxOffset(), off);
            return off;
        }
    }
}
