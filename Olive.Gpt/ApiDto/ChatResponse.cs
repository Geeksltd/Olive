using System.Linq;
using Olive;

namespace Olive.Gpt.ApiDto
{
    class ChatResponse
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public long Created { get; set; }
        public Choice[] Choices { get; set; }

        public override string ToString() => Choices.FirstOrDefault()?.Delta?.Content;
    }
}