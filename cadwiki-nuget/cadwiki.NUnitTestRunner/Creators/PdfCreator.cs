using System;
using System.Collections.Generic;
using System.Drawing;
using cadwiki.NUnitTestRunner.Results;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace cadwiki.NUnitTestRunner.Creators
{
    public class PdfCreator
    {
        public string PdfFilePath;
        public PdfDocument PdfDoc;

        private static XFont _bigFont = new XFont("Arial", 20d, XFontStyle.Regular);
        public int bigFontLineSpacing = (int)Math.Round(_bigFont.Height / 2.0d);
        public XFont smallFont = new XFont("Arial", 8d, XFontStyle.Regular);
        public int smallFontLineSpacing;
        private static int _rightEdgeMargin = 20;

        public PdfCreator(string filePath)
        {
            smallFontLineSpacing = smallFont.Height / 2;
            PdfFilePath = filePath;
            // Create a PdfDocument object
            PdfDoc = new PdfDocument();
        }

        public void Save()
        {
            // Save to file
            if (PdfDoc.PageCount > 0)
            {
                PdfDoc.Save(PdfFilePath);
            }

        }

        public void AddTitlePage(ObservableTestSuiteResults suiteResult)
        {
            var page = PdfDoc.Pages.Add();

            // Get an XGraphics object for drawing
            var gfx = XGraphics.FromPdfPage(page);

            var area = new XRect(0d, 400d, GetMaxPageWidth(page), page.Height);
            var startPoint = new XPoint(GetMaxPageWidth(page) / 2.0d, 100d);
            var strList = suiteResult.ToStringList();
            DrawStringListInsideArea(gfx, _bigFont, bigFontLineSpacing, area, startPoint, strList, XBrushes.Black, XStringFormats.TopCenter);


        }


        public void AddTestPage(TestResult testResult)
        {
            AddTestTitlePage(testResult);
            if (testResult.Evidence is not null)
            {
                foreach (TestEvidence.Image image in testResult.Evidence.Images)
                    AddImageAsNewPage(image.FilePath);
            }
        }


        public void AddTestTitlePage(TestResult testResult)
        {
            var page = PdfDoc.Pages.Add();


            // Get an XGraphics object for drawing
            var gfx = XGraphics.FromPdfPage(page);

            var area = new XRect(10d, 200d, GetMaxPageWidth(page), page.Height);
            var strList = testResult.ToStringList();
            var startPoint = area.TopLeft;
            DrawStringListInsideArea(gfx, smallFont, smallFontLineSpacing, area, startPoint, strList, XBrushes.Black, XStringFormats.TopLeft);

        }

        public List<string> DrawStringListInsideArea(XGraphics gfx, XFont font, double lineSpacing, XRect rect, XPoint startPoint, List<string> strList, XBrush brush, XStringFormat format)
        {

            var point = startPoint;
            var strings = new List<string>();
            foreach (string input in strList)
            {
                var currentStrings = SplitStringIntoLines(gfx, font, rect, input);
                strings.AddRange(currentStrings);
            }
            foreach (string str in strings)
            {
                gfx.DrawString(str, font, brush, point, format);
                point.Y = point.Y + font.Height + lineSpacing;
            }
            point.Y = point.Y + font.Height + lineSpacing;
            return strings;
        }

        private static List<string> SplitStringIntoLines(XGraphics gfx, XFont font, XRect rect, string input)
        {
            XSize stringMeasurement;
            int i = 0;
            var strings = new List<string>();
            string currentLine = "";
            string charactersUpToNextSpace = "";
            var tryParseLine = default(bool);
            var endLoop = default(bool);

            while (i < input.Length & endLoop == false)
            {
                int nextSpaceIndex = input.IndexOf(" ", i);

                if (nextSpaceIndex == -1)
                {
                    nextSpaceIndex = input.Length - 1;
                    tryParseLine = true;
                    endLoop = true;
                }
                else if (nextSpaceIndex == i)
                {
                    i = i + 1;
                    tryParseLine = false;
                }
                else if (nextSpaceIndex == 0)
                {
                    nextSpaceIndex = input.Length;
                    i = input.Length;
                    tryParseLine = true;
                }
                else if (nextSpaceIndex > 0)
                {
                    nextSpaceIndex = nextSpaceIndex;
                    tryParseLine = true;
                }

                if (tryParseLine)
                {
                    charactersUpToNextSpace = input.Substring(i, nextSpaceIndex + 1 - i);
                    stringMeasurement = gfx.MeasureString(currentLine + charactersUpToNextSpace, font);
                    if (stringMeasurement.Width > rect.Width)
                    {
                        // previously parsed current line is too large
                        if (gfx.MeasureString(currentLine, font).Width > rect.Width)
                        {
                            int j = nextSpaceIndex;
                            while (j > i & gfx.MeasureString(currentLine, font).Width > rect.Width)
                            {
                                currentLine = input.Substring(i, j);
                                j = j - 1;
                            }
                            if (j == i)
                            {
                                currentLine = "...";
                            }
                            else
                            {
                                currentLine = currentLine + "...";
                            }
                        }
                        else
                        {
                            // current line will fit, nothing to do
                        }
                        strings.Add(currentLine);
                        currentLine = "";
                    }
                    currentLine = currentLine + charactersUpToNextSpace;
                    i = nextSpaceIndex;
                }
            }
            if (!string.IsNullOrEmpty(currentLine))
            {
                strings.Add(currentLine);
            }
            return strings;
        }

        public void AddImageAsNewPage(string imageFilePath)
        {
            // Add a page
            var page = PdfDoc.Pages.Add();

            // Get an XGraphics object for drawing
            var gfx = XGraphics.FromPdfPage(page);
            double imageStartYLocation = 150d;
            double imageCaptionBuffer = 10d;
            // Load an image
            var image = Image.FromFile(imageFilePath);
            // Get the image width and height
            float width = image.PhysicalDimension.Width;
            float height = image.PhysicalDimension.Height;
            float imageCaptionYLocation = (float)(imageStartYLocation + (double)height + imageCaptionBuffer);
            // Declare a PdfImage variable
            // Get an XGraphics object for drawing
            if (width > GetMaxPageWidth(page))
            {
                // Resize the image to make it to fit to the page width
                float widthFitRate = width / GetMaxPageWidth(page);
                int newWidth = (int)Math.Round(width / widthFitRate);
                int newHeight = (int)Math.Round(height / widthFitRate);
                var size = new Size(newWidth, newHeight);
                var scaledImage = new Bitmap(image, size);
                string ext = System.IO.Path.GetExtension(imageFilePath);
                imageFilePath = imageFilePath.Replace(ext, "-(scaled)" + ext);
                imageFilePath = NetUtils.Paths.GetUniqueFilePath(imageFilePath);
                scaledImage.Save(imageFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                DrawImage(gfx, imageFilePath, 0, (int)Math.Round(imageStartYLocation), newWidth, newHeight);
                imageCaptionYLocation = (float)(imageStartYLocation + newHeight + imageCaptionBuffer);
            }
            else
            {
                DrawImage(gfx, imageFilePath, 0, (int)Math.Round(imageStartYLocation), (int)Math.Round(width), (int)Math.Round(height));
            }

            // Draw the text for the image caption below the image by 10 units
            gfx.DrawString(imageFilePath, smallFont, XBrushes.Black, new XRect(0d, (double)imageCaptionYLocation, GetMaxPageWidth(page), page.Height), XStringFormats.TopCenter);
        }


        private void DrawImage(XGraphics gfx, string jpegSamplePath, int x, int y, int width, int height)
        {
            var image = XImage.FromFile(jpegSamplePath);
            gfx.DrawImage(image, x, y, width, height);
        }

        public int GetMaxPageWidth(PdfPage page)
        {
            int widthMinusMargin = (int)Math.Round(page.Width.Point - _rightEdgeMargin);
            return widthMinusMargin;
        }

    }

}