using System;

namespace cadwiki.AutoCAD2021.Base.Utilities
{

    public class MathUtils
    {
        public static double RadiansToDegree(double radians)
        {
            double degrees = 180d * (radians / Math.PI);
            return degrees;
        }

        public static double DegreesToRadians(double degrees)
        {
            double radians = degrees * (Math.PI / 180d);
            return radians;
        }


        public static bool AreDoublesEqual(double num1, double num2, double fuzz)
        {

            double dRet = num1 - num2;
            bool @bool;

            if (Math.Abs(dRet) < fuzz)
            {
                @bool = true;
            }
            else
            {
                @bool = false;
            }

            return @bool;
        }
    }
}