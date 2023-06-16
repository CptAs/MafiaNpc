using System;

namespace MafiaNpc.MafiaImproved
{
    public enum Emotion
    {
           Happy,
           Angry,
           Excited,
           Sad,
           Relaxed,
           Frustrated,
           Surprised,
           Content,
           Nervous,
           Irritated,
           Neutral
    }
    public class PadEmotionalModel
    {
        
        public double Pleasure { get; private set; }
        public double Arousal { get; private set; }
        public double Dominance { get; private set; }
        public Emotion Emotion { get; private set; }

        public PadEmotionalModel(double pleasure, double arousal, double dominance)
        {
            Pleasure = pleasure;
            Arousal = arousal;
            Dominance = dominance;
            Emotion = CalculateEmotionalState();
        }

        public Emotion CalculateEmotionalState()
        {
            if (Pleasure > 0.7 && Arousal < 0.3 && Dominance > 0.7)
            {
                return Emotion.Happy;
            }
            else if (Pleasure < 0.3 && Arousal > 0.7 && Dominance < 0.3)
            {
                return Emotion.Angry;
            }
            else if (Pleasure > 0.7 && Arousal > 0.7 && Dominance > 0.7)
            {
                return Emotion.Excited;
            }
            else if (Pleasure < 0.3 && Arousal < 0.3 && Dominance < 0.3)
            {
                return Emotion.Sad;
            }
            else if (Pleasure > 0.7 && Arousal < 0.3 && Dominance < 0.3)
            {
                return Emotion.Relaxed;
            }
            else if (Pleasure < 0.3 && Arousal > 0.7 && Dominance > 0.7)
            {
                return Emotion.Frustrated;
            }
            else if (Pleasure > 0.5 && Arousal > 0.5 && Dominance < 0.5)
            {
                return Emotion.Surprised;
            }
            else if (Pleasure < 0.5 && Arousal < 0.5 && Dominance > 0.5)
            {
                return Emotion.Content;
            }
            else if (Pleasure > 0.5 && Arousal > 0.5 && Dominance < 0.5)
            {
                return Emotion.Nervous;
            }
            else if (Pleasure < 0.5 && Arousal > 0.5 && Dominance > 0.5)
            {
                return Emotion.Irritated;
            }
            else
            {
                return Emotion.Neutral;
            }
        }
    }
}