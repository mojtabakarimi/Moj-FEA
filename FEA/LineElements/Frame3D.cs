using System;
using MathNet.Numerics.LinearAlgebra;

namespace FEA.LineElements
{
    public class Frame3D: LineElement
    {
        public Frame3D(string name, int node1, int node2, double area, double modulusOfElasticity, double PoisonRatio, double Ix, double Iy, double J)
            : base(6, name, area, modulusOfElasticity, node1, node2)
        {
            this.PoisonRatio = PoisonRatio;
            this.Ix = Ix;
            this.Iy = Iy;
            this.J = J;
        }
        
        public override void Build(double[] coord1, double[] coord2)
        {
            const double TOLERANCE = 0.00000001;

            var x1 = coord1[0];
            var y1 = coord1[1];
            var z1 = coord1[2];

            var x2 = coord2[0];
            var y2 = coord2[1];
            var z2 = coord2[2];

            var x21 = x2 - x1;
            var y21 = y2 - y1;
            var z21 = z2 - z1;

            var Gm = ModulusOfElasticity / 2 / (1 + PoisonRatio);
            var EA = ModulusOfElasticity * Area;
            var EIzz = ModulusOfElasticity * Ix;
            var EIyy = ModulusOfElasticity * Iy;
            var GJ = Gm * J;
            var L = Math.Sqrt(Math.Pow(x21, 2) + Math.Pow(y21, 2) + Math.Pow(z21, 2));
            var ra = EA / L;
            var rx = GJ / L;
            var ry = 2 * EIyy / L;
            var ry2 = 6 * EIyy / Math.Pow(L, 2);
            var ry3 = 12 * EIyy / Math.Pow(L, 3);
            var rz = 2 * EIzz / L;
            var rz2 = 6 * EIzz / Math.Pow(L, 2);
            var rz3 = 12 * EIzz / Math.Pow(L, 3);

            var matrixBuilder = Matrix<double>.Build;
            Stiffness = matrixBuilder.SparseOfArray(new[,]
            {
                {ra, 0, 0, 0, 0, 0, -ra, 0, 0, 0, 0, 0},
                {0, rz3, 0, 0, 0, rz2, 0, -rz3, 0, 0, 0, rz2},
                {0, 0, ry3, 0, -ry2, 0, 0, 0, -ry3, 0, -ry2, 0},
                {0, 0, 0, rx, 0, 0, 0, 0, 0, -rx, 0, 0},
                {0, 0, -ry2, 0, 2 * ry, 0, 0, 0, ry2, 0, ry, 0},
                {0, rz2, 0, 0, 0, 2 * rz, 0, -rz2, 0, 0, 0, rz},
                {-ra, 0, 0, 0, 0, 0, ra, 0, 0, 0, 0, 0},
                {0, -rz3, 0, 0, 0, -rz2, 0, rz3, 0, 0, 0, -rz2},
                {0, 0, -ry3, 0, ry2, 0, 0, 0, ry3, 0, ry2, 0},
                {0, 0, 0, -rx, 0, 0, 0, 0, 0, rx, 0, 0},
                {0, 0, -ry2, 0, ry, 0, 0, 0, ry2, 0, 2 * ry, 0},
                {0, rz2, 0, 0, 0, rz, 0, -rz2, 0, 0, 0, 2 * rz}
            });

            var xm = (x1 + x2) / 2;
            var ym = (y1 + y2) / 2;
            var zm = (z1 + z2) / 2;
            var x0 = xm;
            var y0 = ym;
            var z0 = zm;
            if (Math.Abs(x1 - x2) > TOLERANCE)
            {
                y0++;
            }
            else
            {
                x0++;
            }

            var dx = x0 - xm;
            var dy = y0 - ym;
            var dz = z0 - zm;

            var czx = dz * y21 - dy * z21;
            var czy = dx * z21 - dz * x21;
            var czz = dy * x21 - dx * y21;
            var zL = Math.Sqrt(Math.Pow(czx, 2) + Math.Pow(czy, 2) + Math.Pow(czz, 2));
            czx /= zL;
            czy /= zL;
            czz /= zL;
            var cxx = x21 / L;
            var cxy = y21 / L;
            var cxz = z21 / L;
            var cyx = czy * cxz - czz * cxy;
            var cyy = czz * cxx - czx * cxz;
            var cyz = czx * cxy - czy * cxx;

            Transformation = matrixBuilder.SparseOfArray(new[,]
            {
                {cxx, cxy, cxz, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {cyx, cyy, cyz, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {czx, czy, czz, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, cxx, cxy, cxz, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, cyx, cyy, cyz, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, czx, czy, czz, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, cxx, cxy, cxz, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, cyx, cyy, cyz, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, czx, czy, czz, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, cxx, cxy, cxz},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, cyx, cyy, cyz},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, czx, czy, czz}
            });

            Stiffness = Transformation.Transpose() * Stiffness * Transformation;

            Length = L;
        }


        public double PoisonRatio { get; }

        public double Ix { get; }

        public double Iy { get; }

        public double J { get; }
    }
}
