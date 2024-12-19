using OpenAI.Audio;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

namespace Olive.OpenAI.Voice
{
    public class OpenAI
    {
        AudioClient _client = null;
        public OpenAI()
        {
            var model = Olive.Config.Get("OpenAI:Voice:Model").Or("whisper-1");
            var key = Olive.Config.Get("OpenAI:Voice:APIKey");

            if (key.IsEmpty())
            {
                throw new ArgumentException("The api key was not provided. Please set it in the AppSettings or webConfig in the OpenAI:Voice:APIKey entry");
            }
            _client = new AudioClient(model, key);
        }
        public async Task<string> ToText(byte[] audio)
        {
            using MemoryStream stream = new MemoryStream(audio);
            AudioTranscription transcription = await _client.TranscribeAudioAsync(stream, "speech.mp3", new AudioTranscriptionOptions { Language="en"});
            return transcription.Text;
        }
    }
}
