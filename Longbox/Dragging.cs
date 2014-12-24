using System;
using System.Drawing;
using System.Windows.Forms;

namespace Longbox
{
    partial class PageView
    {
        private bool _dragging;
        private int _dragStartX;
        private int _dragOffset;
        private int _currentDragOffset;

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
            if (_dragging) UpdateDrag(e.X);
        }

        private void StartDrag(int x)
        {
            _dragStartX = x;
            _currentDragOffset = 0;
            _dragging = true;
        }

        private void EndDrag()
        {
            _dragging = false;
            _dragOffset = OffsetForDisplay();
        }

        private void UpdateDrag(int x)
        {
            _currentDragOffset = x - _dragStartX;
            Refresh();
        }

        private void ResetDrag()
        {
            _dragging = false;
            _dragOffset = 0;
            _currentDragOffset = 0;
            _dragStartX = 0;
        }
        private bool DragHappened(int x)
        {
            return Math.Abs(x - _dragStartX) >= 10;
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
            int off = _dragOffset + _currentDragOffset;
            off = Math.Min(0, off);
            off = Math.Max(-MaxOffset(), off);
            return off;
        }
    }
}
