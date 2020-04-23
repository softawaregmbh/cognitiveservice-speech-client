using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using SpeechClient.Audio;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SpeechClient.Tests
{
    [TestClass]
    public class RecognizerTests
    {
        [TestMethod]
        public async Task TestTextParts()
        {
            var json = await File.ReadAllTextAsync("lkw-motorrad.json");

            var jsonObject = JObject.Parse(json);

            var entities = jsonObject.GetValue("entities").ToObject<IEnumerable<RecognizedEntity>>();
            var topIntent = jsonObject.GetValue("topScoringIntent").ToObject<RecognizedIntent>();

            var textParts = ExtractTextParts(entities, "ein lkw und ein motorrad").ToList();

            Assert.AreEqual("Ein ", textParts[0].Text, true);
            Assert.AreEqual("lkw", textParts[1].Text, true);
            Assert.AreEqual(" und ein ", textParts[2].Text, true);
            Assert.AreEqual("motorrad", textParts[3].Text, true);
        }

        private IEnumerable<TextPart> ExtractTextParts(IEnumerable<RecognizedEntity> entities, string recognizedText)
        {
            var textParts = new List<TextPart>();
            textParts.Add(new TextPart() { StartIndex = 0, EndIndex = recognizedText.Length - 1, Text = recognizedText });

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
                        Text = existingPart.Text.Substring(0, entityPart.StartIndex - existingPart.StartIndex - 1)
                    };

                    textParts.Insert(index, prevPart);
                }
            }

            return textParts;
        }
    }
}
