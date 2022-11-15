using System.Collections.Generic;
using System.Collections.Immutable;

namespace FEA.Plate.Objects
{
    public class NodeObject
    {
        public string Name { get; }
        public ImmutableArray<double> Coord { get; }

        public double X { get; }

        public double Y { get; }

        public List<int> Connectivity;

        public double[] Displacement;

        public double SpringStiffness { get; set; }

        public bool[] BoundaryCondition;

        public NodeObject(string name, double x, double y)
        {
            this.Name = name;
            this.X = x;
            this.Y = y;
            this.Coord = ImmutableArray.Create(x, y);

            Connectivity = new List<int>();
        }
    }
}
