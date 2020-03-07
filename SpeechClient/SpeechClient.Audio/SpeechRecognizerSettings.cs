using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechClient.Audio
{
    public class SpeechRecognizerSettings
    {
        public string SubscriptionKey { get; set; }

        public string Region { get; set; }

        public string LuisAppId { get; set; }
    }
}
