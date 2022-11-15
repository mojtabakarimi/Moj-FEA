using System;
using MathNet.Numerics.LinearAlgebra;

namespace FEA.LineElements
{
    public class Truss3D : LineElement
    {
        public Truss3D(string name, int node1, int node2, double area, double modulusOfElasticity)
            : base(3, name, area, modulusOfElasticity, node1, node2)
        {

        }
        
        public override void Build(double[] coord1, double[] coord2)
        {
            var x1 = coord1[0];
            var y1 = coord1[1];
            var z1 = coord1[2];

            var x2 = coord2[0];
            var y2 = coord2[1];
            var z2 = coord2[2];

            var x21 = x2 - x1;
            var y21 = y2 - y1;
            var z21 = z2 - z1;

            var L = Math.Sqrt(Math.Pow(x21, 2) + Math.Pow(y21, 2));
            var LLL = Math.Pow(L, 3);

            var C = x21 / L;
            var S = y21 / L;

            var EAL3 = ModulusOfElasticity * Area / LLL;

            var matrixBuilder = Matrix<double>.Build;
            Stiffness = matrixBuilder.SparseOfArray(new[,]
            {
                {x21 * x21 * EAL3, x21 * y21 * EAL3, x21 * z21 * EAL3, -x21 * x21 * EAL3, -x21 * y21 * EAL3, -x21 * z21 * EAL3},
                {y21 * x21 * EAL3, y21 * y21 * EAL3, y21 * z21 * EAL3, -y21 * x21 * EAL3, -y21 * y21 * EAL3, -y21 * z21 * EAL3},
                {z21 * x21 * EAL3, z21 * y21 * EAL3, z21 * z21 * EAL3, -z21 * x21 * EAL3, -z21 * y21 * EAL3, -z21 * z21 * EAL3},
                {-x21 * x21 * EAL3, -x21 * y21 * EAL3, -x21 * z21 * EAL3, x21 * x21 * EAL3, x21 * y21 * EAL3, x21 * z21 * EAL3},
                {-y21 * x21 * EAL3, -y21 * y21 * EAL3, -y21 * z21 * EAL3, y21 * x21 * EAL3, y21 * y21 * EAL3, y21 * z21 * EAL3},
                {-z21 * x21 * EAL3, -z21 * y21 * EAL3, -z21 * z21 * EAL3, z21 * x21 * EAL3, z21 * y21 * EAL3, z21 * z21 * EAL3}
            });

            Transformation = matrixBuilder.SparseOfArray(new[,]
            {
                {x21 * x21, x21 * y21, x21 * z21, -x21 * x21, -x21 * y21, -x21 * z21},
                {y21 * x21, y21 * y21, y21 * z21, -y21 * x21, -y21 * y21, -y21 * z21},
                {z21 * x21, z21 * y21, z21 * z21, -z21 * x21, -z21 * y21, -z21 * z21},
                {-x21 * x21, -x21 * y21, -x21 * z21, x21 * x21, x21 * y21, x21 * z21},
                {-y21 * x21, -y21 * y21, -y21 * z21, y21 * x21, y21 * y21, y21 * z21},
                {-z21 * x21, -z21 * y21, -z21 * z21, z21 * x21, z21 * y21, z21 * z21}
            });

            Length = L;
        }

    }
}
