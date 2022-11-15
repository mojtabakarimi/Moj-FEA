namespace FEA.LoadDefinition
{
    public class LoadPattern
    {
        public string Name { get; }
        public bool SelftWeight { get; }

        public LoadPattern(string name, bool selftWeight)
        {
            this.Name = name;
            this.SelftWeight = selftWeight;
        }
    }
}
