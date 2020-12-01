using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Olive.SMS
{
    public class NullSmsDispatcher : ISmsDispatcher
    {
    
        public Task Dispatch(ISmsMessage sms) => Task.CompletedTask;
    }
}
