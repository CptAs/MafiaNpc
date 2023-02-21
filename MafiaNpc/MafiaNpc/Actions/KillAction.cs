namespace MafiaNpc.Actions
{
    public class KillAction : BaseAction
    {
        public BaseNpc Source { get; set; }
        public BaseNpc Target { get; set; }
        public double Value { get; set; }
        
        public KillAction(BaseNpc source, BaseNpc target, double value)
        {
            Source = source;
            Target = target;
            Value = value;
        }
        
        public void Act()
        {
            Target.IsActive = false;
        }
    }
}