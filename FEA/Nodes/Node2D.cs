namespace FEA.Nodes
{
    public class Node2D : Node
    {
        public Node2D(string name, double[] coord, int nDOF)
            : base(name, coord, 2, nDOF)
        {
            this.X = coord[0];
            this.Y = coord[1];
        }

        public double X { get; }

        public double Y { get; }
    }
}