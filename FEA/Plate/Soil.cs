namespace FEA.Plate
{
    public class Soil
    {
        public string Name { get; }

        /// <summary>
        /// Per unit area
        /// </summary>
        public double SpringStiffness { get; }

        public Soil(string name, double springStiffness)
        {
            this.Name = name;
            this.SpringStiffness = springStiffness;
        }
    }
}
