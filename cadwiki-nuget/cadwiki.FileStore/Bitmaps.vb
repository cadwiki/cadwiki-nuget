Imports System
Imports System.Drawing.Imaging
Imports System.Drawing
Imports System.Windows.Media.Imaging
Imports System.Windows.Media
Imports PixelFormat = System.Drawing.Imaging.PixelFormat

Public Class Bitmaps

    Public Shared Function CreateBitmapSourceFromGdiBitmapForAutoCADButtonIcon(ByVal bitmap As Bitmap) As BitmapSource
        If bitmap Is Nothing Then Throw New ArgumentNullException("bitmap")

        Dim rect = New Rectangle(0, 0, bitmap.Width, bitmap.Height)

        Dim bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb)

        Try
            Dim size = rect.Width * rect.Height * 4

            Return BitmapSource.Create(bitmap.Width, bitmap.Height, bitmap.HorizontalResolution, bitmap.VerticalResolution, PixelFormats.Bgra32, Nothing, bitmapData.Scan0, size, bitmapData.Stride)
        Finally
            bitmap.UnlockBits(bitmapData)
        End Try
    End Function

End Class