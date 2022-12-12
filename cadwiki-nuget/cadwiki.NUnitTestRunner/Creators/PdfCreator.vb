
Imports System.Drawing
Imports cadwiki.NUnitTestRunner.Results
Imports PdfSharp.Drawing
Imports PdfSharp.Drawing.Layout
Imports PdfSharp.Pdf

Namespace Creators
    Public Class PdfCreator
        Public PdfFilePath As String
        Public PdfDoc As PdfDocument

        Private Shared _bigFont As XFont = New XFont("Arial", 20, XFontStyle.Regular)
        Private Shared _smallFont As XFont = New XFont("Arial", 8, XFontStyle.Regular)


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

            ' Draw the text
            gfx.DrawString("AutomatedTestEvidence.pdf",
                       _bigFont,
                       XBrushes.Black,
                       New XRect(0, 0, page.Width, page.Height),
                       XStringFormats.TopCenter)

            Dim str As String = "Summary:"
            gfx.DrawString(str,
               _bigFont,
               XBrushes.Black,
               New XRect(0, 200, page.Width, page.Height),
               XStringFormats.TopCenter)

            str = "Elapsed time: " + suiteResult.TimeElapsed.ToString()
            gfx.DrawString(str,
               _bigFont,
               XBrushes.Black,
               New XRect(0, 250, page.Width, page.Height),
               XStringFormats.TopCenter)

            str = "Total tests: " + suiteResult.TotalTests.ToString()
            gfx.DrawString(str,
               _bigFont,
               XBrushes.Black,
               New XRect(0, 300, page.Width, page.Height),
               XStringFormats.TopCenter)

            str = "Passed tests: " + suiteResult.PassedTests.ToString()
            gfx.DrawString(str,
               _bigFont,
               XBrushes.Black,
               New XRect(0, 350, page.Width, page.Height),
               XStringFormats.TopCenter)

            str = "Failed tests: " + suiteResult.FailedTests.ToString()
            gfx.DrawString(str,
               _bigFont,
               XBrushes.Black,
               New XRect(0, 400, page.Width, page.Height),
               XStringFormats.TopCenter)

        End Sub


        Friend Sub AddTestPage(testResult As TestResult)
            AddTestTitlePage(testResult)
            If testResult.Evidence IsNot Nothing Then
                For Each image As TestEvidence.Image In testResult.Evidence.Images
                    AddImageAsNewPage(image.FilePath)
                Next
            End If
        End Sub


        Public Sub AddTestTitlePage(testResult As TestResult)
            Dim page As PdfPage = PdfDoc.Pages.Add()


            ' Get an XGraphics object for drawing
            Dim gfx As XGraphics = XGraphics.FromPdfPage(page)

            Dim area As XRect = New XRect(0, 200, page.Width, page.Height)
            Dim strList As List(Of String) = testResult.ToStringList()
            'inputString.Replace(Environment.Indent, Environment.NewLine)

            GetFittedStringToDraw(gfx,
                                                      _smallFont,
                                                      _smallFont.Height / 2.0,
                                                        area,
                                                      strList,
                                                      XBrushes.Black)

        End Sub

        Private Shared Sub GetFittedStringToDraw(ByVal gfx As XGraphics,
                                                       ByVal font As XFont,
                                                       ByVal lineSpacing As Double,
                                                       ByVal rect As XRect,
                                                       ByVal strList As List(Of String),
                                                       ByVal brush As XBrush
                                                     )
            Dim stringMeasurement As XSize
            Dim point As XPoint = rect.TopLeft
            Dim i As Integer = 0
            Dim character As String = ""
            Dim currentLine As String = ""
            For Each input As String In strList
                currentLine = ""
                i = 0
                While (i < input.Length)
                    character = input(i)
                    stringMeasurement = gfx.MeasureString(currentLine + character, font)
                    If (stringMeasurement.Width > rect.Width) Then
                        gfx.DrawString(currentLine, font, brush, point)
                        point.Y = point.Y + font.Height + lineSpacing
                        currentLine = ""
                    End If
                    currentLine = currentLine + character
                    i = i + 1
                End While
                If Not String.IsNullOrEmpty(currentLine) Then
                    gfx.DrawString(currentLine, font, brush, point)
                    point.Y = point.Y + font.Height + lineSpacing
                    currentLine = ""
                End If
                point.Y = point.Y + font.Height + lineSpacing
                currentLine = ""
            Next
        End Sub

        Public Sub AddImageAsNewPage(imageFilePath As String)
            'Add a page
            Dim page As PdfPage = PdfDoc.Pages.Add()

            ' Get an XGraphics object for drawing
            Dim gfx As XGraphics = XGraphics.FromPdfPage(page)
            Dim imageStartYLocation As Double = 150
            Dim imageCaptionBuffer As Double = 10
            'Load an image
            Dim image As Image = Image.FromFile(imageFilePath)
            'Get the image width and height
            Dim width As Single = image.PhysicalDimension.Width
            Dim height As Single = image.PhysicalDimension.Height
            Dim imageCaptionYLocation As Single = imageStartYLocation + height + imageCaptionBuffer
            'Declare a PdfImage variable
            ' Get an XGraphics object for drawing
            If width > page.Width Then
                'Resize the image to make it to fit to the page width
                Dim widthFitRate As Single = width / CType(page.Width, Single)
                Dim newWidth As Integer = CInt(width / widthFitRate)
                Dim newHeight As Integer = CInt(height / widthFitRate)
                Dim size As New Size(newWidth, newHeight)
                Dim scaledImage As Bitmap = New Bitmap(image, size)
                Dim ext As String = System.IO.Path.GetExtension(imageFilePath)
                imageFilePath = imageFilePath.Replace(ext, "-(scaled)" + ext)
                imageFilePath = NetUtils.Paths.GetUniqueFilePath(imageFilePath)
                scaledImage.Save(imageFilePath, System.Drawing.Imaging.ImageFormat.Jpeg)
                DrawImage(gfx, imageFilePath, 0, imageStartYLocation, newWidth, newHeight)
                imageCaptionYLocation = imageStartYLocation + newHeight + imageCaptionBuffer
            Else
                DrawImage(gfx, imageFilePath, 0, imageStartYLocation, width, height)
            End If

            ' Draw the text for the image caption below the image by 10 units
            gfx.DrawString(imageFilePath,
                       _smallFont,
                       XBrushes.Black,
                       New XRect(0, imageCaptionYLocation, page.Width, page.Height),
                       XStringFormats.TopCenter)
        End Sub


        Private Sub DrawImage(ByVal gfx As XGraphics, ByVal jpegSamplePath As String,
                          ByVal x As Integer, ByVal y As Integer,
                          ByVal width As Integer, ByVal height As Integer)
            Dim image As XImage = XImage.FromFile(jpegSamplePath)
            gfx.DrawImage(image, x, y, width, height)
        End Sub


    End Class

End Namespace
