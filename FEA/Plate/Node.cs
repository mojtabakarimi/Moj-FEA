using System.Collections.Generic;
using System.Collections.Immutable;

namespace FEA.Plate
{
    public class PlateNode: Nodes.Node
    {
        public ImmutableArray<double> Coord { get; }

        public double SpringStiffness;

        public double AreaSpringStiffness;

        public double[,] Load;

        public bool[] BoundaryCondition;
        public double[,] Displacement;

        public double[,] Reaction;
        public double[] SoilReaction;

        public double[] SoilReactionPerArea;
        public PlateNode(string name, double x, double y)
            : base(name, new []{x, y}, 2, 3)
        {
            //todo: remove following 2 lines
            var NumberOfLoadPattern = 0;
            var NumberOfLoadCase = 0;

            this.X = x;
            this.Y = y;

            this.Coord = ImmutableArray.Create(x, y);

            Load = new double[nDOF, NumberOfLoadPattern];
            BoundaryCondition = new bool[nDOF];

            Displacement = new double[nDOF, NumberOfLoadCase];
            Reaction = new double[nDOF, NumberOfLoadCase];

            SoilReaction = new double[NumberOfLoadCase];
            SoilReactionPerArea = new double[NumberOfLoadCase];
        }

        public double X { get; }

        public double Y { get; }
    }

}
