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
            new Dictionary<Emotion, (double, double, double)>
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

        private (double, double, double) GetPadValuesFromEmotion(Emotion emotion)
        {
            switch (emotion)
            {
                case Emotion.Angry:
                    return (-.51, .59, .25);
                case Emotion.Bored:
                    return (-.65, -.62, -.33);
                case Emotion.Curious:
                    return (.22, .62, -.01);
                case Emotion.Dignified:
                    return (.55, .22, .61);
                case Emotion.Elated:
                    return (.50, .42, .23);
                case Emotion.Hungry:
                    return (-.44, .14, -.21);
                case Emotion.Inhibited:
                    return (-.54, -.04, -.41);
                case Emotion.Loved:
                    return (.87, .54, -.18);
                case Emotion.Puzzled:
                    return (-.41, .48, -.33);
                case Emotion.Sleepy:
                    return (.20, -.70, -.44);
                case Emotion.Unconcerned:
                    return (-.13, -.41, .08);
                case Emotion.Violent:
                    return (-.50, .62, .38);
            }

            return (0, 0, 0);
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