using System;
using System.Collections.Generic;
using FEA.Plate;
using MathNet.Numerics.LinearAlgebra;

namespace FEA.Common.Mathematics
{
    public class MathAdv
    {
        public static bool IsPolygonConvex(IList<Point> Points)
        {
            if (Points == null || Points.Count < 3)
            {
                return false;
            }
            if (Points.Count == 3)
            {
                return true;
            }

            double zCrossProduct = 0;
            var length = Points.Count;
            for (var i = 0; i < length; i++)
            {
                var j0 = i;
                var j1 = i + 1;
                var j2 = i + 2;

                if (j1 > length - 1)
                {
                    j1 -= length;
                }

                if (j2 > length - 1)
                {
                    j2 -= length;
                }

                var dx1 = (double)(Points[j1].X - Points[j0].X);
                var dy1 = (double)(Points[j1].Y - Points[j0].Y);

                var dx2 = (double)(Points[j2].X - Points[j1].X);
                var dy2 = (double)(Points[j2].Y - Points[j1].Y);

                if (i == 0)
                {
                    zCrossProduct = Math.Sign(dx1 * dy2 - dy1 * dx2);
                }
                else
                {
                    if (!(zCrossProduct * Math.Sign(dx1 * dy2 - dy1 * dx2) > 0))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public double[] Interpolation(double[] X, double[] Y, double[] Z)
        {
            const int rowCount = 4;
            if (X.Length != rowCount || Y.Length != rowCount || Z.Length != rowCount)
            {
                throw new Exception("dsf");
            }

            var matrixBuilder = Matrix<double>.Build;
            var A = matrixBuilder.DenseOfArray(new[,]
            {
                {1, X[0], Y[0], X[0] * Y[0]},
                {1, X[1], Y[1], X[1] * Y[1]},
                {1, X[2], Y[2], X[2] * Y[2]},
                {1, X[3], Y[3], X[3] * Y[3]}
            });

            var interpolation = A
                .Solve(Vector<double>.Build
                    .DenseOfArray(Z))
                .ToArray();

            return interpolation;
        }
    }
}
