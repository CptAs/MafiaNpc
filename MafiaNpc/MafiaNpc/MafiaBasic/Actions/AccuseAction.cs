namespace MafiaNpc.Actions
{
    public class AccuseAction : BaseAction
    {
        public BaseNpc Source { get; set; }
        public BaseNpc Target { get; set; }
        public double Value { get; set; }
        
        public AccuseAction(BaseNpc source, BaseNpc target, double value)
        {
            Source = source;
            Target = target;
            Value = value;
        }
        
        public void Act()
        {
            Target.ExileProbability += Value;
            Source.KillingProbability += Value;
        }
    }
}