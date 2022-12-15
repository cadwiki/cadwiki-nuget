Imports cadwiki.NUnitTestRunner
Imports cadwiki.NUnitTestRunner.Creators
Imports cadwiki.NUnitTestRunner.TestEvidence
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf

<TestClass()> Public Class TestsForPdfCreator



    <TestMethod()> Public Sub Test_DrawStringListInsideArea_WithLoremIpsum_ShouldReturn11Strings()

        Dim filePath As String = cadwiki.NetUtils.Paths.GetTempFile("Lorem Ipsum.pdf")
        Dim pdfCreator As PdfCreator = New PdfCreator(filePath)
        Dim testResult As cadwiki.NUnitTestRunner.Results.TestResult = New cadwiki.NUnitTestRunner.Results.TestResult()
        testResult.StackTrace.Add("test Lorem Ipsum text in stack trace to ensure pdf creator splits strings on spaces as necessary")
        testResult.StackTrace.Add("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duo Reges: constructio interrete. " +
                                    "In eo autem voluptas omnium Latine loquentium more ponitur, cum percipitur ea, quae sensum aliquem moveat, " +
                                    "iucunditas. Hoc etsi multimodis reprehendi potest, tamen accipio, quod dant. De vacuitate doloris eadem sententia erit. " +
                                    "Quid sequatur, quid repugnet, vident. At iam decimum annum in spelunca iacet. Qualem igitur hominem natura inchoavit? " +
                                    "Nam aliquando posse recte fieri dicunt nulla expectata nec quaesita voluptate.")
        pdfCreator.AddTestPage(testResult)
        pdfCreator.Save()

        Dim pdfDoc As PdfDocument = New PdfDocument()
        Dim page As PdfPage = pdfDoc.Pages.Add()
        Dim gfx As XGraphics = XGraphics.FromPdfPage(page)

        Dim area As XRect = New XRect(10, 200, pdfCreator.GetMaxPageWidth(page), page.Height)
        Dim strList As List(Of String) = testResult.ToStringList()
        Dim startPoint As XPoint = area.TopLeft
        Dim stringList As List(Of String) = pdfCreator.DrawStringListInsideArea(gfx,
                                    pdfCreator.smallFont,
                                    pdfCreator.smallFontLineSpacing,
                                    area,
                                    startPoint,
                                    strList,
                                    XBrushes.Black,
                                    XStringFormats.TopLeft)

        Dim expected As Integer = 11
        Assert.AreEqual(expected, stringList.Count)
    End Sub

End Class