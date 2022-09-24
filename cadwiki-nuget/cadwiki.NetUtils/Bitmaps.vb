
Imports System.Drawing
Imports System.Windows
Imports System.Windows.Interop
Imports System.Windows.Media.Imaging

Public Class Bitmaps
    <System.Runtime.InteropServices.DllImport("gdi32.dll")>
    Private Shared Function DeleteObject(ByVal hObject As IntPtr) As Boolean

    End Function


    Public Shared Function Bitmap2BitmapImage(ByVal bitmap As Bitmap) As BitmapImage
        Dim hBitmap As IntPtr = bitmap.GetHbitmap()
        Dim retval As BitmapImage

        Try
            retval = CType(Imaging.CreateBitmapSourceFromHBitmap(hBitmap,
                                                                 IntPtr.Zero,
                                                                 Int32Rect.Empty,
                                                                 BitmapSizeOptions.FromEmptyOptions()),
                                                                 BitmapImage)
        Finally
            DeleteObject(hBitmap)
        End Try

        Return retval
    End Function
End Class
