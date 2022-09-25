

Imports System.Drawing
Imports System.Windows.Media.Imaging
Imports cadwiki
Imports cadwiki.NetUtils
Imports cadwiki.NUnitTestRunner


<TestClass()> Public Class TestFileStore

    <TestMethod()> Public Sub Test_Get500x500_cadwikiv1_ShouldReturnImage()

        Dim bitMap As Bitmap = FileStore.My.Resources.ResourceIcons._500x500_cadwiki_v1
        Dim bitMapImage As BitmapImage = Bitmaps.BitMapToBitmapImage(bitMap)

        Assert.IsNotNull(bitMap)
        Assert.IsNotNull(bitMapImage)
    End Sub




    <TestMethod()> Public Sub Test_Get500x500_cadwikiv1_ShouldReturnIcon()

        Dim bitMap As Bitmap = FileStore.My.Resources.ResourceIcons._500x500_cadwiki_v1
        Dim icon As Icon = Bitmaps.BitmapToIcon(bitMap, True, Color.White)

        Assert.IsNotNull(bitMap)
        Assert.IsNotNull(icon)
    End Sub
End Class