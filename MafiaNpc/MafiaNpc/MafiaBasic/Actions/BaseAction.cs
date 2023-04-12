namespace MafiaNpc.Actions
{
    public interface BaseAction
    {
        public BaseNpc Source { get; set; }
        public BaseNpc Target { get; set; }
        public double Value { get; set; }
        public void Act();
    }
}