using cadwiki.MVVM.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cadwiki.MVVM
{
    public class Globals
    {
        public static EventsAggregatorAdapter Events = new EventsAggregatorAdapter();
    }
}
