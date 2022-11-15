namespace FEA.Materials
{
    public class AreaMaterial : Material
    {
        public double Nu { get; }

        public AreaMaterial(string name, double modulusOfElasticity, double nu)
            : base(name, modulusOfElasticity)
        {
            this.Nu = nu;
        }
    }
}
