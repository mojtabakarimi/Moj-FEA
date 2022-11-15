//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using FrameAnalysis.Common.Mathematics;
//using FrameAnalysis.LoadDefinition;
//using MathNet.Numerics.LinearAlgebra;

//namespace FrameAnalysis.Plate.Objects
//{
//    public class SAP
//    {
//        #region Variables

//        private int MeshGenerationRound { get; } = 0;

//        //Objects
//        private List<NodeObject> PointObject;
//        private readonly List<AreaObjects> AreaObject;
//        private readonly List<Material> MaterialList;
//        private readonly List<Soil> SoilObject;
//        private readonly List<LoadPattern> LoadPatternObject;
//        private readonly List<LoadCases> LoadCaseObject;
//        private readonly List<LoadCombinations> LoadCombinationObject;

//        private readonly List<AreaLoad> AreaLoadObject = new List<AreaLoad>();
//        private readonly List<PointLoads> PointLoadObject = new List<PointLoads>();

//        //Elements

//        public int nDOF { get; }



//        #endregion

//        #region Constructors
//        public SAP(int meshGenerationRound)
//        {
//            nDOF = 3;
//            SetGaussIntergration(4);

//            PointObject = new List<NodeObject>();
//            AreaObject = new List<AreaObjects>();
//            MaterialList = new List<Material>();

//            SoilObject = new List<Soil>();
//            LoadPatternObject = new List<LoadPattern>();
//            LoadCaseObject = new List<LoadCases>();

//            LoadCombinationObject = new List<LoadCombinations>();

//            //NodeElementCoordDic = new Dictionary<PointCoordinates, int>();
//            //NodeObjectCoordDic = new Dictionary<PointCoordinates, string>();
//        }

//        #endregion

//        #region Property
//        #region Private Variables
        

//        #endregion

//        public SolverTypes SolverType { get; } = SolverTypes.Default;

//        private double maxMeshSize = 500;                  //mm
//        public double MaxMeshSize
//        {
//            get => maxMeshSize;
//            set
//            {
//                if (value <= 0)
//                {
//                    throw new ArgumentException("Mesh size must be positive");
//                }
//                maxMeshSize = value;
//            }
//        }

//        public int NodeObjectCount => PointObject.Count;

//        public int NumberOfNodes { get; private set; }

//        public int NumberOfElements { get; private set; }

//        public int NumberOfLoadPatterns { get; private set; }

//        public int NumberOfLoadCases { get; private set; }

//        public int NumberOfLoadCombos { get; private set; }

//        #endregion

//        #region Creating Model
//        public void AddMaterial(string name, double weight, double modulusOfElasticity, double poisonRatio)
//        {
//            if (!(weight >= 0 && modulusOfElasticity > 0 && poisonRatio > 0 && poisonRatio <= 0.5))
//            {
//                throw new ArgumentException("??");
//            }

//            MaterialList.Add(new Material(name, weight, modulusOfElasticity, poisonRatio));
//        }

//        public void AddSoil(string name, double springStiffness)
//        {
//            if (springStiffness <= 0)
//            {
//                throw new ArgumentException("??");
//            }

//            SoilObject.Add(new Soil(name, springStiffness));
//        }

//        public void AddPoint(string name, double x, double y)
//        {
//            //todo: check name duplication
//            //todo: check coordination duplication

//            PointObject.Add(new NodeObject(name, Math.Round(x, MeshGenerationRound), Math.Round(y, MeshGenerationRound))
//            {
//                Displacement = new double[] { 0, 0, 0 },
//                BoundaryCondition = new[] { false, false, false },
//                Connectivity = new List<int>()
//            });
//        }

//        public void AddArea(string name, string materialName, string soilName, double thickness, string[] nodes)
//        {
//            if (nodes.Length != 4)
//            {
//                throw new Exception("Area should be 4 points!");
//            }

//            var points = new List<NodeObject>();
//            foreach (var node in nodes)
//            {
//                var nodeIndex = PointObject.FindIndex(q => q.Name == node);
//                if (nodeIndex == -1)
//                {
//                    throw new Exception($"Invalid node name {node}");
//                }
//                points.Add(PointObject[nodeIndex]);
//            }

//            var materialIndex = MaterialList.FindIndex(p => p.Name == materialName);
//            if (materialIndex == -1)
//            {
//                throw new ArgumentException("??");
//            }

//            var soilIndex = SoilObject.FindIndex(p => p.Name == soilName);
//            if (soilIndex == -1)
//            {
//                throw new ArgumentException("Unknown soil name");
//            }

//            if (thickness <= 0)
//            {
//                throw new ArgumentException("Area thickness shall be a positive value");
//            }


//            var pointList = points.Select(p => new PointCoordinates(p.X, p.Y)).ToList();
//            if (!MathAdv.IsPolygonConvex(pointList))
//            {
//                throw new Exception($"Area is not convex!");//\r\n{string.Join("\r\n", pointList.Select(t => $"{t.X}\t{t.Y}"))}");
//            }

//            if (!(points[0].Coord[0] == points[3].Coord[0]
//                  || points[1].Coord[0] == points[2].Coord[0]
//                  || points[0].Coord[1] == points[1].Coord[1]
//                  || points[2].Coord[1] == points[3].Coord[1]))
//            {
//                throw new Exception("Area is not rectangle!");
//            }

//            AreaObject.Add(new AreaObjects(name, materialName, soilName, thickness, nodes));
//        }

//        public void AddAreaLoad(string patternName, string[] areaName, double verticalLoad)
//        {
//            for (var i1 = 0; i1 < areaName.Length; i1++)
//            {
//                if (!AreaObject.Keys.Contains(areaName[i1]))
//                {
//                    throw new ArgumentException($"Unknown Area Name \"{areaName[i1]}\"");
//                }
//            }

//            if (!LoadPatternObject.Keys.Contains(patternName))
//            {
//                throw new ArgumentException("Unknown LoadCase Name \"" + patternName + "\"");
//            }

//            for (var i1 = 0; i1 < areaName.Length; i1++)
//            {
//                AreaLoadObject.Add(new AreaLoad(patternName, areaName[i1], verticalLoad));
//            }
//        }

//        public void AddPointLoad(string patternName, string[] pointNames, double Fz, double Mx, double My)
//        {
//            for (var i1 = 0; i1 < pointNames.Length; i1++)
//            {
//                if (!PointObject.Keys.Contains(pointNames[i1]))
//                {
//                    throw new ArgumentException("Unknown Point Name \"" + pointNames[i1] + "\"");
//                }
//            }

//            if (!LoadPatternObject.Keys.Contains(patternName))
//            {
//                throw new ArgumentException("Unknown LoadCase Name \"" + patternName + "\"");
//            }

//            for (var i1 = 0; i1 < pointNames.Length; i1++)
//            {
//                PointLoadObject.Add(new PointLoads(patternName, pointNames[i1], Fz, Mx, My));
//            }
//        }

//        public void AddSupport(string[] pointNames, bool Uz, bool Rx, bool Ry)
//        {
//            for (var i1 = 0; i1 < pointNames.Length; i1++)
//            {
//                if (!PointObject.Keys.Contains(pointNames[i1]))
//                {
//                    throw new ArgumentException("??");
//                }
//            }

//            for (var i1 = 0; i1 < pointNames.Length; i1++)
//            {
//                NodeObject n;

//                n = PointObject[pointNames[i1]];
//                n.BoundaryCondition = new[] { Uz, Rx, Ry };

//                PointObject[pointNames[i1]] = n;
//            }
//        }

//        public void AddPointSpring(string[] pointNames, double Ks)
//        {
//            var points = new int[pointNames.Length];
//            for (var i = 0; i < pointNames.Length; i++)
//            {
//                points[i] = PointObject.FindIndex(p => p.Name == pointNames[i]);

//                if (points[i] == -1)
//                {
//                    throw new ArgumentException("??");
//                }
//            }

//            for (var i = 0; i < pointNames.Length; i++)
//            {
//                PointObject[points[i]].SpringStiffness = Ks;
//            }
//        }

//        public void AddLoadPatten(string patternName, bool selfWeight)
//        {
//            LoadPatternObject.Add(new LoadPattern(patternName, selfWeight));
//        }

//        public void AddLoadCase(string caseName, AnalysisTypes analysisType, decimal maximumNormalError = 0.000001m, int maximumIteratinCount = 200)
//        {
//            LoadCases loadCase;

//            switch (analysisType)
//            {
//                case AnalysisTypes.Linear:
//                    loadCase = new LoadCases(caseName, AnalysisTypes.Linear, 0, 1);
//                    break;
//                case AnalysisTypes.Nonlinear:
//                    loadCase = new LoadCases(caseName, AnalysisTypes.Nonlinear, maximumNormalError, maximumIteratinCount);
//                    break;
//                default:
//                    return;
//            }

//            LoadCaseObject.Add(loadCase);
//        }

//        public void AddLoadCasePattern(string caseName, string patternName, double patternCoeff)
//        {
//            LoadCases LC;

//            LC = LoadCaseObject[caseName];

//            if (!LoadPatternObject.Keys.Contains(patternName))
//            {
//                throw new ArgumentException("Pattern eeror!");
//            }

//            LC.LoadPatternNames.Add(patternName);
//            LC.LoadPatternCoeff.Add(patternCoeff);

//            LoadCaseObject[caseName] = LC;
//        }

//        public void AddLoadCombination(string name, string loadCaseName, double loadCaseCoeff, int loadCombinationType = 0)
//        {
//            LoadCombinationObject.Add(new LoadCombinations(name));
//        }

//        #endregion


//        #region Analysis

//        public void RunAnalysis()
//        {
//            GenerateElements(nDOF);
//            GenerateLoads();
//        }

//        private void GenerateElements(int nDof)
//        {
//            var initialNodeIndex = 0;

//            var pointElementList = new List<Node>();
//            var areaElementList = new List<Element>();

//            LoadPatternElement = LoadPatternObject.ToArray();
//            NumberOfLoadPatterns = LoadPatternElement.Length;

//            LoadCaseElement = LoadCaseObject.ToArray();
//            NumberOfLoadCases = LoadCaseElement.Length;

//            LoadCombinationElement = LoadCombinationObject.ToArray();
//            NumberOfLoadCombos = LoadCombinationObject.Count;

//            //foreach (PointObjects NodeObj in PointObject.Values)
//            //    NodeObjectCoordDic.Add(new PointCoordinates(Math.Round(NodeObj.CoordX, MeshGenerationRound), Math.Round(NodeObj.CoordY ,MeshGenerationRound)), NodeObj.Name);

//            var nodeIndexer = new List<int>();
//            foreach (var AreaObj in AreaObject)
//            {
//                var lx = PointObject[AreaObj.Nodes[1]].X - PointObject[AreaObj.Nodes[0]].X;
//                var ly = PointObject[AreaObj.Nodes[3]].Y - PointObject[AreaObj.Nodes[0]].Y;

//                var nx = Convert.ToInt32(Math.Ceiling(lx / maxMeshSize));
//                var ny = Convert.ToInt32(Math.Ceiling(ly / maxMeshSize));

//                var dx = lx / nx;
//                var dy = ly / ny;

//                var InitialX = PointObject[AreaObj.Nodes[0]].X;
//                var InitialY = PointObject[AreaObj.Nodes[0]].Y;

//                for (var i = 0; i <= nx; i++)
//                {
//                    for (var j = 0; j <= ny; j++)
//                    {
//                        var n = new Node(nDof, NumberOfLoadPatterns, NumberOfLoadCases)
//                        {
//                            Coord = ImmutableArray.Create(Math.Round(InitialX + i * dx, MeshGenerationRound), Math.Round(InitialY + j * dy, MeshGenerationRound))
//                        };

//                        var ii = pointElementList.FindIndex(u => u.Coord.Equals(n.Coord));

//                        if (ii != -1)
//                        {
//                            nodeIndexer.Add(ii);
//                        }
//                        else
//                        {
//                            nodeIndexer.Add(pointElementList.Count);
//                            //NodeElementCoordDic.Add(n.Coord, PointElementList.Count);

//                            if ((i == 0 || i == nx) && (j == 0 || j == ny))
//                            {
//                                var ok = PointObject.First(u => u.X == n.Coord[0] && u.Y == n.Coord[1]);
//                                n.Name = ok.Name;

//                                //n.ObjectName = NodeObjectCoordDic[n.Coord];

//                                //n.ObjectName = NodeElementList.Count.ToString();

//                                n.BoundaryCondition = PointObject[n.Name].BoundaryCondition;
//                            }
//                            else
//                            {
//                                n.Name = "";
//                            }

//                            pointElementList.Add(n);
//                        }
//                    }
//                }

//                for (var i = 0; i < nx; i++)
//                {
//                    for (var j = 0; j < ny; j++)
//                    {
//                        var j1 = initialNodeIndex + i * (ny + 1) + j;
//                        var j2 = initialNodeIndex + (i + 1) * (ny + 1) + j;
//                        var j3 = initialNodeIndex + (i + 1) * (ny + 1) + j + 1;
//                        var j4 = initialNodeIndex + i * (ny + 1) + j + 1;

//                        j1 = nodeIndexer[j1];
//                        j2 = nodeIndexer[j2];
//                        j3 = nodeIndexer[j3];
//                        j4 = nodeIndexer[j4];

//                        var e = new Element(4, nDof, NumberOfLoadPatterns, NumberOfLoadCases)
//                        {
//                            Elements = new[] { j1, j2, j3, j4 }
//                        };

//                        e.b = (double)(pointElementList[e.Elements[1]].Coord[0] - pointElementList[e.Elements[0]].Coord[0]);
//                        e.c = (double)(pointElementList[e.Elements[3]].Coord[1] - pointElementList[e.Elements[0]].Coord[1]);

//                        e.X0 = (double)pointElementList[e.Elements[0]].Coord[0];
//                        e.Y0 = (double)pointElementList[e.Elements[0]].Coord[1];

//                        e.SoilStiffness = AreaObj.SoilName != null
//                            ? SoilObject[AreaObj.SoilName].SpringStiffness
//                            : 0;

//                        e.Thickness = AreaObj.Thickness;
//                        e.E = MaterialList[AreaObj.MaterialName].ModulusOfElasticity;
//                        e.Nu = MaterialList[AreaObj.MaterialName].nu;

//                        e.ParrentObjectName = AreaObj.Name;

//                        areaElementList.Add(e);

//                        pointElementList[j1].Connectivity.Add(areaElementList.Count);
//                        pointElementList[j2].Connectivity.Add(areaElementList.Count);
//                        pointElementList[j3].Connectivity.Add(areaElementList.Count);
//                        pointElementList[j4].Connectivity.Add(areaElementList.Count);
//                    }
//                }

//                initialNodeIndex = nodeIndexer.Count;
//            }

//            AreaElement = areaElementList.ToArray();
//            PointElement = pointElementList.ToArray();
//        }

//        private void GenerateLoads()
//        {
//            for (var i = 0; i < PointLoadObject.Count; i++)
//            {
//                for (var j = 0; j < PointElement.Length; j++)
//                {
//                    if (PointElement[j].Name == PointLoadObject[i].PointName)
//                    {
//                        var PatternIndex = LoadPatternElement.ToList().FindIndex(p => p.Name == PointLoadObject[i].PatternName);

//                        if (PatternIndex == -1)
//                        {
//                            throw new Exception("Bad Pattern name!");
//                        }

//                        for (var k = 0; k < nDOF; k++)
//                        {
//                            PointElement[j].Load[k, PatternIndex] += PointLoadObject[i].Value[k];
//                        }
//                    }
//                }
//            }

//            for (var i1 = 0; i1 < AreaLoadObject.Count; i1++)
//            {
//                for (var i2 = 0; i2 < AreaElement.Length; i2++)
//                {
//                    if (AreaElement[i2].ParrentObjectName == AreaLoadObject[i1].AreaName)
//                    {
//                        var loadPatternIndex = LoadPatternElement.ToList().FindIndex(p => p.Name == AreaLoadObject[i1].PatternName);

//                        if (loadPatternIndex == -1)
//                        {
//                            throw new Exception("Bad Pattern name!");
//                        }

//                        AreaElement[i2].Load[loadPatternIndex] += AreaLoadObject[i1].Value;
//                    }
//                }
//            }

//            for (var i = 0; i < LoadPatternElement.Length; i++)
//            {
//                if (LoadPatternElement[i].SelftWeight)
//                {
//                    for (var j = 0; j < AreaElement.Length; j++)
//                    {
//                        AreaElement[j].Load[i] -= AreaElement[j].Thickness * MaterialList[AreaObject[AreaElement[j].ParrentObjectName].MaterialName].Weight;
//                    }
//                }
//            }
//        }
//        #endregion

//    }
//}
