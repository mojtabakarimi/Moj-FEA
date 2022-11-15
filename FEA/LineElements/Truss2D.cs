using System;
using MathNet.Numerics.LinearAlgebra;

namespace FEA.LineElements
{
    public class Truss2D : LineElement
    {
        public Truss2D(string name, int node1, int node2, double area, double modulusOfElasticity)
            : base(2, name, area, modulusOfElasticity, node1, node2)
        {

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

            var EAL3 = ModulusOfElasticity * Area / LLL;

            var C = x21 / L;
            var S = y21 / L;

            var matrixBuilder = Matrix<double>.Build;
            Stiffness = matrixBuilder.SparseOfArray(new[,]
            {
                {x21 * x21 * EAL3, x21 * y21 * EAL3, -x21 * x21 * EAL3, -x21 * y21 * EAL3},
                {y21 * x21 * EAL3, y21 * y21 * EAL3, -y21 * x21 * EAL3, -y21 * y21 * EAL3},
                {-x21 * x21 * EAL3, -x21 * y21 * EAL3, x21 * x21 * EAL3, x21 * y21 * EAL3},
                {-y21 * x21 * EAL3, -y21 * y21 * EAL3, y21 * x21 * EAL3, y21 * y21 * EAL3}
            });

            Transformation = matrixBuilder.SparseOfArray(new[,]
            {
                {C, S, 0, 0},
                {-S, C, 0, 0},
                {0, 0, C, S},
                {0, 0, -S, C}
            });

            Length = L;
        }
    }
}
