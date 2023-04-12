namespace MafiaNpc.Actions
{
    public class DefendAction : BaseAction
    {
        public BaseNpc Source { get; set; }
        public BaseNpc Target { get; set; }
        public double Value { get; set; }
        
        public DefendAction(BaseNpc source, BaseNpc target, double value)
        {
            Source = source;
            Target = target;
            Value = value;
        }
        
        public void Act()
        {
            Target.ExileProbability -= Value;
            Target.KillingProbability += Value;
            Source.ExileProbability -= Value;
            Source.KillingProbability += Value;
        }
    }
}