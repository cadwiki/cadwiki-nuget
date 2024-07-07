using cadwiki.WpfLibrary.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cadwiki.WpfLibrary
{
    public class Globals
    {
        private static ExceptionHandler _exceptionHandler;
        public static ExceptionHandler Ex
        {
            get
            {
                //create new instance if one does not exist already
                if (_exceptionHandler == null)
                {
                    _exceptionHandler = new ExceptionHandler();
                }
                return _exceptionHandler;
            }
            set
            {
                _exceptionHandler = value;
            }
        }
    }
}


