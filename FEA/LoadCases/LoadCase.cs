namespace FEA.LoadCases
{
    public class LoadCase
    {
        public string Name { get; }

        public LoadCase(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
