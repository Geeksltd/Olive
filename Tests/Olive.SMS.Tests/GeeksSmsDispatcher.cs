using System.Threading.Tasks;

namespace Olive.SMS.Tests
{
    public class GeeksSmsDispatcher : ISmsDispatcher
    {
        public Task Dispatch(ISmsMessage sms)
        {
            //send sms
        }
    }
}