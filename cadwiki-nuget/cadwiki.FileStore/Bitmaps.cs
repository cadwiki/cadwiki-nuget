using System;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace cadwiki.FileStore
{

    public class Bitmaps
    {

        public static BitmapSource CreateBitmapSourceFromGdiBitmapForAutoCADButtonIcon(Bitmap bitmap)
        {
            if (bitmap is null)
            {
                return CreateDefaultAcadBitmap();
            }

            BitmapData bitmapData = null;
            try
            {
                var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                int size = rect.Width * rect.Height * 4;
                return BitmapSource.Create(
                    bitmap.Width, 
                    bitmap.Height,
                    (double)bitmap.HorizontalResolution,
                    (double)bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride
                    );
            }
            catch (Exception)
            {
                return CreateDefaultAcadBitmap();
            }
            finally
            {
                if (bitmapData != null)
                {
                    bitmap.UnlockBits(bitmapData);
                }
            }
        }

        private static BitmapSource CreateDefaultAcadBitmap()
        {
            BitmapData bitmapData = null;
            Bitmap defaultBitmap = new Bitmap(100, 100);
            using (Graphics graphics = Graphics.FromImage(defaultBitmap))
            {
                graphics.Clear(System.Drawing.Color.White);
                graphics.DrawLine(Pens.Black, 0, 0, 100, 100);
                graphics.DrawLine(Pens.Black, 100, 0, 0, 100);
            }
            var rect = new Rectangle(0, 0, defaultBitmap.Width, defaultBitmap.Height);
            bitmapData = defaultBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int size = rect.Width * rect.Height * 4;

            return BitmapSource.Create(
                defaultBitmap.Width, 
                defaultBitmap.Height,
                (double)defaultBitmap.HorizontalResolution,
                (double)defaultBitmap.VerticalResolution,
                PixelFormats.Bgra32,
                null,
                bitmapData.Scan0,
                size,
                bitmapData.Stride
                );
        }
    }
}