using System;
using System.Collections.Generic;
using System.Text;

namespace SpeechClient.Audio
{
    public class RecognizedIntent
    {
        public string Intent { get; set; }

        public float Score { get; set; }
    }
}
