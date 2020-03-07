using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechClient.Audio
{
    public class RecognitionResult
    {
        public string Intent { get; set; }

        public string Text { get; set; }

        public bool IsRecognizing { get; set; }
        public IEnumerable<RecognizedEntity> Entities { get; internal set; }
        public IEnumerable<TextPart> TextParts { get; internal set; }
    }
}
