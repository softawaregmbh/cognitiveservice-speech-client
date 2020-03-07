using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechClient.Audio
{
    public class TextPart
    {
        public int StartIndex { get; set; }

        public string Text { get; set; }

        public RecognizedEntity Entity { get; set; }
        public int EndIndex { get; internal set; }
    }
}
