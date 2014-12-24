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
using System.Windows.Forms.VisualStyles;
using SharpCompress.Archive;

namespace Longbox
{
    public partial class PageView : UserControl
    {
        internal IPageLoader PageLoader { get; set; }
        private int CurrentPageNumber { get; set; }
        private Image CurrentPage { get; set; }
        public MainWindow Window { get; set; }

        public PageView()
        {
            InitializeComponent();
            ResizeRedraw = true;
            ResetDrag();
        }

        public void OpenComic(string filename)
        {
            PageLoader = new CBZLoader();
            PageLoader.OpenComic(filename);
            CurrentPageNumber = 0;
            SetPage(0);
            Refresh();
        }

        private async void SetPage(int newPageNumber)
        {
            if (PageLoader == null) return;
            var newPage = await PageLoader.LoadPage(newPageNumber);
            if (newPage == null) return;
            CurrentPage = newPage;
            CurrentPageNumber = newPageNumber;
            ResetDrag();

            Window.setPageLabel(string.Format("{0} / {1}", CurrentPageNumber + 1, PageLoader.NumPages));
            Refresh();
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
    }
}
