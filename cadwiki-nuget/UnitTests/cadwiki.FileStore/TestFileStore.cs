

using System.Drawing;
using System.Windows.Media.Imaging;
using cadwiki.NetUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using cadwiki.FileStore;
using Bitmaps = cadwiki.NetUtils.Bitmaps;

namespace UnitTests
{


    [TestClass()]
    public class TestFileStore
    {

        [TestMethod()]
        public void Test_Get500x500_cadwikiv1_ShouldReturnImage()
        {

            Bitmap bitMap = ResourceIcons._500x500_cadwiki_v1;
            BitmapImage bitMapImage = Bitmaps.BitMapToBitmapImage(bitMap);

            Assert.IsNotNull(bitMap);
            Assert.IsNotNull(bitMapImage);
        }


        [TestMethod()]
        public void Test_Get500x500_cadwikiv1_ShouldReturnIcon()
        {

            Bitmap bitMap = ResourceIcons._500x500_cadwiki_v1;
            Icon icon = Bitmaps.BitmapToIcon(bitMap, true, Color.White);

            Assert.IsNotNull(bitMap);
            Assert.IsNotNull(icon);
        }
    }
}