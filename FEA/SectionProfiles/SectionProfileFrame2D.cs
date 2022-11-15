namespace FEA.SectionProfiles
{
    public class SectionProfileFrame2D: ProfileSection
    {
        public double Ix { get; set; }

        public SectionProfileFrame2D(string name, double area, double Ix) 
            : base(name, area)
        {
            this.Ix = Ix;
        }
    }
}
