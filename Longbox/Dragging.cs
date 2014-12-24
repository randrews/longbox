using System;
using System.Drawing;
using System.Windows.Forms;

namespace Longbox
{
    partial class PageView
    {
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
