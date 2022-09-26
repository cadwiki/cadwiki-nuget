
Imports System.Drawing

Imports Spire.Pdf
Imports Spire.Pdf.Graphics


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
        PdfDoc.SaveToFile(PdfFilePath)
    End Sub


    Public Sub AddImageAsNewPage(imageFilePath As String)
        'Set the margins
        PdfDoc.PageSettings.SetMargins(20)
        'Add a page
        Dim page As PdfPageBase = PdfDoc.Pages.Add()
        'Load an image
        Dim image As Image = Image.FromFile(imageFilePath)
        'Get the image width and height
        Dim width As Single = image.PhysicalDimension.Width
        Dim height As Single = image.PhysicalDimension.Height
        'Declare a PdfImage variable
        Dim pdfImage As PdfImage
        'If the image width is larger than page width
        If width > page.Canvas.ClientSize.Width Then
            'Resize the image to make it to fit to the page width
            Dim widthFitRate As Single = width / page.Canvas.ClientSize.Width
            Dim num1 As Integer = CInt(width / widthFitRate)
            Dim num2 As Integer = CInt(height / widthFitRate)
            Dim size As New Size(num1, num2)
            Dim scaledImage As Bitmap = New Bitmap(image, size)
            'Load the scaled image to the PdfImage object
            pdfImage = PdfImage.FromImage(scaledImage)
        Else
            'Load the original image to the PdfImage object
            pdfImage = PdfImage.FromImage(image)

        End If
        'Draw image at (0, 0)
        page.Canvas.DrawImage(pdfImage, 0, 0, pdfImage.Width, pdfImage.Height)
        PdfDoc.SaveToFile(PdfFilePath)
    End Sub

End Class
