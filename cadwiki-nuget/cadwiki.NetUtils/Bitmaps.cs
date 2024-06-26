
using System.Drawing;
using Color = System.Drawing.Color;
using System.Drawing.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace cadwiki.NetUtils
{


    public class Bitmaps
    {

        public static BitmapImage BitMapToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0L;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        public static Icon BitmapToIcon(Bitmap bitMap, bool makeTransparent, Color colorToMakeTransparent)
        {
            if (makeTransparent)
            {
                bitMap.MakeTransparent(colorToMakeTransparent);
            }
            var iconHandle = bitMap.GetHicon();
            return Icon.FromHandle(iconHandle);
        }

        public static BitmapSource CreateBitmapSourceFromBitmap(Bitmap bitmap)
        {
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            try
            {
                int size = rect.Width * rect.Height * 4;

                return BitmapSource.Create(bitmap.Width, bitmap.Height, (double)bitmap.HorizontalResolution, (double)bitmap.VerticalResolution, PixelFormats.Bgra32, null, bitmapData.Scan0, size, bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }
    }
}