using System;
using MathNet.Numerics;

namespace FEA.Common.Mathematics
{
    public readonly struct Point : IEquatable<Point>
    {
        private const double tolerance = 0.00000001;
        public double X { get; }
        public double Y { get; }

        public Point(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        public static bool operator ==(Point P1, Point P2)
        {
            return (P1.X.AlmostEqual(P2.X) && P1.Y.AlmostEqual(P2.Y));
        }

        public static bool operator !=(Point P1, Point P2)
        {
            return !(P1 == P2);
        }

        public override bool Equals(object obj)
        {
            return obj != null && (this == (Point)obj);
        }

        public override int GetHashCode() => this.GetHashCode();

        bool IEquatable<Point>.Equals(Point Point)
        {
            return this == Point;
        }

        public double GetDistance(Point p)
        {
            return Math.Sqrt(Math.Pow(p.X - X, 2) + Math.Pow(p.Y - Y, 2));
        }

        public static double GetDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
    }

}
