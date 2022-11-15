namespace FEA.Plate
{
    public class AreaLoad
    {
        public int AreaName { get; }

        public int PatternName { get; }

        public double Value { get; }

        public AreaLoad(int areaName, int caseName, double value)
        {
            this.PatternName = caseName;
            this.Value = value;
            this.AreaName = areaName;
        }
    }
}
