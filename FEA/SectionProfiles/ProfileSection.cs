namespace FEA.SectionProfiles
{
    public class ProfileSection
    {
        public string Name { get; }

        /// <summary>
        /// Cross section area, mm2
        /// </summary>
        public double A { get; }

        public ProfileSection(string name, double area)
        {
            this.Name = name;
            this.A = area;
        }
    }
}
