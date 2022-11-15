using System.Collections.Immutable;

namespace FEA.Force
{
    public class NodalForce
    {
        public int NodeIndex { get; }

        public int LoadCaseIndex { get; }

        public ImmutableArray<double> Force { get; }

        public NodalForce(int nodeIndex, int loadCaseIndex, double[] force)
        {
            NodeIndex = nodeIndex;
            LoadCaseIndex = loadCaseIndex;
            this.Force = ImmutableArray.Create(force);
        }
    }
}
