using System;
using System.Collections.Generic;
using System.Linq;

namespace MafiaNpc.MafiaImproved
{
    public enum Emotion
    {
        Angry,
        Bored,
        Curious,
        Dignified,
        Elated,
        Hungry,
        Inhibited,
        Loved,
        Puzzled,
        Sleepy,
        Unconcerned,
        Violent
    }

    public class PadEmotionalModel
    {
        public Emotion BaseEmotion { get; private set; }

        public Dictionary<Emotion, (double, double, double)> EmotionsPadValues =
            new()
            {
                { Emotion.Angry, (-.51, .59, .25) },
                { Emotion.Bored, (-.65, -.62, -.33) },
                { Emotion.Curious, (.22, .62, -.01) },
                { Emotion.Dignified, (.55, .22, .61) },
                { Emotion.Elated, (-.65, -.62, -.33) },
                { Emotion.Hungry, (-.65, -.62, -.33) },
                { Emotion.Inhibited, (-.65, -.62, -.33) },
                { Emotion.Loved, (-.65, -.62, -.33) },
                { Emotion.Puzzled, (-.65, -.62, -.33) },
                { Emotion.Sleepy, (-.65, -.62, -.33) },
                { Emotion.Unconcerned, (-.65, -.62, -.33) },
                { Emotion.Violent, (-.65, -.62, -.33) }
            };

        public PadEmotionalModel(Emotion baseEmotion)
        {
            BaseEmotion = baseEmotion;
        }

        public Emotion CalculateEmotionalState(List<Memory> memories)
        {
            var emotions = memories.Select(x => x.Emotion).ToList();
            emotions.Add(BaseEmotion);
            var padEmotions = emotions.ConvertAll(x => EmotionsPadValues[x]);
            var pleasure = padEmotions.Sum(x => x.Item1) / padEmotions.Count;
            var arousal = padEmotions.Sum(x => x.Item2) / padEmotions.Count;
            var dominance = padEmotions.Sum(x => x.Item3) / padEmotions.Count;
            return GetEmotionFromPadValues((pleasure, arousal, dominance));
        }

        private Emotion GetEmotionFromPadValues((double, double, double) padValues)
        {
            var lowestDistance = double.MinValue;
            var lowestEmotion = BaseEmotion;
            foreach (var emotions in EmotionsPadValues)
            {
                var distance = GetDistance(padValues, emotions.Value);
                if (distance < lowestDistance)
                {
                    lowestEmotion = emotions.Key;
                }
            }

            return lowestEmotion;
        }

        private double GetDistance((double, double, double) p1, (double, double, double) p2)
        {
            var deltaX = p2.Item1 - p1.Item1;
            var deltaY = p2.Item2 - p1.Item2;
            var deltaZ = p2.Item3 - p1.Item3;
            return Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2));
        }
    }
}