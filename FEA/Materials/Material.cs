namespace FEA.Materials
{
    public class Material
    {
        public string Name { get; }
        public double ModulusOfElasticity { get; }

        public Material(string name, double modulusOfElasticity)
        {
            this.Name = name;
            this.ModulusOfElasticity = modulusOfElasticity;
        }
    }
}
