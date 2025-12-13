using System;
using System.Collections.Generic;

namespace cadwiki.AC.Plotters
{
    public class PlotReturn
    {
        public string OutputPath = "";
        public List<Exception> Exceptions = new List<Exception>();
        public string ErrorMessage = "";
        public bool Error = true;
    }
}
