
Imports System.Drawing
Imports cadwiki.NUnitTestRunner.Results
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf

Namespace Creators
    Public Class PdfCreator
        Public PdfFilePath As String
        Public PdfDoc As PdfDocument

        Private Shared _bigFont As XFont = New XFont("Arial", 20, XFontStyle.Regular)
        Public bigFontLineSpacing As Integer = _bigFont.Height / 2.0
        Public smallFont As XFont = New XFont("Arial", 8, XFontStyle.Regular)
        Public smallFontLineSpacing As Integer = smallFont.Height / 2.0
        Private Shared _rightEdgeMargin As Integer = 20

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

            Dim area As XRect = New XRect(0, 400, GetMaxPageWidth(page), page.Height)
            Dim startPoint As XPoint = New XPoint(GetMaxPageWidth(page) / 2.0, 100)
            Dim strList As List(Of String) = suiteResult.ToStringList()
            DrawStringListInsideArea(gfx,
                                    _bigFont,
                                    bigFontLineSpacing,
                                    area,
                                    startPoint,
                                    strList,
                                    XBrushes.Black,
                                    XStringFormats.TopCenter)


        End Sub


        Public Sub AddTestPage(testResult As TestResult)
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

            Dim area As XRect = New XRect(10, 200, GetMaxPageWidth(page), page.Height)
            Dim strList As List(Of String) = testResult.ToStringList()
            Dim startPoint As XPoint = area.TopLeft
            DrawStringListInsideArea(gfx,
                                    smallFont,
                                    smallFontLineSpacing,
                                    area,
                                    startPoint,
                                    strList,
                                    XBrushes.Black,
                                    XStringFormats.TopLeft)

        End Sub

        Public Function DrawStringListInsideArea(ByVal gfx As XGraphics,
                                                       ByVal font As XFont,
                                                       ByVal lineSpacing As Double,
                                                       ByVal rect As XRect,
                                                       ByVal startPoint As XPoint,
                                                       ByVal strList As List(Of String),
                                                       ByVal brush As XBrush,
                                                       ByVal format As XStringFormat
                                                     ) As List(Of String)

            Dim point As XPoint = startPoint
            Dim strings As New List(Of String)
            For Each input As String In strList
                Dim currentStrings As List(Of String) = SplitStringIntoLines(gfx, font, rect, input)
                strings.AddRange(currentStrings)
            Next
            For Each str As String In strings
                gfx.DrawString(str, font, brush, point, format)
                point.Y = point.Y + font.Height + lineSpacing
            Next
            point.Y = point.Y + font.Height + lineSpacing
            Return strings
        End Function

        Private Shared Function SplitStringIntoLines(gfx As XGraphics,
                                                font As XFont,
                                                rect As XRect,
                                                input As String) As List(Of String)
            Dim stringMeasurement As XSize
            Dim i As Integer = 0
            Dim strings As New List(Of String)
            Dim currentLine As String = ""
            Dim charactersUpToNextSpace As String = ""
            Dim tryParseLine As Boolean
            Dim endLoop As Boolean

            While (i < input.Length And endLoop = False)
                Dim nextSpaceIndex As Integer = input.IndexOf(" ", i)

                If (nextSpaceIndex = -1) Then
                    nextSpaceIndex = input.Length - 1
                    tryParseLine = True
                    endLoop = True
                ElseIf (nextSpaceIndex = i) Then
                    i = i + 1
                    tryParseLine = False
                ElseIf (nextSpaceIndex = 0) Then
                    nextSpaceIndex = input.Length
                    i = input.Length
                    tryParseLine = True
                ElseIf (nextSpaceIndex > 0) Then
                    nextSpaceIndex = nextSpaceIndex
                    tryParseLine = True
                End If

                If (tryParseLine) Then
                    charactersUpToNextSpace = input.Substring(i, nextSpaceIndex + 1 - i)
                    stringMeasurement = gfx.MeasureString(currentLine + charactersUpToNextSpace, font)
                    If (stringMeasurement.Width > rect.Width) Then
                        'previously parsed current line is too large
                        If (gfx.MeasureString(currentLine, font).Width > rect.Width) Then
                            Dim j As Integer = nextSpaceIndex
                            While (j > i And gfx.MeasureString(currentLine, font).Width > rect.Width)
                                currentLine = input.Substring(i, j)
                                j = j - 1
                            End While
                            If (j = i) Then
                                currentLine = "..."
                            Else
                                currentLine = currentLine + "..."
                            End If
                        Else
                            'current line will fit, nothing to do
                        End If
                        strings.Add(currentLine)
                        currentLine = ""
                    End If
                    currentLine = currentLine + charactersUpToNextSpace
                    i = nextSpaceIndex
                End If
            End While
            If (Not String.IsNullOrEmpty(currentLine)) Then
                strings.Add(currentLine)
            End If
            Return strings
        End Function

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
            If width > GetMaxPageWidth(page) Then
                'Resize the image to make it to fit to the page width
                Dim widthFitRate As Single = width / CType(GetMaxPageWidth(page), Single)
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
                       smallFont,
                       XBrushes.Black,
                       New XRect(0, imageCaptionYLocation, GetMaxPageWidth(page), page.Height),
                       XStringFormats.TopCenter)
        End Sub


        Private Sub DrawImage(ByVal gfx As XGraphics, ByVal jpegSamplePath As String,
                          ByVal x As Integer, ByVal y As Integer,
                          ByVal width As Integer, ByVal height As Integer)
            Dim image As XImage = XImage.FromFile(jpegSamplePath)
            gfx.DrawImage(image, x, y, width, height)
        End Sub

        Public Function GetMaxPageWidth(page As PdfPage) As Integer
            Dim widthMinusMargin As Integer = page.Width.Point - _rightEdgeMargin
            Return widthMinusMargin
        End Function

    End Class

End Namespace
