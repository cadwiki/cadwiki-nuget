using Autodesk.AutoCAD.DatabaseServices;

namespace cadwiki.AC
{

    public class Lines
    {
        public static Line TruncateEndpointsToTwoDecimalPlaces(Line line)
        {
            var truncatedStart = Points.ToTwoDecimalPlaces(line.StartPoint);
            var truncatedEnd = Points.ToTwoDecimalPlaces(line.EndPoint);
            var truncatedLine = new Line(truncatedStart, truncatedEnd);
            return truncatedLine;
        }
    }
}