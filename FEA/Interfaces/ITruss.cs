using System.Collections.Immutable;
using MathNet.Numerics.LinearAlgebra;

namespace FEA.Interfaces
{
    public interface ITruss
    {
        string Name { get; }

        int Node2 { get; }

        int Node1 { get; }

        void Build(double[] coordinates1, double[] coordinates2);

        void FormEFT();

        Matrix<double> Transformation { get; set; }

        Matrix<double> Stiffness { get; set; }

        double Area { get; }

        double Length { get; set; }

        double ModulusOfElasticity { get; }

        ImmutableArray<int> EFT { get; set; }
    }
}
