using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Intent;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechClient.Audio
{
    public delegate Task StateChangedHandler(SpeechState speechState);
    public delegate Task RecognizedHandler(SpeechClient.Audio.RecognitionResult result);

    public class SpeechRecognizer
    {
        private readonly SpeechRecognizerSettings settings;
        //private Microsoft.CognitiveServices.Speech.SpeechRecognizer speechRecognizer;
        private TaskCompletionSource<int> stopRecognition;

        public SpeechRecognizer(IOptions<SpeechRecognizerSettings> settings)
        {
            this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public event StateChangedHandler SpeechStateChanged;

        public event RecognizedHandler IntentRecognized;

        public async Task StartAsync(string fileName = null)
        {
            var speechConfig = SpeechConfig.FromSubscription(this.settings.SubscriptionKey, this.settings.Region);
            
            speechConfig.SpeechRecognitionLanguage = "de-de";
            speechConfig.OutputFormat = OutputFormat.Detailed;

            using (var audioInput = fileName == null ? AudioConfig.FromDefaultMicrophoneInput() : AudioConfig.FromWavFileInput(fileName))
            {
                using (var intentRecognizer = new IntentRecognizer(speechConfig, audioInput))
                {
                    stopRecognition = new TaskCompletionSource<int>();

                    var model = LanguageUnderstandingModel.FromAppId(this.settings.LuisAppId);
                    
                    intentRecognizer.AddAllIntents(model);
                    
                    intentRecognizer.SessionStarted += IntentRecognizer_SessionStarted;
                    intentRecognizer.Recognized += IntentRecognizer_Recognized;
                    intentRecognizer.Recognizing += IntentRecognizer_Recognizing;
                    intentRecognizer.SessionStopped += IntentRecognizer_SessionStopped;
                    intentRecognizer.SpeechEndDetected += IntentRecognizer_SpeechEndDetected;
                    intentRecognizer.SpeechStartDetected += IntentRecognizer_SpeechStartDetected;
                    intentRecognizer.Canceled += IntentRecognizer_Canceled;

                    await intentRecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    Task.WaitAny(stopRecognition.Task);

                    await intentRecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
        }

        private void IntentRecognizer_Canceled(object sender, IntentRecognitionCanceledEventArgs e)
        {
            stopRecognition.TrySetResult(0);
        }

        private void IntentRecognizer_SpeechStartDetected(object sender, RecognitionEventArgs e)
        {
            this.SpeechStateChanged?.Invoke(SpeechState.Speaking);
        }

        private void IntentRecognizer_SpeechEndDetected(object sender, RecognitionEventArgs e)
        {
            this.SpeechStateChanged?.Invoke(SpeechState.NotSpeaking);
        }

        private void IntentRecognizer_SessionStopped(object sender, SessionEventArgs e)
        {
            Console.WriteLine("Session stopped.");
        }

        private void IntentRecognizer_SessionStarted(object sender, SessionEventArgs e)
        {
            Console.WriteLine("Session started...");
        }

        private void IntentRecognizer_Recognized(object sender, IntentRecognitionEventArgs e)
        {
            if (e.Result.Reason == ResultReason.NoMatch)
            {
                return;
            }
            
            var json = e.Result.Properties.GetProperty(PropertyId.LanguageUnderstandingServiceResponse_JsonResult);

            var jsonObject = JObject.Parse(json);
            var entities = jsonObject.GetValue("entities").ToObject<IEnumerable<RecognizedEntity>>();

            var textParts = ExtractTextParts(entities, e.Result.Text);

            this.IntentRecognized?.Invoke(new RecognitionResult()
            {
                Intent = e.Result.IntentId,
                Text = e.Result.Text,
                IsRecognizing = false,
                Entities = entities,
                TextParts = textParts
            });
        }

        private static IEnumerable<TextPart> ExtractTextParts(IEnumerable<RecognizedEntity> entities, string recognizedText)
        {
            var textParts = new List<TextPart>();
            textParts.Add(new TextPart() { StartIndex = 0, EndIndex = recognizedText.Length, Text = recognizedText });

            foreach (var entity in entities)
            {
                var existingPart = textParts.First(t => t.StartIndex <= entity.StartIndex && t.EndIndex >= entity.EndIndex);

                var index = textParts.IndexOf(existingPart);
                textParts.RemoveAt(index);

                var entityPart = new TextPart()
                {
                    StartIndex = entity.StartIndex,
                    EndIndex = entity.EndIndex,
                    Text = entity.Entity,
                    Entity = entity
                };

                if (existingPart.EndIndex > entityPart.EndIndex)
                {
                    var lastPart = new TextPart()
                    {
                        StartIndex = entityPart.EndIndex,
                        EndIndex = existingPart.EndIndex,
                        Text = existingPart.Text.Substring(entityPart.EndIndex - existingPart.StartIndex + 1)
                    };

                    textParts.Insert(index, lastPart);
                }

                textParts.Insert(index, entityPart);

                if (existingPart.StartIndex < entityPart.StartIndex)
                {
                    var prevPart = new TextPart()
                    {
                        StartIndex = existingPart.StartIndex,
                        EndIndex = entityPart.StartIndex,
                        Text = existingPart.Text.Substring(0, entityPart.StartIndex - existingPart.StartIndex)
                    };

                    textParts.Insert(index, prevPart);
                }
            }

            return textParts;
        }

        private void IntentRecognizer_Recognizing(object sender, IntentRecognitionEventArgs e)
        {
            if (e.Result.Reason == ResultReason.NoMatch)
            {
                return;
            }

            this.IntentRecognized?.Invoke(new RecognitionResult()
            {
                Intent = e.Result.IntentId,
                Text = e.Result.Text,
                IsRecognizing = true
            });
        }
    }
}
