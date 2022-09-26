Imports System.Drawing.Imaging

Namespace TestEvidence
    Public Class Evidence
        Public Images As New List(Of Image)

        Public Sub TakeJpegScreenshot(windowIntPtr As IntPtr, title As String)
            Dim creator As New TestEvidenceCreator()
            Dim fileName As String = title + ".jpg"
            fileName = NetUtils.Paths.ReplaceAllillegalCharsForWindowsOSInFileName(fileName, "-")
            Dim screenshotPath As String = creator.GetScreenshotCache() + "\" + fileName
            screenshotPath = NetUtils.Paths.GetUniqueFilePath(screenshotPath)
            Dim format As ImageFormat = ImageFormat.Jpeg
            TestEvidenceCreator.PrintWindowToImage(windowIntPtr, screenshotPath, format)
            Dim image As New Image()
            image.Title = title
            image.FilePath = screenshotPath
            Me.Images.Add(image)
            creator.SetEvidenceForCurrentTest(Me)
        End Sub
    End Class
End Namespace

