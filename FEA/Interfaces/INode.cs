using System.Collections.Immutable;

namespace FEA.Interfaces
{
    public interface INode
    {
        string Name { get; }

        ImmutableArray<double> Coordinates { get; }

        bool[] IsSupport { get; set; }
        double[] Support { get; set; }

        double[][] ReactionForce { get; set; }
        double[][] ResultantDisplacement { get; set; }
    }
}
