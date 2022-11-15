using System.Collections.Immutable;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace FEA.LineElements
{
    public abstract class LineElement
    {
        public int nDOF { get; }

        protected LineElement(int ndof, string name, double area, double modulusOfElasticity, int node1, int node2)
        {
            this.nDOF = ndof;
            this.Name = name;
            this.Node1 = node1;
            this.Node2 = node2;

            this.Area = area;
            this.ModulusOfElasticity = modulusOfElasticity;

            var list1 = Enumerable
                .Range(0, nDOF)
                .Select(i => (Node1 + 1) * nDOF - (nDOF - i))
                .ToList();

            var list2 = Enumerable
                .Range(0, nDOF)
                .Select(i => (Node2 + 1) * nDOF - (nDOF - i))
                .ToList();

            var list = list1.Concat(list2).ToArray();

            this.EFT = ImmutableArray.Create(list);
        }

        public string Name { get; }

        public int Node2 { get; }

        public int Node1 { get; }

        public abstract void Build(double[] coord1, double[] coord2);

        public Matrix<double> Transformation { get; protected set; }

        public Matrix<double> Stiffness { get; protected set; }

        public double Area { get; }

        public double Length { get; protected set; }

        public double ModulusOfElasticity { get; }

        public ImmutableArray<int> EFT { get; protected set; }

        public double Elongation { get; set; }

        public double InternalForce { get; set; }

        public double Stress => InternalForce / Area;

        public double[] GlobalDisplacement { get; set; }

        public double[] LocalDisplacement { get; set; }
    }
}
