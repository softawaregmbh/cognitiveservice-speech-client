using softaware.ViewPort.Core;
using SpeechClient.Audio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace SpeechClient.UI
{
    public class MainViewModel : NotifyPropertyChanged
    {
        private readonly SpeechRecognizer speechRecognizer;
        private readonly IUIThread uiThread;
        private bool isSpeaking;

        public MainViewModel(SpeechRecognizer speechRecognizer, IUIThread uiThread)
        {
            this.speechRecognizer = speechRecognizer ?? throw new ArgumentNullException(nameof(speechRecognizer));
            this.uiThread = uiThread ?? throw new ArgumentNullException(nameof(uiThread));
            this.RecognitionResults = new ObservableCollection<RecognitionResult>();
        }

        private string currentRecognitionText;

        public string CurrentRecognitionText
        {
            get { return currentRecognitionText; }
            set { this.SetProperty(ref this.currentRecognitionText, value); }
        }

        public bool IsSpeaking
        {
            get { return isSpeaking; }
            set { this.SetProperty(ref this.isSpeaking, value); }
        }

        public ObservableCollection<RecognitionResult> RecognitionResults { get; set; }

        public async Task InitializeAsync()
        {
            this.speechRecognizer.SpeechStateChanged += SpeechRecognizer_SpeechStateChanged;
            this.speechRecognizer.IntentRecognized += SpeechRecognizer_IntentRecognized;

            await this.speechRecognizer.StartAsync();
        }

        private Task SpeechRecognizer_IntentRecognized(RecognitionResult result)
        {
            if (result.IsRecognizing)
            {
                this.CurrentRecognitionText = result.Text;
            }
            else
            {
                this.CurrentRecognitionText = string.Empty;
                this.uiThread.Run(() => this.RecognitionResults.Insert(0, result));
            }

            return Task.CompletedTask;
        }

        private Task SpeechRecognizer_SpeechStateChanged(SpeechState speechState)
        {
            this.uiThread.Run(() => this.IsSpeaking = speechState == SpeechState.Speaking);

            return Task.CompletedTask;
        }
    }
}
