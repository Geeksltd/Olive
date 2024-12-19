namespace Olive.OpenAI
{
    public class OpenAI
    {
        /// <summary>
        /// Creates a new instance of OpenAI. If no key is passed it reads the OpenAI:Key from config.
        /// </summary>
        public OpenAI(string? key = null)
        {
            // Get the keys from Config
        }

        public async Task GetResponse(string[] messages,string instruction= null)
        {
            throw new NotImplementedException();
        }

        public async IAsyncEnumerable<string> GetResponseStream(string[] messages, string instruction = null)
        {
            throw new NotImplementedException();
        }

    }
}
