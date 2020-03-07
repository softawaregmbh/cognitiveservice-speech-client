using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechClient.Audio
{
    public class RecognizedEntity
    {
        public string Entity { get; set; }

        public string Type { get; set; }

        public int StartIndex { get; set; }

        public int EndIndex { get; set; }

        public float Score { get; set; }
    }
}
