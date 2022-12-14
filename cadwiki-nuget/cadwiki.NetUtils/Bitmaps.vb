
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Windows.Media.Imaging

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
End Class
