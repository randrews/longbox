using System.Drawing;
using System.Windows.Forms;

namespace Longbox
{
    partial class PageView
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (CurrentPage != null)
                e.Graphics.DrawImage(CurrentPage, FindRect(CurrentPage));
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
            left = Width > size.Width ? (int)(Width - size.Width) / 2 : OffsetForDisplay();

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
    }
}
