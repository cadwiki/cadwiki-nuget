
Imports System.Drawing
Imports cadwiki.NUnitTestRunner.Results
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf

Public Class PdfCreator
    Public PdfFilePath As String
    Public PdfDoc As PdfDocument

    Public Sub New(filePath As String)
        PdfFilePath = filePath
        'Create a PdfDocument object
        PdfDoc = New PdfDocument()
    End Sub

    Public Sub Save()
        'Save to file
        If PdfDoc.PageCount > 0 Then
            PdfDoc.Save(PdfFilePath)
        End If

    End Sub

    Public Sub AddTitlePage(suiteResult As ObservableTestSuiteResults)
        Dim page As PdfPage = PdfDoc.Pages.Add()

        ' Get an XGraphics object for drawing
        Dim gfx As XGraphics = XGraphics.FromPdfPage(page)
        ' Create a font
        Dim font As XFont = New XFont("Arial", 20, XFontStyle.BoldItalic)

        ' Draw the text
        gfx.DrawString("AutomatedTestEvidence.pdf",
                       font,
                       XBrushes.Black,
                       New XRect(0, 0, page.Width, page.Height),
                       XStringFormats.TopCenter)

        Dim str As String = "Summary:"
        gfx.DrawString(str,
               font,
               XBrushes.Black,
               New XRect(0, 200, page.Width, page.Height),
               XStringFormats.TopCenter)

        str = "Elapsed time: " + suiteResult.TimeElapsed.ToString()
        gfx.DrawString(str,
               font,
               XBrushes.Black,
               New XRect(0, 250, page.Width, page.Height),
               XStringFormats.TopCenter)

        str = "Total tests: " + suiteResult.TotalTests.ToString()
        gfx.DrawString(str,
               font,
               XBrushes.Black,
               New XRect(0, 300, page.Width, page.Height),
               XStringFormats.TopCenter)

        str = "Passed tests: " + suiteResult.PassedTests.ToString()
        gfx.DrawString(str,
               font,
               XBrushes.Black,
               New XRect(0, 350, page.Width, page.Height),
               XStringFormats.TopCenter)

        str = "Failed tests: " + suiteResult.FailedTests.ToString()
        gfx.DrawString(str,
               font,
               XBrushes.Black,
               New XRect(0, 400, page.Width, page.Height),
               XStringFormats.TopCenter)

    End Sub
    Public Sub AddImageAsNewPage(imageFilePath As String)
        'Add a page
        Dim page As PdfPage = PdfDoc.Pages.Add()
        'Load an image
        Dim image As Image = Image.FromFile(imageFilePath)
        'Get the image width and height
        Dim width As Single = image.PhysicalDimension.Width
        Dim height As Single = image.PhysicalDimension.Height
        'Declare a PdfImage variable
        ' Get an XGraphics object for drawing
        If width > page.Width Then
            'Resize the image to make it to fit to the page width
            Dim widthFitRate As Single = width / CType(page.Width, Single)
            Dim newWidth As Integer = CInt(width / widthFitRate)
            Dim newHeight As Integer = CInt(height / widthFitRate)
            Dim size As New Size(newWidth, newHeight)
            Dim scaledImage As Bitmap = New Bitmap(image, size)
            imageFilePath = imageFilePath + "-(scaled)"
            scaledImage.Save(imageFilePath, System.Drawing.Imaging.ImageFormat.Jpeg)
            Dim gfx As XGraphics = XGraphics.FromPdfPage(page)
            DrawImage(gfx, imageFilePath, 0, 50, newWidth, newHeight)
        Else
            Dim gfx As XGraphics = XGraphics.FromPdfPage(page)
            DrawImage(gfx, imageFilePath, 0, 50, width, height)
        End If

        PdfDoc.Save(PdfFilePath)
    End Sub


    Private Sub DrawImage(ByVal gfx As XGraphics, ByVal jpegSamplePath As String,
                          ByVal x As Integer, ByVal y As Integer,
                          ByVal width As Integer, ByVal height As Integer)
        Dim image As XImage = XImage.FromFile(jpegSamplePath)
        gfx.DrawImage(image, x, y, width, height)
    End Sub


End Class
