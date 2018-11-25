using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;

namespace YTUploader.Thumbnail
{
    public class ThumbnailGenerator
    {
        private readonly Bitmap baseBmp;

        public ThumbnailGenerator()
        {
            baseBmp = new Bitmap(Image.FromFile(@".\octoawesome.png"));
        }

        public Image GenerateThumbnail(string version)
        {
            var bmp = new Bitmap(baseBmp);

            using (var b = new SolidBrush(Color.FromArgb(255, 36, 163, 179)))
            using (var pen = new Pen(Color.Black, 10))
            using (var g = Graphics.FromImage(bmp))
            {
                var p = new GraphicsPath();
                p.AddString(
                    "#" + version,
                    FontFamily.GenericSansSerif,
                    (int)FontStyle.Bold,
                    g.DpiY * 275 / 72,
                    new Point(3, 58),
                    new StringFormat());
                g.DrawPath(pen, p);
                g.FillPath(b, p);
                return bmp;
            }
        }
    }
}
