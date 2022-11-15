using System;
using System.Collections.Immutable;
using FEA.Interfaces;

namespace FEA.Nodes
{
    public abstract class Node : INode
    {
        public int nDIM { get; }

        public int nDOF { get; }

        public string Name { get; }
        
        public ImmutableArray<double> Coordinates { get; }

        public bool[] IsSupport { get; set; }

        public double[] Support { get; set; }
        
        public double[][] ReactionForce { get; set; }

        public double[][] ResultantDisplacement { get; set; }

        protected Node(string name, double[] coord, int ndim, int ndof)
        {
            this.Name = name;
            if (coord.Length != ndim)
            {
                throw new ArgumentException("Dimension incompatibility!");
            }
            this.Coordinates = ImmutableArray.Create(coord);
            this.nDIM = ndim;
            this.nDOF = ndof;

            IsSupport = new bool[nDOF];
            Support = new double[nDOF];
        }
    }
}
