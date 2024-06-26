using cadwiki.NUnitTestRunner.Creators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace UnitTests
{

    [TestClass()]
    public class TestsForPdfCreator
    {



        [TestMethod()]
        public void Test_DrawStringListInsideArea_WithLoremIpsum_ShouldReturn11Strings()
        {

            string filePath = cadwiki.NetUtils.Paths.GetTempFile("Lorem Ipsum.pdf");
            var pdfCreator = new PdfCreator(filePath);
            var testResult = new cadwiki.NUnitTestRunner.Results.TestResult();
            testResult.StackTrace.Add("test Lorem Ipsum text in stack trace to ensure pdf creator splits strings on spaces as necessary");
            testResult.StackTrace.Add("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Duo Reges: constructio interrete. " + "In eo autem voluptas omnium Latine loquentium more ponitur, cum percipitur ea, quae sensum aliquem moveat, " + "iucunditas. Hoc etsi multimodis reprehendi potest, tamen accipio, quod dant. De vacuitate doloris eadem sententia erit. " + "Quid sequatur, quid repugnet, vident. At iam decimum annum in spelunca iacet. Qualem igitur hominem natura inchoavit? " + "Nam aliquando posse recte fieri dicunt nulla expectata nec quaesita voluptate.");
            pdfCreator.AddTestPage(testResult);
            pdfCreator.Save();

            var pdfDoc = new PdfDocument();
            var page = pdfDoc.Pages.Add();
            var gfx = XGraphics.FromPdfPage(page);

            var area = new XRect(10d, 200d, pdfCreator.GetMaxPageWidth(page), page.Height);
            var strList = testResult.ToStringList();
            var startPoint = area.TopLeft;
            var stringList = pdfCreator.DrawStringListInsideArea(gfx, pdfCreator.smallFont, pdfCreator.smallFontLineSpacing, area, startPoint, strList, XBrushes.Black, XStringFormats.TopLeft);

            int expected = 11;
            Assert.AreEqual(expected, stringList.Count);
        }

    }
}