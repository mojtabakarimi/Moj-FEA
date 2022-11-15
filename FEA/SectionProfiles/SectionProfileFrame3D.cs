namespace FEA.SectionProfiles
{
    public class SectionProfileFrame3D: ProfileSection
    {
        public double PoisonRatio { get;  }

        public double Ix { get; }

        public double Iy { get; }

        public double J { get; }

        public SectionProfileFrame3D(string name, double area, double PoisonRatio, double Ix, double Iy, double J) 
            : base(name, area)
        {
            this.PoisonRatio = PoisonRatio;
            this.Ix = Ix;
            this.Iy = Iy;
            this.J = J;
        }
    }
}
