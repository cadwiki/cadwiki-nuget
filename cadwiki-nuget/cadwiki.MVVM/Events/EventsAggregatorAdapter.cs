using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cadwiki.MVVM.Events
{
    public class EventsAggregatorAdapter : EventAggregator
    {
        public void PublishOnUiThread(object message)
        {
            Caliburn.Micro.EventAggregatorExtensions.PublishOnUIThreadAsync(this, message);
        }
    }
}
