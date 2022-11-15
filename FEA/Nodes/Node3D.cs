namespace FEA.Nodes
{
    public class Node3D: Node
    {
        public Node3D(string name, double[] coord, int nDOF)
            : base(name, coord, 3, nDOF)
        {
            this.X = coord[0];
            this.Y = coord[1];
            this.Z = coord[2];
        }

        public double X { get; }

        public double Y { get; }

        public double Z { get; }
    }
}
