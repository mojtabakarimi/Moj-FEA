using System;

namespace FEA.AreaElements
{
    public class AreaElement
    {
        private const int numberOfNodes = 4;

        public string Name { get; set; }

        
        public readonly int nDOF;
        public readonly int NumberOfNodes;

        public string ParrentObjectName;
        public int[] Nodes;

        public double[,] Stiffness;

        public double[] Load;
        public double[,] Displacement;
        public double[,] Reaction;

        public double[,] SoilReaction;
        public double X0 { get; set; }
        public double Y0;

        public double b { get; }
        public double c { get; }

        public double E { get; }
        public double Nu { get; }
        public double Thickness { get; }

        public double SoilStiffness;


        public AreaElement(string name, double thickness, double E, double nu, int[] nodes, double b, double c)
        {
            this.Name = name;
            this.Thickness = thickness;
            this.E = E;
            this.Nu = nu;
            this.b = b;
            this.c = c;

            Nodes = nodes;
            
            var numberOfLoadPattern = 0;
            var numberOfLoadCase = 0;

            this.NumberOfNodes = numberOfNodes;

            Load = new double[numberOfLoadPattern];

            Displacement = new double[numberOfNodes * nDOF, numberOfLoadCase];
            Reaction = new double[numberOfNodes * nDOF, numberOfLoadCase];
            SoilReaction = new double[numberOfNodes, numberOfLoadCase];
        }

        public void Build()
        {
            var h = Thickness;

            var nu = Nu;

            var D = E * Math.Pow(h, 3) / (12 * (1 - Math.Pow(nu, 2)));

            //Ref. : A First Course in the Finite Element Method(Fourth Edition) ,Page 522
            Stiffness = new[,]
            {
                {(2 * D * (10 * Math.Pow(b, 4) + 10 * Math.Pow(c, 4) + 7 * Math.Pow(b, 2) * Math.Pow(c, 2) - 2 * Math.Pow(b, 2) * Math.Pow(c, 2) * nu)) / (5 * Math.Pow(b, 3) * Math.Pow(c, 3)),(D * (4 * nu + 1)) / (5 * b) + (2 * b * D) / Math.Pow(c, 2),-(D * (4 * nu + 1)) / (5 * c) - (2 * c * D) / Math.Pow(b, 2),(2 * D * (5 * Math.Pow(b, 4) - 10 * Math.Pow(c, 4) - 7 * Math.Pow(b, 2) * Math.Pow(c, 2) + 2 * Math.Pow(b, 2) * Math.Pow(c, 2) * nu)) / (5 * Math.Pow(b, 3) * Math.Pow(c, 3)),(b * D) / Math.Pow(c, 2) - (D * (4 * nu + 1)) / (5 * b),(D * (nu - 1)) / (5 * c) - (2 * c * D) / Math.Pow(b, 2),-(2 * D * (5 * Math.Pow(b, 4) + 5 * Math.Pow(c, 4) - 7 * Math.Pow(b, 2) * Math.Pow(c, 2) + 2 * Math.Pow(b, 2) * Math.Pow(c, 2) * nu)) / (5 * Math.Pow(b, 3) * Math.Pow(c, 3)),(D * (nu - 1)) / (5 * b) + (b * D) / Math.Pow(c, 2),-(D * (nu - 1)) / (5 * c) - (c * D) / Math.Pow(b, 2),-(2 * D * (10 * Math.Pow(b, 4) - 5 * Math.Pow(c, 4) + 7 * Math.Pow(b, 2) * Math.Pow(c, 2) - 2 * Math.Pow(b, 2) * Math.Pow(c, 2) * nu)) / (5 * Math.Pow(b, 3) * Math.Pow(c, 3)),(2 * b * D) / Math.Pow(c, 2) - (D * (nu - 1)) / (5 * b),(D * (4 * nu + 1)) / (5 * c) - (c * D) / Math.Pow(b, 2)},
                {(D * (4 * nu + 1)) / (5 * b) + (2 * b * D) / Math.Pow(c, 2),(4 * b * D) / (3 * c) - (4 * c * D * (nu - 1)) / (15 * b),-nu * D,(b * D) / Math.Pow(c, 2) - (D * (4 * nu + 1)) / (5 * b),(2 * b * D) / (3 * c) + (2 * c * D * (2 * nu - 2)) / (15 * b),0,-(D * (nu - 1)) / (5 * b) - (b * D) / Math.Pow(c, 2),(b * D) / (3 * c) - (c * D * (nu - 1)) / (15 * b),0,(D * (nu - 1)) / (5 * b) - (2 * b * D) / Math.Pow(c, 2),(2 * b * D) / (3 * c) + (c * D * (nu - 1)) / (15 * b),0},
                {-(D * (4 * nu + 1)) / (5 * c) - (2 * c * D) / Math.Pow(b, 2),-nu * D,(4 * c * D) / (3 * b) - (4 * b * D * (nu - 1)) / (15 * c),(2 * c * D) / Math.Pow(b, 2) - (D * (nu - 1)) / (5 * c),0,(2 * c * D) / (3 * b) + (b * D * (nu - 1)) / (15 * c),(D * (nu - 1)) / (5 * c) + (c * D) / Math.Pow(b, 2),0,(c * D) / (3 * b) - (b * D * (nu - 1)) / (15 * c),(D * (4 * nu + 1)) / (5 * c) - (c * D) / Math.Pow(b, 2),0,(2 * c * D) / (3 * b) + (2 * b * D * (2 * nu - 2)) / (15 * c)},
                {(2 * D * (5 * Math.Pow(b, 4) - 10 * Math.Pow(c, 4) - 7 * Math.Pow(b, 2) * Math.Pow(c, 2) + 2 * Math.Pow(b, 2) * Math.Pow(c, 2) * nu)) / (5 * Math.Pow(b, 3) * Math.Pow(c, 3)),(b * D) / Math.Pow(c, 2) - (D * (4 * nu + 1)) / (5 * b),(2 * c * D) / Math.Pow(b, 2) - (D * (nu - 1)) / (5 * c),(2 * D * (10 * Math.Pow(b, 4) + 10 * Math.Pow(c, 4) + 7 * Math.Pow(b, 2) * Math.Pow(c, 2) - 2 * Math.Pow(b, 2) * Math.Pow(c, 2) * nu)) / (5 * Math.Pow(b, 3) * Math.Pow(c, 3)),(D * (4 * nu + 1)) / (5 * b) + (2 * b * D) / Math.Pow(c, 2),(D * (4 * nu + 1)) / (5 * c) + (2 * c * D) / Math.Pow(b, 2),-(2 * D * (10 * Math.Pow(b, 4) - 5 * Math.Pow(c, 4) + 7 * Math.Pow(b, 2) * Math.Pow(c, 2) - 2 * Math.Pow(b, 2) * Math.Pow(c, 2) * nu)) / (5 * Math.Pow(b, 3) * Math.Pow(c, 3)),(2 * b * D) / Math.Pow(c, 2) - (D * (nu - 1)) / (5 * b),(c * D) / Math.Pow(b, 2) - (D * (4 * nu + 1)) / (5 * c),-(2 * D * (5 * Math.Pow(b, 4) + 5 * Math.Pow(c, 4) - 7 * Math.Pow(b, 2) * Math.Pow(c, 2) + 2 * Math.Pow(b, 2) * Math.Pow(c, 2) * nu)) / (5 * Math.Pow(b, 3) * Math.Pow(c, 3)),(D * (nu - 1)) / (5 * b) + (b * D) / Math.Pow(c, 2),(D * (nu - 1)) / (5 * c) + (c * D) / Math.Pow(b, 2)},
                {(b * D) / Math.Pow(c, 2) - (D * (4 * nu + 1)) / (5 * b),(2 * b * D) / (3 * c) + (2 * c * D * (2 * nu - 2)) / (15 * b),0,(D * (4 * nu + 1)) / (5 * b) + (2 * b * D) / Math.Pow(c, 2),(4 * b * D) / (3 * c) - (4 * c * D * (nu - 1)) / (15 * b),nu * D,(D * (nu - 1)) / (5 * b) - (2 * b * D) / Math.Pow(c, 2),(2 * b * D) / (3 * c) + (c * D * (nu - 1)) / (15 * b),0,-(D * (nu - 1)) / (5 * b) - (b * D) / Math.Pow(c, 2),(b * D) / (3 * c) - (c * D * (nu - 1)) / (15 * b),0},
                {(D * (nu - 1)) / (5 * c) - (2 * c * D) / Math.Pow(b, 2),0,(2 * c * D) / (3 * b) + (b * D * (nu - 1)) / (15 * c),(D * (4 * nu + 1)) / (5 * c) + (2 * c * D) / Math.Pow(b, 2),nu * D,(4 * c * D) / (3 * b) - (4 * b * D * (nu - 1)) / (15 * c),(c * D) / Math.Pow(b, 2) - (D * (4 * nu + 1)) / (5 * c),0,(2 * c * D) / (3 * b) + (2 * b * D * (2 * nu - 2)) / (15 * c),-(D * (nu - 1)) / (5 * c) - (c * D) / Math.Pow(b, 2),0,(c * D) / (3 * b) - (b * D * (nu - 1)) / (15 * c)},
                {-(2 * D * (5 * Math.Pow(b, 4) + 5 * Math.Pow(c, 4) - 7 * Math.Pow(b, 2) * Math.Pow(c, 2) + 2 * Math.Pow(b, 2) * Math.Pow(c, 2) * nu)) / (5 * Math.Pow(b, 3) * Math.Pow(c, 3)),-(D * (nu - 1)) / (5 * b) - (b * D) / Math.Pow(c, 2),(D * (nu - 1)) / (5 * c) + (c * D) / Math.Pow(b, 2),-(2 * D * (10 * Math.Pow(b, 4) - 5 * Math.Pow(c, 4) + 7 * Math.Pow(b, 2) * Math.Pow(c, 2) - 2 * Math.Pow(b, 2) * Math.Pow(c, 2) * nu)) / (5 * Math.Pow(b, 3) * Math.Pow(c, 3)),(D * (nu - 1)) / (5 * b) - (2 * b * D) / Math.Pow(c, 2),(c * D) / Math.Pow(b, 2) - (D * (4 * nu + 1)) / (5 * c),(2 * D * (10 * Math.Pow(b, 4) + 10 * Math.Pow(c, 4) + 7 * Math.Pow(b, 2) * Math.Pow(c, 2) - 2 * Math.Pow(b, 2) * Math.Pow(c, 2) * nu)) / (5 * Math.Pow(b, 3) * Math.Pow(c, 3)),-(D * (4 * nu + 1)) / (5 * b) - (2 * b * D) / Math.Pow(c, 2),(D * (4 * nu + 1)) / (5 * c) + (2 * c * D) / Math.Pow(b, 2),(2 * D * (5 * Math.Pow(b, 4) - 10 * Math.Pow(c, 4) - 7 * Math.Pow(b, 2) * Math.Pow(c, 2) + 2 * Math.Pow(b, 2) * Math.Pow(c, 2) * nu)) / (5 * Math.Pow(b, 3) * Math.Pow(c, 3)),(D * (4 * nu + 1)) / (5 * b) - (b * D) / Math.Pow(c, 2),(2 * c * D) / Math.Pow(b, 2) - (D * (nu - 1)) / (5 * c)},
                {(D * (nu - 1)) / (5 * b) + (b * D) / Math.Pow(c, 2),(b * D) / (3 * c) - (c * D * (nu - 1)) / (15 * b),0,(2 * b * D) / Math.Pow(c, 2) - (D * (nu - 1)) / (5 * b),(2 * b * D) / (3 * c) + (c * D * (nu - 1)) / (15 * b),0,-(D * (4 * nu + 1)) / (5 * b) - (2 * b * D) / Math.Pow(c, 2),(4 * b * D) / (3 * c) - (4 * c * D * (nu - 1)) / (15 * b),-nu * D,(D * (4 * nu + 1)) / (5 * b) - (b * D) / Math.Pow(c, 2),(2 * b * D) / (3 * c) + (2 * c * D * (2 * nu - 2)) / (15 * b),0},
                {-(D * (nu - 1)) / (5 * c) - (c * D) / Math.Pow(b, 2),0,(c * D) / (3 * b) - (b * D * (nu - 1)) / (15 * c),(c * D) / Math.Pow(b, 2) - (D * (4 * nu + 1)) / (5 * c),0,(2 * c * D) / (3 * b) + (2 * b * D * (2 * nu - 2)) / (15 * c),(D * (4 * nu + 1)) / (5 * c) + (2 * c * D) / Math.Pow(b, 2),-nu * D,(4 * c * D) / (3 * b) - (4 * b * D * (nu - 1)) / (15 * c),(D * (nu - 1)) / (5 * c) - (2 * c * D) / Math.Pow(b, 2),0,(2 * c * D) / (3 * b) + (b * D * (nu - 1)) / (15 * c)},
                {-(2 * D * (10 * Math.Pow(b, 4) - 5 * Math.Pow(c, 4) + 7 * Math.Pow(b, 2) * Math.Pow(c, 2) - 2 * Math.Pow(b, 2) * Math.Pow(c, 2) * nu)) / (5 * Math.Pow(b, 3) * Math.Pow(c, 3)),(D * (nu - 1)) / (5 * b) - (2 * b * D) / Math.Pow(c, 2),(D * (4 * nu + 1)) / (5 * c) - (c * D) / Math.Pow(b, 2),-(2 * D * (5 * Math.Pow(b, 4) + 5 * Math.Pow(c, 4) - 7 * Math.Pow(b, 2) * Math.Pow(c, 2) + 2 * Math.Pow(b, 2) * Math.Pow(c, 2) * nu)) / (5 * Math.Pow(b, 3) * Math.Pow(c, 3)),-(D * (nu - 1)) / (5 * b) - (b * D) / Math.Pow(c, 2),-(D * (nu - 1)) / (5 * c) - (c * D) / Math.Pow(b, 2),(2 * D * (5 * Math.Pow(b, 4) - 10 * Math.Pow(c, 4) - 7 * Math.Pow(b, 2) * Math.Pow(c, 2) + 2 * Math.Pow(b, 2) * Math.Pow(c, 2) * nu)) / (5 * Math.Pow(b, 3) * Math.Pow(c, 3)),(D * (4 * nu + 1)) / (5 * b) - (b * D) / Math.Pow(c, 2),(D * (nu - 1)) / (5 * c) - (2 * c * D) / Math.Pow(b, 2),(2 * D * (10 * Math.Pow(b, 4) + 10 * Math.Pow(c, 4) + 7 * Math.Pow(b, 2) * Math.Pow(c, 2) - 2 * Math.Pow(b, 2) * Math.Pow(c, 2) * nu)) / (5 * Math.Pow(b, 3) * Math.Pow(c, 3)),-(D * (4 * nu + 1)) / (5 * b) - (2 * b * D) / Math.Pow(c, 2),-(D * (4 * nu + 1)) / (5 * c) - (2 * c * D) / Math.Pow(b, 2)},
                {(2 * b * D) / Math.Pow(c, 2) - (D * (nu - 1)) / (5 * b),(2 * b * D) / (3 * c) + (c * D * (nu - 1)) / (15 * b),0,(D * (nu - 1)) / (5 * b) + (b * D) / Math.Pow(c, 2),(b * D) / (3 * c) - (c * D * (nu - 1)) / (15 * b),0,(D * (4 * nu + 1)) / (5 * b) - (b * D) / Math.Pow(c, 2),(2 * b * D) / (3 * c) + (2 * c * D * (2 * nu - 2)) / (15 * b),0,-(D * (4 * nu + 1)) / (5 * b) - (2 * b * D) / Math.Pow(c, 2),(4 * b * D) / (3 * c) - (4 * c * D * (nu - 1)) / (15 * b),nu * D},
                {(D * (4 * nu + 1)) / (5 * c) - (c * D) / Math.Pow(b, 2),0,(2 * c * D) / (3 * b) + (2 * b * D * (2 * nu - 2)) / (15 * c),(D * (nu - 1)) / (5 * c) + (c * D) / Math.Pow(b, 2),0,(c * D) / (3 * b) - (b * D * (nu - 1)) / (15 * c),(2 * c * D) / Math.Pow(b, 2) - (D * (nu - 1)) / (5 * c),0,(2 * c * D) / (3 * b) + (b * D * (nu - 1)) / (15 * c),-(D * (4 * nu + 1)) / (5 * c) - (2 * c * D) / Math.Pow(b, 2),nu * D,(4 * c * D) / (3 * b) - (4 * b * D * (nu - 1)) / (15 * c)}
            };
        }

        public double Area => b * c;

        private double[,] CInv => new[,] { { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, { -3 / Math.Pow(b, 2), -2 / b, 0, 3 / Math.Pow(b, 2), -1 / b, 0, 0, 0, 0, 0, 0, 0 }, { -3 / Math.Pow(c, 2), 0, -2 / c, 0, 0, 0, 0, 0, 0, 3 / Math.Pow(c, 2), 0, -1 / c }, { -1 / (b * c), -1 / c, -1 / b, 1 / (b * c), 0, 1 / b, -1 / (b * c), 0, 0, 1 / (b * c), 1 / c, 0 }, { 3 / (Math.Pow(b, 2) * c), 2 / (b * c), 0, -3 / (Math.Pow(b, 2) * c), 1 / (b * c), 0, 3 / (Math.Pow(b, 2) * c), -1 / (b * c), 0, -3 / (Math.Pow(b, 2) * c), -2 / (b * c), 0 }, { 3 / (b * Math.Pow(c, 2)), 0, 2 / (b * c), -3 / (b * Math.Pow(c, 2)), 0, -2 / (b * c), 3 / (b * Math.Pow(c, 2)), 0, -1 / (b * c), -3 / (b * Math.Pow(c, 2)), 0, 1 / (b * c) }, { 2 / Math.Pow(b, 3), 1 / Math.Pow(b, 2), 0, -2 / Math.Pow(b, 3), 1 / Math.Pow(b, 2), 0, 0, 0, 0, 0, 0, 0 }, { 2 / Math.Pow(c, 3), 0, 1 / Math.Pow(c, 2), 0, 0, 0, 0, 0, 0, -2 / Math.Pow(c, 3), 0, 1 / Math.Pow(c, 2) }, { -2 / (Math.Pow(b, 3) * c), -1 / (Math.Pow(b, 2) * c), 0, 2 / (Math.Pow(b, 3) * c), -1 / (Math.Pow(b, 2) * c), 0, -2 / (Math.Pow(b, 3) * c), 1 / (Math.Pow(b, 2) * c), 0, 2 / (Math.Pow(b, 3) * c), 1 / (Math.Pow(b, 2) * c), 0 }, { -2 / (b * Math.Pow(c, 3)), 0, -1 / (b * Math.Pow(c, 2)), 2 / (b * Math.Pow(c, 3)), 0, 1 / (b * Math.Pow(c, 2)), -2 / (b * Math.Pow(c, 3)), 0, 1 / (b * Math.Pow(c, 2)), 2 / (b * Math.Pow(c, 3)), 0, -1 / (b * Math.Pow(c, 2)) } };

        private double[,] CInv1 => new[,] { { 1, 0, 0, 0 }, { -1 / b, 1 / b, 0, 0 }, { -1 / c, 0, 0, 1 / c }, { 1 / (b * c), -1 / (b * c), 1 / (b * c), -1 / (b * c) } };

        public double[] GetStress(int caseIndex, double x, double y)
        {
            var coeff = E * Math.Pow(Thickness, 3) / (12 * (1 - Math.Pow(Nu, 2)));

            var DB = new double[3, 12];
            var result = new double[nDOF];

            var DD = new[,] { { 1, Nu, 0 }, { Nu, 1, 0 }, { 0, 0, 1 / 2.0 - Nu / 2 } };

            var BB = new[,] {
                    {(12 * (c - y)) / (Math.Pow(b, 3) * c),(6 * (c - y)) / (Math.Pow(b, 2) * c),0,-(12 * (c - y)) / (Math.Pow(b, 3) * c),(6 * (c - y)) / (Math.Pow(b, 2) * c),0,-(12 * y) / (Math.Pow(b, 3) * c),(6 * y) / (Math.Pow(b, 2) * c),0,(12 * y) / (Math.Pow(b, 3) * c),(6 * y) / (Math.Pow(b, 2) * c),0},
                    {(6 * (c - 2 * y)) / (b * Math.Pow(c, 3)),0,(2 * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)),-(6 * (c - 2 * y)) / (b * Math.Pow(c, 3)),0,-(2 * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)),(6 * (c - 2 * y)) / (b * Math.Pow(c, 3)),0,-(2 * (c - 3 * y)) / (b * Math.Pow(c, 2)),-(6 * (c - 2 * y)) / (b * Math.Pow(c, 3)),0,(2 * (c - 3 * y)) / (b * Math.Pow(c, 2))},
                    {(12 * (b - 2 * x)) / (Math.Pow(b, 3) * c),(4 * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c),0,-(12 * (b - 2 * x)) / (Math.Pow(b, 3) * c),(4 * (b - 3 * x)) / (Math.Pow(b, 2) * c),0,(12 * (b - 2 * x)) / (Math.Pow(b, 3) * c),-(4 * (b - 3 * x)) / (Math.Pow(b, 2) * c),0,-(12 * (b - 2 * x)) / (Math.Pow(b, 3) * c),-(4 * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c),0}
                };

            //Dim BD1(,) As Double = { _
            //{-(6 * (b - 2 * x) * (c - y)) / (b ^ 3 * c) - (6 * Nu * (b - x) * (c - 2 * y)) / (b * c ^ 3), -(2 * (c - y) * (2 * b - 3 * x)) / (b ^ 2 * c), -(2 * Nu * (b - x) * (2 * c - 3 * y)) / (b * c ^ 2), (6 * (b - 2 * x) * (c - y)) / (b ^ 3 * c) - (6 * Nu * x * (c - 2 * y)) / (b * c ^ 3), -(2 * (b - 3 * x) * (c - y)) / (b ^ 2 * c), -(2 * Nu * x * (2 * c - 3 * y)) / (b * c ^ 2), (6 * y * (b - 2 * x)) / (b ^ 3 * c) + (6 * Nu * x * (c - 2 * y)) / (b * c ^ 3), -(2 * y * (b - 3 * x)) / (b ^ 2 * c), -(2 * Nu * x * (c - 3 * y)) / (b * c ^ 2), (6 * Nu * (b - x) * (c - 2 * y)) / (b * c ^ 3) - (6 * y * (b - 2 * x)) / (b ^ 3 * c), -(2 * y * (2 * b - 3 * x)) / (b ^ 2 * c), -(2 * Nu * (b - x) * (c - 3 * y)) / (b * c ^ 2)}, _
            //{-(6 * (b - x) * (c - 2 * y)) / (b * c ^ 3) - (6 * Nu * (b - 2 * x) * (c - y)) / (b ^ 3 * c), -(2 * Nu * (c - y) * (2 * b - 3 * x)) / (b ^ 2 * c), -(2 * (b - x) * (2 * c - 3 * y)) / (b * c ^ 2), (6 * Nu * (b - 2 * x) * (c - y)) / (b ^ 3 * c) - (6 * x * (c - 2 * y)) / (b * c ^ 3), -(2 * Nu * (b - 3 * x) * (c - y)) / (b ^ 2 * c), -(2 * x * (2 * c - 3 * y)) / (b * c ^ 2), (6 * x * (c - 2 * y)) / (b * c ^ 3) + (6 * Nu * y * (b - 2 * x)) / (b ^ 3 * c), -(2 * Nu * y * (b - 3 * x)) / (b ^ 2 * c), -(2 * x * (c - 3 * y)) / (b * c ^ 2), (6 * (b - x) * (c - 2 * y)) / (b * c ^ 3) - (6 * Nu * y * (b - 2 * x)) / (b ^ 3 * c), -(2 * Nu * y * (2 * b - 3 * x)) / (b ^ 2 * c), -(2 * (b - x) * (c - 3 * y)) / (b * c ^ 2)}, _
            //{((Nu / 2 - 1 / 2) * (c ^ 2 * (2 * b ^ 2 - 12 * b * x + 12 * x ^ 2) + 12 * b ^ 2 * y ^ 2 - 12 * b ^ 2 * c * y)) / (b ^ 3 * c ^ 3), (2 * (Nu / 2 - 1 / 2) * (b - x) * (b - 3 * x)) / (b ^ 2 * c), (2 * (Nu / 2 - 1 / 2) * (c - y) * (c - 3 * y)) / (b * c ^ 2), -((Nu / 2 - 1 / 2) * (c ^ 2 * (2 * b ^ 2 - 12 * b * x + 12 * x ^ 2) + 12 * b ^ 2 * y ^ 2 - 12 * b ^ 2 * c * y)) / (b ^ 3 * c ^ 3), -(2 * x * (Nu / 2 - 1 / 2) * (2 * b - 3 * x)) / (b ^ 2 * c), -(2 * (Nu / 2 - 1 / 2) * (c - y) * (c - 3 * y)) / (b * c ^ 2), ((Nu / 2 - 1 / 2) * (c ^ 2 * (2 * b ^ 2 - 12 * b * x + 12 * x ^ 2) + 12 * b ^ 2 * y ^ 2 - 12 * b ^ 2 * c * y)) / (b ^ 3 * c ^ 3), (2 * x * (Nu / 2 - 1 / 2) * (2 * b - 3 * x)) / (b ^ 2 * c), (2 * y * (Nu / 2 - 1 / 2) * (2 * c - 3 * y)) / (b * c ^ 2), -((Nu / 2 - 1 / 2) * (c ^ 2 * (2 * b ^ 2 - 12 * b * x + 12 * x ^ 2) + 12 * b ^ 2 * y ^ 2 - 12 * b ^ 2 * c * y)) / (b ^ 3 * c ^ 3), -(2 * (Nu / 2 - 1 / 2) * (b - x) * (b - 3 * x)) / (b ^ 2 * c), -(2 * y * (Nu / 2 - 1 / 2) * (2 * c - 3 * y)) / (b * c ^ 2)}}


            for (var i = 0; i < nDOF; i++)
            {
                for (var j = 0; j < 12; j++)
                {
                    for (var k = 0; k < nDOF; k++)
                    {
                        DB[i, j] += coeff * DD[i, k] * BB[k, j];
                    }
                }
            }

            DB = new[,] {
                    {
                        -(6 * coeff * (b - 2 * x) * (c - y)) / (Math.Pow(b, 3) * c) - (6 * coeff * Nu * (b - x) * (c - 2 * y)) / (b * Math.Pow(c, 3)),
                        -(2 * coeff * (c - y) * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * Nu * (b - x) * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)),
                        (6 * coeff * (b - 2 * x) * (c - y)) / (Math.Pow(b, 3) * c) - (6 * coeff * Nu * x * (c - 2 * y)) / (b * Math.Pow(c, 3)),
                        -(2 * coeff * (b - 3 * x) * (c - y)) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * Nu * x * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)),
                        (6 * coeff * y * (b - 2 * x)) / (Math.Pow(b, 3) * c) + (6 * coeff * Nu * x * (c - 2 * y)) / (b * Math.Pow(c, 3)),
                        -(2 * coeff * y * (b - 3 * x)) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * Nu * x * (c - 3 * y)) / (b * Math.Pow(c, 2)),
                        (6 * coeff * Nu * (b - x) * (c - 2 * y)) / (b * Math.Pow(c, 3)) - (6 * coeff * y * (b - 2 * x)) / (Math.Pow(b, 3) * c),
                        -(2 * coeff * y * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * Nu * (b - x) * (c - 3 * y)) / (b * Math.Pow(c, 2))
                    },
                    {
                        -(6 * coeff * (b - x) * (c - 2 * y)) / (b * Math.Pow(c, 3)) - (6 * coeff * Nu * (b - 2 * x) * (c - y)) / (Math.Pow(b, 3) * c),
                        -(2 * coeff * Nu * (c - y) * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * (b - x) * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)),
                        (6 * coeff * Nu * (b - 2 * x) * (c - y)) / (Math.Pow(b, 3) * c) - (6 * coeff * x * (c - 2 * y)) / (b * Math.Pow(c, 3)),
                        -(2 * coeff * Nu * (b - 3 * x) * (c - y)) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * x * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)),
                        (6 * coeff * x * (c - 2 * y)) / (b * Math.Pow(c, 3)) + (6 * coeff * Nu * y * (b - 2 * x)) / (Math.Pow(b, 3) * c),
                        -(2 * coeff * Nu * y * (b - 3 * x)) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * x * (c - 3 * y)) / (b * Math.Pow(c, 2)),
                        (6 * coeff * (b - x) * (c - 2 * y)) / (b * Math.Pow(c, 3)) - (6 * coeff * Nu * y * (b - 2 * x)) / (Math.Pow(b, 3) * c),
                        -(2 * coeff * Nu * y * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * (b - x) * (c - 3 * y)) / (b * Math.Pow(c, 2))
                    },
                    {
                        (2 * coeff * (Nu / 2 - 1 / 2.0) * (Math.Pow(b, 2) * Math.Pow(c, 2) - 6 * Math.Pow(b, 2) * c * y + 6 * Math.Pow(b, 2) * Math.Pow(y, 2) - 6 * b * Math.Pow(c, 2) * x + 6 * Math.Pow(c, 2) * Math.Pow(x, 2))) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                        (2 * coeff * (Nu / 2 - 1 / 2.0) * (Math.Pow(b, 2) - 4 * b * x + 3 * Math.Pow(x, 2))) / (Math.Pow(b, 2) * c),
                        (2 * coeff * (Nu / 2 - 1 / 2.0) * (Math.Pow(c, 2) - 4 * c * y + 3 * Math.Pow(y, 2))) / (b * Math.Pow(c, 2)),
                        -(2 * coeff * (Nu / 2 - 1 / 2.0) * (Math.Pow(b, 2) * Math.Pow(c, 2) - 6 * Math.Pow(b, 2) * c * y + 6 * Math.Pow(b, 2) * Math.Pow(y, 2) - 6 * b * Math.Pow(c, 2) * x + 6 * Math.Pow(c, 2) * Math.Pow(x, 2))) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                        -(2 * coeff * x * (Nu / 2 - 1 / 2.0) * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * (Nu / 2 - 1 / 2.0) * (Math.Pow(c, 2) - 4 * c * y + 3 * Math.Pow(y, 2))) / (b * Math.Pow(c, 2)),
                        (2 * coeff * (Nu / 2 - 1 / 2.0) * (Math.Pow(b, 2) * Math.Pow(c, 2) - 6 * Math.Pow(b, 2) * c * y + 6 * Math.Pow(b, 2) * Math.Pow(y, 2) - 6 * b * Math.Pow(c, 2) * x + 6 * Math.Pow(c, 2) * Math.Pow(x, 2))) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                        (2 * coeff * x * (Nu / 2 - 1 / 2.0) * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c),
                        (2 * coeff * y * (Nu / 2 - 1 / 2.0) * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)),
                        -(2 * coeff * (Nu / 2 - 1 / 2.0) * (Math.Pow(b, 2) * Math.Pow(c, 2) - 6 * Math.Pow(b, 2) * c * y + 6 * Math.Pow(b, 2) * Math.Pow(y, 2) - 6 * b * Math.Pow(c, 2) * x + 6 * Math.Pow(c, 2) * Math.Pow(x, 2))) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                        -(2 * coeff * (Nu / 2 - 1 / 2.0) * (Math.Pow(b, 2) - 4 * b * x + 3 * Math.Pow(x, 2))) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * y * (Nu / 2 - 1 / 2.0) * (2 * c - 3 * y)) / (b * Math.Pow(c, 2))
                    }
                };

            for (var i = 0; i < nDOF; i++)
            {
                for (var k = 0; k < Nodes.Length * nDOF; k++)
                {
                    result[i] += DB[i, k] * Displacement[k, caseIndex];
                }
            }

            return result;
        }

        public double[] GetDisplacement(int caseIndex, double x, double y)
        {
            var pc = new double[nDOF, 12];
            var result = new double[nDOF];

            if ((x < 0 || x > b) || (y < 0 || y > c))
            {
                return null;
            }

            var P = new[,]
            {
                {1, x, y, Math.Pow(x, 2), Math.Pow(y, 2), x * y, Math.Pow(x, 2) * y, x * Math.Pow(y, 2), Math.Pow(x, 3), Math.Pow(y, 3), Math.Pow(x, 3) * y, x * Math.Pow(y, 3)},
                {0, 1, 0, 2 * x, 0, y, 2 * x * y, Math.Pow(y, 2), 3 * Math.Pow(x, 2), 0, 3 * Math.Pow(x, 2) * y, Math.Pow(y, 3)},
                {0, 0, 1, 0, 2 * y, x, Math.Pow(x, 2), 2 * x * y, 0, 3 * Math.Pow(y, 2), Math.Pow(x, 3), 3 * x * Math.Pow(y, 2)}
            };

            var CInv = this.CInv;

            for (var i = 0; i < nDOF; i++)
            {
                for (var j = 0; j < 12; j++)
                {
                    for (var k = 0; k < nDOF; k++)
                    {
                        pc[i, j] += P[i, k] * CInv[k, j];
                    }
                }
            }

            double[,] PCC =
            {
                {
                    (2 * Math.Pow(x, 3)) / Math.Pow(b, 3) - (3 * Math.Pow(x, 2)) / Math.Pow(b, 2) - (3 * Math.Pow(y, 2)) / Math.Pow(c, 2) + (2 * Math.Pow(y, 3)) / Math.Pow(c, 3) + (3 * x * Math.Pow(y, 2)) / (b * Math.Pow(c, 2)) +
                    (3 * Math.Pow(x, 2) * y) / (Math.Pow(b, 2) * c) - (2 * x * Math.Pow(y, 3)) / (b * Math.Pow(c, 3)) - (2 * Math.Pow(x, 3) * y) / (Math.Pow(b, 3) * c) - (x * y) / (b * c) + 1,
                    x - (2 * Math.Pow(x, 2)) / b + Math.Pow(x, 3) / Math.Pow(b, 2) - (x * y) / c + (2 * Math.Pow(x, 2) * y) / (b * c) - (Math.Pow(x, 3) * y) / (Math.Pow(b, 2) * c),
                    y - (2 * Math.Pow(y, 2)) / c + Math.Pow(y, 3) / Math.Pow(c, 2) - (x * y) / b + (2 * x * Math.Pow(y, 2)) / (b * c) - (x * Math.Pow(y, 3)) / (b * Math.Pow(c, 2)),
                    (3 * Math.Pow(x, 2)) / Math.Pow(b, 2) - (2 * Math.Pow(x, 3)) / Math.Pow(b, 3) - (3 * x * Math.Pow(y, 2)) / (b * Math.Pow(c, 2)) - (3 * Math.Pow(x, 2) * y) / (Math.Pow(b, 2) * c) + (2 * x * Math.Pow(y, 3)) / (b * Math.Pow(c, 3)) +
                    (2 * Math.Pow(x, 3) * y) / (Math.Pow(b, 3) * c) + (x * y) / (b * c),
                    Math.Pow(x, 3) / Math.Pow(b, 2) - Math.Pow(x, 2) / b + (Math.Pow(x, 2) * y) / (b * c) - (Math.Pow(x, 3) * y) / (Math.Pow(b, 2) * c),
                    (x * y) / b - (2 * x * Math.Pow(y, 2)) / (b * c) + (x * Math.Pow(y, 3)) / (b * Math.Pow(c, 2)),
                    (3 * x * Math.Pow(y, 2)) / (b * Math.Pow(c, 2)) + (3 * Math.Pow(x, 2) * y) / (Math.Pow(b, 2) * c) - (2 * x * Math.Pow(y, 3)) / (b * Math.Pow(c, 3)) - (2 * Math.Pow(x, 3) * y) / (Math.Pow(b, 3) * c) - (x * y) / (b * c),
                    (Math.Pow(x, 3) * y) / (Math.Pow(b, 2) * c) - (Math.Pow(x, 2) * y) / (b * c),
                    (x * Math.Pow(y, 3)) / (b * Math.Pow(c, 2)) - (x * Math.Pow(y, 2)) / (b * c),
                    (3 * Math.Pow(y, 2)) / Math.Pow(c, 2) - (2 * Math.Pow(y, 3)) / Math.Pow(c, 3) - (3 * x * Math.Pow(y, 2)) / (b * Math.Pow(c, 2)) - (3 * Math.Pow(x, 2) * y) / (Math.Pow(b, 2) * c) + (2 * x * Math.Pow(y, 3)) / (b * Math.Pow(c, 3)) +
                    (2 * Math.Pow(x, 3) * y) / (Math.Pow(b, 3) * c) + (x * y) / (b * c),
                    (x * y) / c - (2 * Math.Pow(x, 2) * y) / (b * c) + (Math.Pow(x, 3) * y) / (Math.Pow(b, 2) * c),
                    Math.Pow(y, 3) / Math.Pow(c, 2) - Math.Pow(y, 2) / c + (x * Math.Pow(y, 2)) / (b * c) - (x * Math.Pow(y, 3)) / (b * Math.Pow(c, 2))
                },
                {
                    (6 * Math.Pow(x, 2)) / Math.Pow(b, 3) - (6 * x) / Math.Pow(b, 2) - y / (b * c) + (3 * Math.Pow(y, 2)) / (b * Math.Pow(c, 2)) - (2 * Math.Pow(y, 3)) / (b * Math.Pow(c, 3)) - (6 * Math.Pow(x, 2) * y) / (Math.Pow(b, 3) * c) +
                    (6 * x * y) / (Math.Pow(b, 2) * c),
                    (3 * Math.Pow(x, 2)) / Math.Pow(b, 2) - y / c - (4 * x) / b - (3 * Math.Pow(x, 2) * y) / (Math.Pow(b, 2) * c) + (4 * x * y) / (b * c) + 1,
                    -Math.Pow(y, 3) / (b * Math.Pow(c, 2)) + (2 * Math.Pow(y, 2)) / (b * c) - y / b,
                    (6 * x) / Math.Pow(b, 2) - (6 * Math.Pow(x, 2)) / Math.Pow(b, 3) + y / (b * c) - (3 * Math.Pow(y, 2)) / (b * Math.Pow(c, 2)) + (2 * Math.Pow(y, 3)) / (b * Math.Pow(c, 3)) + (6 * Math.Pow(x, 2) * y) / (Math.Pow(b, 3) * c) -
                    (6 * x * y) / (Math.Pow(b, 2) * c),
                    (3 * Math.Pow(x, 2)) / Math.Pow(b, 2) - (2 * x) / b - (3 * Math.Pow(x, 2) * y) / (Math.Pow(b, 2) * c) + (2 * x * y) / (b * c),
                    y / b - (2 * Math.Pow(y, 2)) / (b * c) + Math.Pow(y, 3) / (b * Math.Pow(c, 2)),
                    (3 * Math.Pow(y, 2)) / (b * Math.Pow(c, 2)) - y / (b * c) - (2 * Math.Pow(y, 3)) / (b * Math.Pow(c, 3)) - (6 * Math.Pow(x, 2) * y) / (Math.Pow(b, 3) * c) + (6 * x * y) / (Math.Pow(b, 2) * c),
                    (3 * Math.Pow(x, 2) * y) / (Math.Pow(b, 2) * c) - (2 * x * y) / (b * c),
                    Math.Pow(y, 3) / (b * Math.Pow(c, 2)) - Math.Pow(y, 2) / (b * c),
                    y / (b * c) - (3 * Math.Pow(y, 2)) / (b * Math.Pow(c, 2)) + (2 * Math.Pow(y, 3)) / (b * Math.Pow(c, 3)) + (6 * Math.Pow(x, 2) * y) / (Math.Pow(b, 3) * c) - (6 * x * y) / (Math.Pow(b, 2) * c),
                    y / c + (3 * Math.Pow(x, 2) * y) / (Math.Pow(b, 2) * c) - (4 * x * y) / (b * c),
                    -Math.Pow(y, 3) / (b * Math.Pow(c, 2)) + Math.Pow(y, 2) / (b * c)
                },
                {
                    (6 * Math.Pow(y, 2)) / Math.Pow(c, 3) - (6 * y) / Math.Pow(c, 2) - x / (b * c) + (3 * Math.Pow(x, 2)) / (Math.Pow(b, 2) * c) - (2 * Math.Pow(x, 3)) / (Math.Pow(b, 3) * c) - (6 * x * Math.Pow(y, 2)) / (b * Math.Pow(c, 3)) +
                    (6 * x * y) / (b * Math.Pow(c, 2)),
                    -Math.Pow(x, 3) / (Math.Pow(b, 2) * c) + (2 * Math.Pow(x, 2)) / (b * c) - x / c,
                    (3 * Math.Pow(y, 2)) / Math.Pow(c, 2) - (4 * y) / c - x / b - (3 * x * Math.Pow(y, 2)) / (b * Math.Pow(c, 2)) + (4 * x * y) / (b * c) + 1,
                    x / (b * c) - (3 * Math.Pow(x, 2)) / (Math.Pow(b, 2) * c) + (2 * Math.Pow(x, 3)) / (Math.Pow(b, 3) * c) + (6 * x * Math.Pow(y, 2)) / (b * Math.Pow(c, 3)) - (6 * x * y) / (b * Math.Pow(c, 2)),
                    -Math.Pow(x, 3) / (Math.Pow(b, 2) * c) + Math.Pow(x, 2) / (b * c),
                    x / b + (3 * x * Math.Pow(y, 2)) / (b * Math.Pow(c, 2)) - (4 * x * y) / (b * c),
                    (3 * Math.Pow(x, 2)) / (Math.Pow(b, 2) * c) - x / (b * c) - (2 * Math.Pow(x, 3)) / (Math.Pow(b, 3) * c) - (6 * x * Math.Pow(y, 2)) / (b * Math.Pow(c, 3)) + (6 * x * y) / (b * Math.Pow(c, 2)),
                    Math.Pow(x, 3) / (Math.Pow(b, 2) * c) - Math.Pow(x, 2) / (b * c),
                    (3 * x * Math.Pow(y, 2)) / (b * Math.Pow(c, 2)) - (2 * x * y) / (b * c),
                    (6 * y) / Math.Pow(c, 2) - (6 * Math.Pow(y, 2)) / Math.Pow(c, 3) + x / (b * c) - (3 * Math.Pow(x, 2)) / (Math.Pow(b, 2) * c) + (2 * Math.Pow(x, 3)) / (Math.Pow(b, 3) * c) + (6 * x * Math.Pow(y, 2)) / (b * Math.Pow(c, 3)) -
                    (6 * x * y) / (b * Math.Pow(c, 2)),
                    x / c - (2 * Math.Pow(x, 2)) / (b * c) + Math.Pow(x, 3) / (Math.Pow(b, 2) * c),
                    (3 * Math.Pow(y, 2)) / Math.Pow(c, 2) - (2 * y) / c - (3 * x * Math.Pow(y, 2)) / (b * Math.Pow(c, 2)) + (2 * x * y) / (b * c)
                }
            };

            for (var i = 0; i < nDOF; i++)
            {
                for (var k = 0; k < 12; k++)
                {
                    result[i] += PCC[i, k] * Displacement[k, caseIndex];
                }
            }

            return result;
        }

        public double GetDisplacementApproximate(int CaseIndex, double x, double y)
        {
            double result = 0;

            if ((x < 0 || x > b) || (y < 0 || y > c))
            {
                throw new ArgumentException("Invalid coordinates!");
            }

            double[] shapeFunc;
            switch (NumberOfNodes)
            {
                case 4:
                    shapeFunc = new[]
                    {
                        (x * y) / (b * c) - y / c - x / b + 1,
                        x / b - (x * y) / (b * c),
                        (x * y) / (b * c),
                        y / c - (x * y) / (b * c)
                    };
                    break;
                default:
                    shapeFunc = new[]
                    {
                        (x * y) / (b * c) - y / c - x / b + 1,
                        x / b - (x * y) / (b * c),
                        (x * y) / (b * c),
                        y / c - (x * y) / (b * c)
                    };
                    break;
            }

            for (var i = 0; i < shapeFunc.Length; i++)
            {
                result += shapeFunc[i] * Displacement[i * nDOF, CaseIndex];
            }

            return result;
        }

        public double GetSoilReaction(int caseIndex, double x, double y)
        {
            //var displacement = GetDisplacement(caseIndex, x, y)[0];
            var displacement = GetDisplacementApproximate(caseIndex, x, y);
            return displacement >= 0 ? 0 : displacement * SoilStiffness;
        }

        public double[] GetStressXX(int caseIndex, double x, double y)
        {
            var coeff = E * Math.Pow(Thickness, 3) / (12 * (1 - Math.Pow(Nu, 2)));

            var db = new double[3, 12];
            var result = new double[nDOF];

            db = new[,]
            {
                    {
                        -(6 * coeff * (b - 2 * x) * (c - y)) / (Math.Pow(b, 3) * c) - (6 * coeff * Nu * (b - x) * (c - 2 * y)) / (b * Math.Pow(c, 3)),
                        -(2 * coeff * (c - y) * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * Nu * (b - x) * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)),
                        (6 * coeff * (b - 2 * x) * (c - y)) / (Math.Pow(b, 3) * c) - (6 * coeff * Nu * x * (c - 2 * y)) / (b * Math.Pow(c, 3)),
                        -(2 * coeff * (b - 3 * x) * (c - y)) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * Nu * x * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)),
                        (6 * coeff * y * (b - 2 * x)) / (Math.Pow(b, 3) * c) + (6 * coeff * Nu * x * (c - 2 * y)) / (b * Math.Pow(c, 3)),
                        -(2 * coeff * y * (b - 3 * x)) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * Nu * x * (c - 3 * y)) / (b * Math.Pow(c, 2)),
                        (6 * coeff * Nu * (b - x) * (c - 2 * y)) / (b * Math.Pow(c, 3)) - (6 * coeff * y * (b - 2 * x)) / (Math.Pow(b, 3) * c),
                        -(2 * coeff * y * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * Nu * (b - x) * (c - 3 * y)) / (b * Math.Pow(c, 2))
                    },
                    {
                        -(6 * coeff * (b - x) * (c - 2 * y)) / (b * Math.Pow(c, 3)) - (6 * coeff * Nu * (b - 2 * x) * (c - y)) / (Math.Pow(b, 3) * c),
                        -(2 * coeff * Nu * (c - y) * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * (b - x) * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)),
                        (6 * coeff * Nu * (b - 2 * x) * (c - y)) / (Math.Pow(b, 3) * c) - (6 * coeff * x * (c - 2 * y)) / (b * Math.Pow(c, 3)),
                        -(2 * coeff * Nu * (b - 3 * x) * (c - y)) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * x * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)),
                        (6 * coeff * x * (c - 2 * y)) / (b * Math.Pow(c, 3)) + (6 * coeff * Nu * y * (b - 2 * x)) / (Math.Pow(b, 3) * c),
                        -(2 * coeff * Nu * y * (b - 3 * x)) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * x * (c - 3 * y)) / (b * Math.Pow(c, 2)),
                        (6 * coeff * (b - x) * (c - 2 * y)) / (b * Math.Pow(c, 3)) - (6 * coeff * Nu * y * (b - 2 * x)) / (Math.Pow(b, 3) * c),
                        -(2 * coeff * Nu * y * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * (b - x) * (c - 3 * y)) / (b * Math.Pow(c, 2))
                    },
                    {
                        (2 * coeff * (Nu / 2 - 1 / 2.0) * (Math.Pow(b, 2) * Math.Pow(c, 2) - 6 * Math.Pow(b, 2) * c * y + 6 * Math.Pow(b, 2) * Math.Pow(y, 2) - 6 * b * Math.Pow(c, 2) * x + 6 * Math.Pow(c, 2) * Math.Pow(x, 2))) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                        (2 * coeff * (Nu / 2 - 1 / 2.0) * (Math.Pow(b, 2) - 4 * b * x + 3 * Math.Pow(x, 2))) / (Math.Pow(b, 2) * c),
                        (2 * coeff * (Nu / 2 - 1 / 2.0) * (Math.Pow(c, 2) - 4 * c * y + 3 * Math.Pow(y, 2))) / (b * Math.Pow(c, 2)),
                        -(2 * coeff * (Nu / 2 - 1 / 2.0) * (Math.Pow(b, 2) * Math.Pow(c, 2) - 6 * Math.Pow(b, 2) * c * y + 6 * Math.Pow(b, 2) * Math.Pow(y, 2) - 6 * b * Math.Pow(c, 2) * x + 6 * Math.Pow(c, 2) * Math.Pow(x, 2))) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                        -(2 * coeff * x * (Nu / 2 - 1 / 2.0) * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * (Nu / 2 - 1 / 2.0) * (Math.Pow(c, 2) - 4 * c * y + 3 * Math.Pow(y, 2))) / (b * Math.Pow(c, 2)),
                        (2 * coeff * (Nu / 2 - 1 / 2.0) * (Math.Pow(b, 2) * Math.Pow(c, 2) - 6 * Math.Pow(b, 2) * c * y + 6 * Math.Pow(b, 2) * Math.Pow(y, 2) - 6 * b * Math.Pow(c, 2) * x + 6 * Math.Pow(c, 2) * Math.Pow(x, 2))) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                        (2 * coeff * x * (Nu / 2 - 1 / 2.0) * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c),
                        (2 * coeff * y * (Nu / 2 - 1 / 2.0) * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)),
                        -(2 * coeff * (Nu / 2 - 1 / 2.0) * (Math.Pow(b, 2) * Math.Pow(c, 2) - 6 * Math.Pow(b, 2) * c * y + 6 * Math.Pow(b, 2) * Math.Pow(y, 2) - 6 * b * Math.Pow(c, 2) * x + 6 * Math.Pow(c, 2) * Math.Pow(x, 2))) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                        -(2 * coeff * (Nu / 2 - 1 / 2.0) * (Math.Pow(b, 2) - 4 * b * x + 3 * Math.Pow(x, 2))) / (Math.Pow(b, 2) * c),
                        -(2 * coeff * y * (Nu / 2 - 1 / 2.0) * (2 * c - 3 * y)) / (b * Math.Pow(c, 2))
                    }
                };

            for (var i = 0; i < nDOF; i++)
            {
                for (var k = 0; k < Nodes.Length * nDOF; k++)
                {
                    result[i] += db[i, k] * Displacement[k, caseIndex];
                }
            }

            return result;
        }

        public double GetShearXZ(int caseIndex, double x, double y)
        {
            var coeff = E * Math.Pow(Thickness, 3) / (12 * (1 - Math.Pow(Nu, 2)));

            double[] M =
            {
                    (2 * coeff * (Nu / 2 - 1 / 2.0) * (6 * Math.Pow(b, 2) * c - 12 * Math.Pow(b, 2) * y)) / (Math.Pow(b, 3) * Math.Pow(c, 3)) - (6 * Nu * coeff * (c - 2 * y)) / (b * Math.Pow(c, 3)) - (12 * coeff * (c - y)) / (Math.Pow(b, 3) * c),
                    -(6 * coeff * (c - y)) / (Math.Pow(b, 2) * c),
                    (2 * coeff * (Nu / 2 - 1 / 2.0) * (4 * c - 6 * y)) / (b * Math.Pow(c, 2)) - (2 * Nu * coeff * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)),
                    (12 * coeff * (c - y)) / (Math.Pow(b, 3) * c) + (6 * Nu * coeff * (c - 2 * y)) / (b * Math.Pow(c, 3)) - (2 * coeff * (Nu / 2 - 1 / 2.0) * (6 * Math.Pow(b, 2) * c - 12 * Math.Pow(b, 2) * y)) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                    -(6 * coeff * (c - y)) / (Math.Pow(b, 2) * c),
                    (2 * Nu * coeff * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)) - (2 * coeff * (Nu / 2 - 1 / 2.0) * (4 * c - 6 * y)) / (b * Math.Pow(c, 2)),
                    (12 * coeff * y) / (Math.Pow(b, 3) * c) - (6 * Nu * coeff * (c - 2 * y)) / (b * Math.Pow(c, 3)) + (2 * coeff * (Nu / 2 - 1 / 2.0) * (6 * Math.Pow(b, 2) * c - 12 * Math.Pow(b, 2) * y)) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                    -(6 * coeff * y) / (Math.Pow(b, 2) * c),
                    (2 * Nu * coeff * (c - 3 * y)) / (b * Math.Pow(c, 2)) - (2 * coeff * (Nu / 2 - 1 / 2.0) * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)) + (6 * coeff * y * (Nu / 2 - 1 / 2.0)) / (b * Math.Pow(c, 2)),
                    (6 * Nu * coeff * (c - 2 * y)) / (b * Math.Pow(c, 3)) - (12 * coeff * y) / (Math.Pow(b, 3) * c) - (2 * coeff * (Nu / 2 - 1 / 2.0) * (6 * Math.Pow(b, 2) * c - 12 * Math.Pow(b, 2) * y)) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                    -(6 * coeff * y) / (Math.Pow(b, 2) * c),
                    (2 * coeff * (Nu / 2 - 1 / 2.0) * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)) - (2 * Nu * coeff * (c - 3 * y)) / (b * Math.Pow(c, 2)) - (6 * coeff * y * (Nu / 2 - 1 / 2.0)) / (b * Math.Pow(c, 2))
                };

            double[,] MM =
            {
                    {
                        (2 * coeff * (Nu / 2 - 1 / 2.0) * (6 * Math.Pow(b, 2) * c - 12 * Math.Pow(b, 2) * y)) / (Math.Pow(b, 3) * Math.Pow(c, 3)) - (6 * Nu * coeff * (c - 2 * y)) / (b * Math.Pow(c, 3)) - (12 * coeff * (c - y)) / (Math.Pow(b, 3) * c),
                        -(6 * coeff * (c - y)) / (Math.Pow(b, 2) * c),
                        (2 * coeff * (Nu / 2 - 1 / 2.0) * (4 * c - 6 * y)) / (b * Math.Pow(c, 2)) - (2 * Nu * coeff * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)),
                        (12 * coeff * (c - y)) / (Math.Pow(b, 3) * c) + (6 * Nu * coeff * (c - 2 * y)) / (b * Math.Pow(c, 3)) - (2 * coeff * (Nu / 2 - 1 / 2.0) * (6 * Math.Pow(b, 2) * c - 12 * Math.Pow(b, 2) * y)) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                        -(6 * coeff * (c - y)) / (Math.Pow(b, 2) * c),
                        (2 * Nu * coeff * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)) - (2 * coeff * (Nu / 2 - 1 / 2.0) * (4 * c - 6 * y)) / (b * Math.Pow(c, 2)),
                        (12 * coeff * y) / (Math.Pow(b, 3) * c) - (6 * Nu * coeff * (c - 2 * y)) / (b * Math.Pow(c, 3)) + (2 * coeff * (Nu / 2 - 1 / 2.0) * (6 * Math.Pow(b, 2) * c - 12 * Math.Pow(b, 2) * y)) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                        -(6 * coeff * y) / (Math.Pow(b, 2) * c),
                        (2 * Nu * coeff * (c - 3 * y)) / (b * Math.Pow(c, 2)) - (2 * coeff * (Nu / 2 - 1 / 2.0) * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)) + (6 * coeff * y * (Nu / 2 - 1 / 2.0)) / (b * Math.Pow(c, 2)),
                        (6 * Nu * coeff * (c - 2 * y)) / (b * Math.Pow(c, 3)) - (12 * coeff * y) / (Math.Pow(b, 3) * c) - (2 * coeff * (Nu / 2 - 1 / 2.0) * (6 * Math.Pow(b, 2) * c - 12 * Math.Pow(b, 2) * y)) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                        -(6 * coeff * y) / (Math.Pow(b, 2) * c),
                        (2 * coeff * (Nu / 2 - 1 / 2.0) * (2 * c - 3 * y)) / (b * Math.Pow(c, 2)) - (2 * Nu * coeff * (c - 3 * y)) / (b * Math.Pow(c, 2)) - (6 * coeff * y * (Nu / 2 - 1 / 2.0)) / (b * Math.Pow(c, 2))
                    },
                    {
                        (2 * coeff * (Nu / 2 - 1 / 2.0) * (6 * b * Math.Pow(c, 2) - 12 * Math.Pow(c, 2) * x)) / (Math.Pow(b, 3) * Math.Pow(c, 3)) - (6 * Nu * coeff * (b - 2 * x)) / (Math.Pow(b, 3) * c) - (12 * coeff * (b - x)) / (b * Math.Pow(c, 3)),
                        (2 * coeff * (Nu / 2 - 1 / 2.0) * (4 * b - 6 * x)) / (Math.Pow(b, 2) * c) - (2 * Nu * coeff * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c),
                        -(6 * coeff * (b - x)) / (b * Math.Pow(c, 2)),
                        (6 * Nu * coeff * (b - 2 * x)) / (Math.Pow(b, 3) * c) - (12 * coeff * x) / (b * Math.Pow(c, 3)) - (2 * coeff * (Nu / 2 - 1 / 2.0) * (6 * b * Math.Pow(c, 2) - 12 * Math.Pow(c, 2) * x)) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                        (2 * coeff * (Nu / 2 - 1 / 2.0) * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c) - (2 * Nu * coeff * (b - 3 * x)) / (Math.Pow(b, 2) * c) - (6 * coeff * x * (Nu / 2 - 1 / 2.0)) / (Math.Pow(b, 2) * c),
                        -(6 * coeff * x) / (b * Math.Pow(c, 2)),
                        (12 * coeff * x) / (b * Math.Pow(c, 3)) - (6 * Nu * coeff * (b - 2 * x)) / (Math.Pow(b, 3) * c) + (2 * coeff * (Nu / 2 - 1 / 2.0) * (6 * b * Math.Pow(c, 2) - 12 * Math.Pow(c, 2) * x)) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                        (2 * Nu * coeff * (b - 3 * x)) / (Math.Pow(b, 2) * c) - (2 * coeff * (Nu / 2 - 1 / 2.0) * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c) + (6 * coeff * x * (Nu / 2 - 1 / 2.0)) / (Math.Pow(b, 2) * c),
                        -(6 * coeff * x) / (b * Math.Pow(c, 2)),
                        (12 * coeff * (b - x)) / (b * Math.Pow(c, 3)) + (6 * Nu * coeff * (b - 2 * x)) / (Math.Pow(b, 3) * c) - (2 * coeff * (Nu / 2 - 1 / 2.0) * (6 * b * Math.Pow(c, 2) - 12 * Math.Pow(c, 2) * x)) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                        (2 * Nu * coeff * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c) - (2 * coeff * (Nu / 2 - 1 / 2.0) * (4 * b - 6 * x)) / (Math.Pow(b, 2) * c),
                        -(6 * coeff * (b - x)) / (b * Math.Pow(c, 2))
                    }
                };

            //M = {(6 * Coeff * (-2 * c ^ 3 + 2 * y * c ^ 2)) / (b ^ 2 * c ^ 3) - (6 * Coeff * (c - 2 * y)) / c ^ 3, -(6 * Coeff * (c - y)) / (b * c), -(2 * Coeff * (2 * c - 3 * y)) / c ^ 2, (6 * Coeff * (c - 2 * y)) / c ^ 3 - (6 * Coeff * (-2 * c ^ 3 + 2 * y * c ^ 2)) / (b ^ 2 * c ^ 3), -(6 * Coeff * (c - y)) / (b * c), (2 * Coeff * (2 * c - 3 * y)) / c ^ 2, (12 * Coeff * y) / (b ^ 2 * c) - (6 * Coeff * (c - 2 * y)) / c ^ 3, -(6 * Coeff * y) / (b * c), (2 * Coeff * (c - 3 * y)) / c ^ 2, (6 * Coeff * (c - 2 * y)) / c ^ 3 - (12 * Coeff * y) / (b ^ 2 * c), -(6 * Coeff * y) / (b * c), -(2 * Coeff * (c - 3 * y)) / c ^ 2}
            //M = {-(6 * c * Coeff) / b ^ 3, -(3 * c * Coeff) / b ^ 2, -Coeff / b, (6 * c * Coeff) / b ^ 3, -(3 * c * Coeff) / b ^ 2, Coeff / b, (6 * c * Coeff) / b ^ 3, -(3 * c * Coeff) / b ^ 2, -Coeff / b, -(6 * c * Coeff) / b ^ 3, -(3 * c * Coeff) / b ^ 2, Coeff / b}

            var value = 0.0;
            for (var i = 0; i < Nodes.Length * nDOF; i++)
            {
                value += M[i] * Displacement[i, caseIndex];
            }

            return value;
        }

        public double GetShearYZ(int caseIndex, double x, double y)
        {
            var coeff = E * Math.Pow(Thickness, 3) / (12 * (1 - Math.Pow(Nu, 2)));

            double[] M =
            {
                    (2 * coeff * (Nu / 2 - 1 / 2.0) * (6 * b * Math.Pow(c, 2) - 12 * Math.Pow(c, 2) * x)) / (Math.Pow(b, 3) * Math.Pow(c, 3)) - (6 * Nu * coeff * (b - 2 * x)) / (Math.Pow(b, 3) * c) - (12 * coeff * (b - x)) / (b * Math.Pow(c, 3)),
                    (2 * coeff * (Nu / 2 - 1 / 2.0) * (4 * b - 6 * x)) / (Math.Pow(b, 2) * c) - (2 * Nu * coeff * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c),
                    -(6 * coeff * (b - x)) / (b * Math.Pow(c, 2)),
                    (6 * Nu * coeff * (b - 2 * x)) / (Math.Pow(b, 3) * c) - (12 * coeff * x) / (b * Math.Pow(c, 3)) - (2 * coeff * (Nu / 2 - 1 / 2.0) * (6 * b * Math.Pow(c, 2) - 12 * Math.Pow(c, 2) * x)) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                    (2 * coeff * (Nu / 2 - 1 / 2.0) * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c) - (2 * Nu * coeff * (b - 3 * x)) / (Math.Pow(b, 2) * c) - (6 * coeff * x * (Nu / 2 - 1 / 2.0)) / (Math.Pow(b, 2) * c),
                    -(6 * coeff * x) / (b * Math.Pow(c, 2)),
                    (12 * coeff * x) / (b * Math.Pow(c, 3)) - (6 * Nu * coeff * (b - 2 * x)) / (Math.Pow(b, 3) * c) + (2 * coeff * (Nu / 2 - 1 / 2.0) * (6 * b * Math.Pow(c, 2) - 12 * Math.Pow(c, 2) * x)) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                    (2 * Nu * coeff * (b - 3 * x)) / (Math.Pow(b, 2) * c) - (2 * coeff * (Nu / 2 - 1 / 2.0) * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c) + (6 * coeff * x * (Nu / 2 - 1 / 2.0)) / (Math.Pow(b, 2) * c),
                    -(6 * coeff * x) / (b * Math.Pow(c, 2)),
                    (12 * coeff * (b - x)) / (b * Math.Pow(c, 3)) + (6 * Nu * coeff * (b - 2 * x)) / (Math.Pow(b, 3) * c) - (2 * coeff * (Nu / 2 - 1 / 2.0) * (6 * b * Math.Pow(c, 2) - 12 * Math.Pow(c, 2) * x)) / (Math.Pow(b, 3) * Math.Pow(c, 3)),
                    (2 * Nu * coeff * (2 * b - 3 * x)) / (Math.Pow(b, 2) * c) - (2 * coeff * (Nu / 2 - 1 / 2.0) * (4 * b - 6 * x)) / (Math.Pow(b, 2) * c),
                    -(6 * coeff * (b - x)) / (b * Math.Pow(c, 2))
                };

            var value = 0.0;
            for (var i = 0; i < Nodes.Length * nDOF; i++)
            {
                value += M[i] * Displacement[i, caseIndex];
            }

            return value;
        }
    }
}
