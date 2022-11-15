using System;
using MathNet.Numerics.LinearAlgebra;

namespace FEA.LineElements
{
    public class Frame2D: LineElement
    {
        public Frame2D(string name, int node1, int node2, double area, double modulusOfElasticity, double Ix)
            : base(3, name, area, modulusOfElasticity, node1, node2)
        {
            this.Ix = Ix;
        }
        
        public override void Build(double[] coord1, double[] coord2)
        {
            var x1 = coord1[0];
            var y1 = coord1[1];

            var x2 = coord2[0];
            var y2 = coord2[1];

            var x21 = x2 - x1;
            var y21 = y2 - y1;

            var LL = Math.Pow(x21, 2) + Math.Pow(y21, 2);
            var L = Math.Sqrt(LL);
            var LLL = LL * L;

            var EAL = ModulusOfElasticity * Area / L;
            var EIL3 = 2 * ModulusOfElasticity * Ix / LLL;

            var C = x21 / L;
            var S = y21 / L;

            var matrixBuilder = Matrix<double>.Build;
            Stiffness = matrixBuilder.SparseOfArray(new[,]
            {
                {EAL, 0, 0, -EAL, 0, 0},
                {0, 6 * EIL3, 3 * L * EIL3, 0, -6 * EIL3, 3 * L * EIL3},
                {0, 3 * L * EIL3, 2 * LL * EIL3, 0, -3 * L * EIL3, LL * EIL3},
                {-EAL, 0, 0, EAL, 0, 0},
                {0, -6 * EIL3, -3 * L * EIL3, 0, 6 * EIL3, -3 * L * EIL3},
                {0, 3 * L * EIL3, LL * EIL3, 0, -3 * L * EIL3, 2 * LL * EIL3}
            });
            
            Transformation = matrixBuilder.SparseOfArray(new[,]
            {
                {C, S, 0, 0, 0, 0},
                {-S, C, 0, 0, 0, 0},
                {0, 0, 1, 0, 0, 0},
                {0, 0, 0, C, S, 0},
                {0, 0, 0, -S, C, 0},
                {0, 0, 0, 0, 0, 1}
            });
            
            Stiffness = Transformation.Transpose() * Stiffness * Transformation;

            Length = L;
        }

        public double Ix { get; }
    }
}
