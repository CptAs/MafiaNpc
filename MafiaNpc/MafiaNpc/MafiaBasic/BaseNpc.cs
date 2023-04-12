namespace MafiaNpc
{
    public class BaseNpc
    {
        public string Name { get; set; }
        public double KillingProbability { get; set; }
        public double ExileProbability { get; set; }
        public bool IsActive { get; set; }
        public bool IsMafia { get; set; }

        public BaseNpc(string name, double killingProbability, double exileProbability, bool isMafia)
        {
            Name = name;
            KillingProbability = killingProbability;
            ExileProbability = exileProbability;
            IsMafia = isMafia;
            IsActive = true;
        }
        
        public void Kill(BaseNpc target)
        {
            target.IsActive = false;
        }
    }
}