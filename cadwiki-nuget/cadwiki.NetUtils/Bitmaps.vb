
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports Color = System.Drawing.Color
Imports PixelFormat = System.Drawing.Imaging.PixelFormat


Public Class Bitmaps

    Public Shared Function BitMapToBitmapImage(ByVal bitmap As Bitmap) As BitmapImage
        Using memory As MemoryStream = New MemoryStream()
            bitmap.Save(memory, ImageFormat.Png)
            memory.Position = 0
            Dim bitmapImage As BitmapImage = New BitmapImage()
            bitmapImage.BeginInit()
            bitmapImage.StreamSource = memory
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad
            bitmapImage.EndInit()
            bitmapImage.Freeze()
            Return bitmapImage
        End Using
    End Function

    Public Shared Function BitmapToIcon(bitMap As Bitmap,
                                        makeTransparent As Boolean,
                                        colorToMakeTransparent As Color) As Icon
        If makeTransparent Then
            bitMap.MakeTransparent(colorToMakeTransparent)
        End If
        Dim iconHandle As IntPtr = bitMap.GetHicon()
        Return Icon.FromHandle(iconHandle)
    End Function

    Public Shared Function CreateBitmapSourceFromBitmap(ByVal bitmap As Bitmap) As BitmapSource
        Dim rect As Rectangle = New Rectangle(0, 0, bitmap.Width, bitmap.Height)

        Dim bitmapData As BitmapData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)

        Try
            Dim size As Integer = rect.Width * rect.Height * 4

            Return BitmapSource.Create(bitmap.Width,
                                       bitmap.Height, bitmap.HorizontalResolution,
                                       bitmap.VerticalResolution,
                                       PixelFormats.Bgra32,
                                       Nothing,
                                       bitmapData.Scan0,
                                       size,
                                       bitmapData.Stride)
        Finally
            bitmap.UnlockBits(bitmapData)
        End Try
    End Function
End Class
