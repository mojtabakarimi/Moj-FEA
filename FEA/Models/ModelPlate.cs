using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FEA.AreaElements;
using FEA.Common.Mathematics;
using FEA.Force;
using FEA.LoadCases;
using FEA.LoadDefinition;
using FEA.Materials;
using FEA.Nodes;
using FEA.Plate;
using MathNet.Numerics.LinearAlgebra;

namespace FEA.Models
{
    public class ModelPlate : Model
    {

        public new List<PlateNode> Nodes;
        private List<Soil> Soils;
        private List<AreaLoad> AreaLoads;

        private double[,] InitialStiffness;
        private double[,] MainStiffness;
        private double[,] Force;
        private double[,] Displacement;
        private double[,] Reaction;

        //Secondary
        private double[] GaussPoints;
        private double[] GaussWeight;

        public int NumberOfNodes { get; private set; }

        public int NumberOfElements { get; private set; }

        //public int NumberOfLoadCases { get; private set; }

        public IList<double> PointCoordX, PointCoordY;
        public IList<double> GlobalUpliftRatio { get; private set; }
        public IList<double> MaximumSoilPressurePerArea, MinimumSoilPressurePerArea;
        public IList<double> MaximumDisplacement, MinimumDisplacement;

        public int[][] NodeConnectivity;

        public ModelPlate()
            : base(2, 3)
        {
            Nodes = new List<PlateNode>();
            Soils = new List<Soil>();
            AreaLoads = new List<AreaLoad>();
        }

        #region Creating Model

        public void AddMaterial(string name, double modulusOfElasticity, double poisonRatio)
        {
            if (modulusOfElasticity < 0)
            {
                throw new ArgumentException("??");
            }

            if (poisonRatio < 0 || poisonRatio > 0.5)
            {
                throw new ArgumentException("??");
            }

            if (Materials.Any(p => p.Name == name))
            {
                throw new ArgumentException($"duplicate material name: {name}");
            }

            Materials.Add(new AreaMaterial(name, modulusOfElasticity, poisonRatio));
        }

        public void AddSoil(string name, double springStiffness)
        {
            if (springStiffness <= 0)
            {
                throw new ArgumentException("??");
            }

            if (Soils.Any(p => p.Name == name))
            {
                throw new ArgumentException($"duplicate soil name: {name}");
            }

            Soils.Add(new Soil(name, springStiffness));
        }

        public void AddNode(string name, double x, double y)
        {
            //todo: check coordination duplication

            if (Nodes.Any(p => p.Name == name))
            {
                throw new ArgumentException($"duplicate node name: {name}");
            }

            Nodes.Add(new PlateNode(name, x, y));
        }

        public void AddArea(string name, string materialName, double thickness, string node1, string node2, string node3, string node4)
        {
            var nodes = new[] { node1, node2, node3, node4 };

            var n1 = Nodes.SingleOrDefault(p => p.Name == node1);

            var nodeIndices = new int[4];
            for (var i = 0; i < nodes.Length; i++)
            {
                var nodeIndex = Nodes.FindIndex(q => q.Name == nodes[i]);
                if (nodeIndex == -1)
                {
                    throw new Exception($"Invalid node name {nodes[i]}");
                }

                nodeIndices[i] = nodeIndex;
            }

            var material = Materials.SingleOrDefault(p => p.Name == materialName) as AreaMaterial;
            if (material == null)
            {
                throw new ArgumentException("??");
            }

            if (thickness <= 0)
            {
                throw new ArgumentException("Area thickness shall be a positive value");
            }

            var pointList = nodeIndices.Select(p => new Point(Nodes[p].X, Nodes[p].Y)).ToList();
            if (!MathAdv.IsPolygonConvex(pointList))
            {
                throw new Exception($"Area is not convex!"); //\r\n{string.Join("\r\n", pointList.Select(t => $"{t.X}\t{t.Y}"))}");
            }

            if (!(Nodes[nodeIndices[0]].Coord[0] == Nodes[nodeIndices[3]].Coord[0]
                  || Nodes[nodeIndices[1]].Coord[0] == Nodes[nodeIndices[2]].Coord[0]
                  || Nodes[nodeIndices[0]].Coord[1] == Nodes[nodeIndices[1]].Coord[1]
                  || Nodes[nodeIndices[2]].Coord[1] == Nodes[nodeIndices[3]].Coord[1]))
            {
                throw new Exception("Area is not rectangle!");
            }

            var b = Nodes[nodeIndices[1]].X - Nodes[nodeIndices[0]].X;
            var c = Nodes[nodeIndices[2]].Y - Nodes[nodeIndices[1]].Y;

            AreaElements.Add(new AreaElement(name, thickness, material.ModulusOfElasticity, material.Nu, nodeIndices, b, c));
        }

        public void AddAreaLoad(string areaName, string patternName, double verticalLoad)
        {
            var areaIndex = AreaElements.FindIndex(p => p.Name == areaName);
            var loadCaseIndex = LoadCases.FindIndex(p => p.Name == patternName);

            AreaLoads.Add(new AreaLoad(areaIndex, loadCaseIndex, verticalLoad));
        }

        public void AddNodeLoad(string patternName, string nodeName, double Fz, double Mx, double My)
        {
            var nodeIndex = Nodes.FindIndex(p => p.Name == nodeName);
            if (nodeIndex == -1)
            {
                throw new ArgumentException("Name does not exist");
            }

            var loadCaseIndex = LoadCases.FindIndex(p => p.Name == patternName);
            if (loadCaseIndex == -1)
            {
                throw new ArgumentException($"load case \"{patternName}\" does not exist");
            }

            NodalForces.Add(new NodalForce(nodeIndex, loadCaseIndex, new[] { Fz, Mx, My }));
        }

        public void AddRestraint(string nodeName, bool Uz, bool Rx, bool Ry)
        {
            var index = Nodes.FindIndex(p => p.Name == nodeName);
            if (index == -1)
            {
                throw new ArgumentException($"Node name {nodeName} is not exist");
            }

            Nodes[index].IsSupport = new[] { Uz, Rx, Ry };
        }

        public void AddPointSpring(string[] pointNames, double Ks)
        {
            throw new NotImplementedException();
        }

        public void AddLoadPatten(string patternName, bool selfWeight)
        {
            throw new NotImplementedException();
        }

        public void AddLoadCase(string loadCaseName, AnalysisTypes analysisType, decimal maximumNormalError = 0.000001m, int maximumIteratinCount = 200)
        {
            if (LoadCases.Any(p => p.Name == loadCaseName))
            {
                throw new ArgumentException("Duplicated load case name!");
            }

            LoadCases.Add(new LoadCase(loadCaseName));
        }

        public void AddLoadCasePattern(string caseName, string patternName, double patternCoeff)
        {
            throw new NotImplementedException();
        }

        public void AddLoadCombination(string name, string loadCaseName, double loadCaseCoeff, int loadCombinationType = 0)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void InitialStuff()
        {
            const double TOLERANCE = 0.0000000001;

            NumberOfNodes = Nodes.Count;
            NumberOfElements = AreaElements.Count;

            Force = new double[NumberOfNodes * nDOF, LoadCases.Count];
            Displacement = new double[NumberOfNodes * nDOF, LoadCases.Count];
            Reaction = new double[NumberOfNodes * nDOF, LoadCases.Count];

            InitialStiffness = new double[NumberOfNodes * nDOF, NumberOfNodes * nDOF];
            MainStiffness = new double[NumberOfNodes * nDOF, NumberOfNodes * nDOF];

            var nodeConnectivity = new List<int>[Nodes.Count];
            for (var i = 0; i < Nodes.Count; i++)
            {
                nodeConnectivity[i] = new List<int>();
            }
            for (var i = 0; i < AreaElements.Count; i++)
            {
                foreach (var node in AreaElements[i].Nodes)
                {
                    nodeConnectivity[node].Add(i);
                }
            }

            NodeConnectivity = nodeConnectivity
                .Select(p => p.ToArray())
                .ToArray();

            for (var i = 0; i < NumberOfElements; i++)
            {
                for (var j = 0; j < AreaElements[i].NumberOfNodes; j++)
                {
                    if (Math.Abs(AreaElements[i].SoilStiffness) > TOLERANCE)
                    {
                        Nodes[AreaElements[i].Nodes[j]].AreaSpringStiffness += AreaElements[i].SoilStiffness * AreaElements[i].Area / AreaElements[i].NumberOfNodes;
                        Nodes[AreaElements[i].Nodes[j]].SpringStiffness += AreaElements[i].SoilStiffness;
                    }
                }
            }

            PointCoordX = Nodes
                .Select(p => p.X)
                .Distinct()
                .ToList()
                .AsReadOnly()
                .ToList();

            PointCoordY = Nodes
                .Select(p => p.Y)
                .Distinct()
                .ToList()
                .AsReadOnly()
                .ToList();
        }

        #region Post Analysis

        #region "Stress"
        public double? GetGlobalStress(string caseName, double x, double y)
        {
            var CaseIndex = -1;
            for (var i = 0; i < LoadCases.Count; i++)
            {
                if (LoadCases[i].Name == caseName)
                {
                    CaseIndex = i;
                    break;
                }
            }

            if (CaseIndex == -1)
            {
                throw new AggregateException("Case Name is not exist");
            }

            var j = GetElementIndexByPosition(x, y);

            if (j == -1)
            {
                return null;
            }

            var Xl = x - AreaElements[j].X0;
            var Yl = y - AreaElements[j].Y0;

            return AreaElements[j].GetStress(CaseIndex, Xl, Yl)[0];
        }

        public double? GetGlobalShearXZ(string caseName, double x, double y)
        {
            var CaseIndex = -1;
            for (var i = 0; i < LoadCases.Count; i++)
            {
                if (LoadCases[i].Name == caseName)
                {
                    CaseIndex = i;
                    break;
                }
            }

            if (CaseIndex == -1)
            {
                throw new AggregateException("Case Name is not exist");
            }

            var j = GetElementIndexByPosition(x, y);

            if (j == -1)
            {
                return null;
            }

            var Xl = x - AreaElements[j].X0;
            var Yl = y - AreaElements[j].Y0;

            return AreaElements[j].GetShearXZ(CaseIndex, Xl, Yl);
        }

        public double? GetGlobalShearYZ(string caseName, double x, double y)
        {
            var caseIndex = -1;
            for (var i = 0; i < LoadCases.Count; i++)
            {
                if (LoadCases[i].Name == caseName)
                {
                    caseIndex = i;
                    break;
                }
            }

            if (caseIndex == -1)
            {
                throw new AggregateException("Case Name is not exist");
            }

            var j = GetElementIndexByPosition(x, y);

            if (j == -1)
            {
                return null;
            }

            var Xl = x - AreaElements[j].X0;
            var Yl = y - AreaElements[j].Y0;

            return AreaElements[j].GetShearYZ(caseIndex, Xl, Yl);
        }

        #endregion
        #region "Displacement"
        public double? GetGlobalDiplacement(string caseName, double x, double y, decimal tolerance)
        {
            var caseIndex = -1;
            for (var i0 = 0; i0 < LoadCases.Count; i0++)
            {
                if (LoadCases[i0].Name == caseName)
                {
                    caseIndex = i0;
                    break;
                }
            }

            if (caseIndex == -1)
            {
                throw new AggregateException("Case Name is not exist");
            }

            var j = GetNodeIndexByPosition(x, y, tolerance);
            if (j != -1)
            {
                return Nodes[j].Displacement[0, caseIndex];
            }
            j = GetElementIndexByPosition(x, y);

            if (j == -1)
            {
                return null;
            }

            var X1 = x - AreaElements[j].X0;
            var Y1 = y - AreaElements[j].Y0;

            return AreaElements[j].GetDisplacementApproximate(caseIndex, X1, Y1);
        }

        public double[] GetGlobalDiplacement(string caseName, Point[] xy, decimal tolerance)
        {
            //var caseIndex = GetLoadCaseIndex(caseName);
            //caseIndex = Array.FindIndex(LoadCaseElement, element => element.Name == caseName);

            //if (caseIndex == -1)
            //{
            //    throw new AggregateException("Case Name is not exist");
            //}

            //var count = xy.Length;
            //var results = new double[count];
            //for (var i = 0; i < count; i++)
            //{
            //    var j = GetElementIndexByPosition(xy[i].X, xy[i].Y);

            //    if (j == -1)
            //    {
            //        continue;
            //    }

            //    results[i] = Elements[j].GetDisplacementApproximate(caseIndex, (xy[i].X - Elements[j].X0), (xy[i].Y - Elements[j].Y0));
            //}

            //return results;
            throw new NotImplementedException();
        }

        //Public Function GetGlobalDiplacement1(CaseName As String, X As Double, Y As Double, Tol As Decimal) As Double
        //    Dim Xe, Ye As Double
        //    Dim j As Integer
        //    Dim CaseIndex As Integer

        //    CaseIndex = -1
        //    For i0 As Integer = 0 To NumberOfLoadCases - 1
        //        If LoadCaseElement(i0).Name = CaseName Then
        //            CaseIndex = i0
        //            Exit For
        //        End If
        //    Next i0

        //    If CaseIndex = -1 Then
        //        Throw New AggregateException("Case Name is not exist")
        //    End If

        //    j = GetNodeIndexByPosition(X, Y, Tol)
        //    If j <> -1 Then
        //        Return PointElement(j).Displacement(0, CaseIndex)
        //    End If
        //    j = GetElementIndexByPosition(X, Y)

        //    If j = -1 Then
        //        Return Nothing
        //    End If

        //    Xe = X - AreaElement(j).X0
        //    Ye = Y - AreaElement(j).Y0

        //    Return AreaElement(j).InterpolationCoeff(0, CaseIndex) + AreaElement(j).InterpolationCoeff(1, CaseIndex) * Xe + AreaElement(j).InterpolationCoeff(2, CaseIndex) * Ye + AreaElement(j).InterpolationCoeff(3, CaseIndex) * Xe * Ye
        //End Function

        public double? GetGlobalDiplacementNode(string caseName, double x, double y)
        {
            var caseIndex = -1;
            for (var i = 0; i < LoadCases.Count; i++)
            {
                if (LoadCases[i].Name == caseName)
                {
                    caseIndex = i;
                    break;
                }
            }

            if (caseIndex == -1)
            {
                throw new AggregateException("Case Name is not exist");
            }

            var j = GetNodeIndexByPosition(x, y, 50);

            if (j == -1)
            {
                return null;
            }

            return Nodes[j].Displacement[0, caseIndex];
        }

        #endregion
        #region "Soil Reaction"

        public double? GetGlobalSoilReactionPerArea(string caseName, double x, double y, decimal tolerance)
        {
            var caseIndex = LoadCases.FindIndex(p => p.Name == caseName);

            if (caseIndex == -1)
            {
                throw new AggregateException("Case Name is not exist");
            }

            var index = GetNodeIndexByPosition(x, y, tolerance);
            if (index != -1)
            {
                return Nodes[index].SoilReactionPerArea[caseIndex];
            }

            index = GetElementIndexByPosition(x, y);

            if (index == -1)
            {
                return null;
            }

            var X1 = x - AreaElements[index].X0;
            var Y1 = y - AreaElements[index].Y0;

            return AreaElements[index].GetSoilReaction(caseIndex, X1, Y1);
        }

        public double? GetGlobalSoilReaction(string caseName, double x, double y, decimal tolerance)
        {
            var caseIndex = LoadCases.FindIndex(p => p.Name == caseName);

            if (caseIndex == -1)
            {
                throw new AggregateException("Case Name is not exist");
            }

            var index = GetNodeIndexByPosition(x, y, tolerance);

            if (index == -1)
            {
                return null;
            }

            return Nodes[index].SoilReaction[caseIndex];
        }

        public double[] GetGlobalSoilReaction(string caseName, Point[] xy, decimal tolerance)
        {
            var caseIndex = LoadCases.FindIndex(p => p.Name == caseName);

            if (caseIndex == -1)
            {
                throw new AggregateException("Case Name is not exist");
            }

            var count = xy.Length;
            var results = new double[count];
            for (var i = 0; i < count; i++)
            {
                var j = GetElementIndexByPosition(xy[i].X, xy[i].Y);

                if (j == -1)
                {
                    continue;
                }

                results[i] = AreaElements[j].GetSoilReaction(caseIndex, xy[i].X - AreaElements[j].X0, xy[i].Y - AreaElements[j].Y0);
            }

            return results;
        }
        #endregion

        #region "Strip"
        public double[,,] GetGlobalStripX()
        {
            var result = new double[2, PointCoordX.Count, LoadCases.Count];

            for (var i = 0; i < LoadCases.Count; i++)
            {
                var currentShear = 0.0;
                var currentMoment = 0.0;
                for (var j = 0; j < PointCoordX.Count; j++)
                {
                    if (j > 0)
                    {
                        currentMoment += currentShear * (double)(PointCoordX[j] - PointCoordX[j - 1]);
                    }

                    for (var i2 = 0; i2 < NumberOfNodes; i2++)
                    {
                        if (Nodes[i2].X == PointCoordX[j])
                        {
                            currentShear += Nodes[i2].SoilReaction[i] - Force[i2 * nDOF, i];
                        }
                    }

                    result[0, j, i] = currentShear;
                    result[1, j, i] = currentMoment;
                }
            }

            return result;
        }

        public double[] GetGlobalStripX(string caseName, double x)
        {
            //if (!LoadCaseElement.Contains(caseName))
            //{
            //    return null;
            //}

            var loadCaseIndex = LoadCases.FindIndex(p => p.Name == caseName);

            var currentShear = 0.0;
            var currentMoment = 0.0;
            for (var i = 0; i < NumberOfNodes; i++)
            {
                if ((double)Nodes[i].X <= x)
                {
                    currentShear += Nodes[i].SoilReaction[loadCaseIndex] - Force[i * nDOF, loadCaseIndex];
                    currentMoment += (Nodes[i].SoilReaction[loadCaseIndex] - Force[i * nDOF, loadCaseIndex]) * ((double)Nodes[i].X - x);
                }
            }

            return new[] { currentShear, currentMoment };
        }

        public double[] GetGlobalStripY(string caseName, double y)
        {
            double CurrentShear = 0;
            double CurrentMoment = 0;
            int CaseIndex;

            //if (!LoadCaseObject.Keys.Contains(caseName))
            //{
            //    return null;
            //}

            CaseIndex = LoadCases.FindIndex(p => p.Name == caseName);

            for (var i = 0; i < NumberOfNodes; i++)
            {
                if (Nodes[i].Y <= y)
                {
                    CurrentShear += Nodes[i].SoilReaction[CaseIndex] - Force[i * nDOF, CaseIndex];
                    CurrentMoment += (Nodes[i].SoilReaction[CaseIndex] - Force[i * nDOF, CaseIndex]) * ((double)Nodes[i].Y - y);
                }
            }

            return new[] { CurrentShear, CurrentMoment };
        }




        private void GenerateElementStiffness()
        {
            for (var i = 0; i < AreaElements.Count; i++)
            {
                AreaElements[i].Build();
            }

            for (var i0 = 0; i0 < NumberOfElements; i0++)
            {
                var n = AreaElements[i0].NumberOfNodes;
                for (var i1 = 0; i1 < n; i1++)
                {
                    for (var i2 = 0; i2 < n; i2++)
                    {
                        for (var i3 = 0; i3 < nDOF; i3++)
                        {
                            for (var i4 = 0; i4 < nDOF; i4++)
                            {
                                var ii2 = AreaElements[i0].Nodes[i1] * nDOF + i3;
                                var ii3 = AreaElements[i0].Nodes[i2] * nDOF + i4;

                                InitialStiffness[ii2, ii3] += AreaElements[i0].Stiffness[i1 * (n - 1) + i3, i2 * (n - 1) + i4];
                            }
                        }
                    }
                }
            }

            //Applying Area Loads
            for (var i = 0; i < AreaElements.Count(); i++)
            {
                var F = GetAreaLoadArray(AreaElements[i].b, AreaElements[i].c);

                for (var j = 0; j < LoadCases.Count; j++)
                {
                    for (var k = 0; k < AreaElements[i].NumberOfNodes; k++)
                    {
                        Force[AreaElements[i].Nodes[k] * nDOF + 0, j] += F[k, 0];
                        Force[AreaElements[i].Nodes[k] * nDOF + 1, j] += F[k, 1];
                        Force[AreaElements[i].Nodes[k] * nDOF + 2, j] += F[k, 2];
                    }
                }
            }

            //Applying Point Loads
            for (var i = 0; i < Nodes.Count; i++)
            {
                for (var j = 0; j < LoadCases.Count; j++)
                {
                    //var Load = new double[nDOF];
                    //for (var k = 0; k < LoadCaseElement[j].LoadPatternNames.Count; k++)
                    //{
                    //    var PatIndex = 0;//LoadPatternElement.ToList().FindIndex(p => p.Name == LoadCaseElement[j].LoadPatternNames[k]);

                    //    if (PatIndex == -1)
                    //    {
                    //        throw new ArgumentException("Error");
                    //    }

                    //    for (var i4 = 0; i4 < nDOF; i4++)
                    //    {
                    //        Load[i4] += LoadCaseElement[j].LoadPatternCoeff[k] * Nodes[i].Load[i4, PatIndex];
                    //    }
                    //}

                    //for (var i3 = 0; i3 < nDOF; i3++)
                    //{
                    //    if (Load[i3] != 0)
                    //    {
                    //        Load[i3] = Load[i3];
                    //    }

                    //    Force[i * nDOF + i3, j] += Load[i3];
                    //}
                }
            }
        }


        private double[,] GetAreaLoadArray(double b, double c)
        {
            return new[,] {
                {
                    b * c / 4,
                    b * Math.Pow(c, 2) / 24,
                    -(Math.Pow(b, 2) * c) / 24
                },
                {
                    b * c / 4,
                    b * Math.Pow(c, 2) / 24,
                    Math.Pow(b, 2) * c / 24
                },
                {
                    b * c / 4,
                    -(b * Math.Pow(c, 2)) / 24,
                    Math.Pow(b, 2) * c / 24
                },
                {
                    b * c / 4,
                    -(b * Math.Pow(c, 2)) / 24,
                    -(Math.Pow(b, 2) * c) / 24
                }
            };
        }

        private void ApplyBoundaryCondition()
        {
            var dominantPilot = 0.0;
            for (var i = 0; i < NumberOfNodes * nDOF; i++)
            {
                dominantPilot += MainStiffness[i, i];
            }

            dominantPilot /= NumberOfNodes * nDOF;

            var index = 0;
            for (var i = 0; i < NumberOfNodes; i++)
            {
                for (var j = 0; j < nDOF; j++)
                {
                    if (!Nodes[i].BoundaryCondition[j])
                    {
                        continue;
                    }

                    for (var k = 0; k < NumberOfNodes * nDOF; k++)
                    {
                        index = i * nDOF + j;
                        MainStiffness[index, k] = 0;
                        MainStiffness[k, index] = 0;
                    }
                    MainStiffness[index, index] = dominantPilot;
                }
            }

            for (var i = 0; i < NumberOfNodes * nDOF; i++)
            {
                for (var j = 0; j < NumberOfNodes * nDOF; j++)
                {
                    MainStiffness[i, j] = InitialStiffness[i, j];
                }
            }

            for (var i = 0; i < NumberOfNodes; i++)
            {
                MainStiffness[i * nDOF, i * nDOF] += Nodes[i].AreaSpringStiffness;
            }
        }

        private void ApplyStiffnessModificationNonlinear(IReadOnlyList<double> displacement)
        {
            for (var i = 0; i < NumberOfNodes * nDOF; i++)
            {
                for (var j = 0; j < NumberOfNodes * nDOF; j++)
                {
                    MainStiffness[i, j] = InitialStiffness[i, j];
                }
            }

            for (var i = 0; i < NumberOfNodes; i++)
            {
                if (displacement[i * nDOF] < 0)
                {
                    MainStiffness[i * nDOF, i * nDOF] += Nodes[i].AreaSpringStiffness;
                }
            }
        }

        public new void Build()
        {
            InitialStuff();
            GenerateElementStiffness();
            ApplyBoundaryCondition();
        }

        public new void RunAnalysis()
        {
            var effFrameStiffnessM = Matrix<double>.Build.DenseOfArray(MainStiffness);
            var nodalForceM = Matrix<double>.Build.DenseOfArray(Force);
            var displacementM = effFrameStiffnessM.Solve(nodalForceM);
            var supportReactionM = Matrix<double>.Build.DenseOfArray(MainStiffness) * displacementM;
            var Displacement = displacementM.Transpose();
            var supportReaction = effFrameStiffnessM * supportReactionM;

            for (var i = 0; i < LoadCases.Count; i++)
            {
                //if (LoadCases[i].CaseType == AnalysisTypes.Nonlinear)
                //{
                //    var D = new double[NumberOfNodes * nDOF];
                //    var F = new double[NumberOfNodes * nDOF];

                //    for (var j = 0; j < NumberOfNodes * nDOF; j++)
                //    {
                //        F[j] = Force[j, i];
                //        D[j] = Displacement[j, i];
                //    }

                //    SolveNonLinearAnalysis(F, ref D, LoadCaseElement[i].NormalError, LoadCaseElement[i].MaximumIteration, out _, out _);

                //    for (var j = 0; j < NumberOfNodes * nDOF; j++)
                //    {
                //        Displacement[j, i] = D[j];
                //    }

                //    //System.Buffer.BlockCopy(D, 0, Displacement, 0, D.Length * sizeof(double));
                //    //System.Buffer.BlockCopy(D, 0, Displacement, i * D.GetLength(0) * sizeof(double), D.GetLength(0) * sizeof(double));
                //}
            }

            //for (var i = 0; i < supportReaction.GetLength(0); i++)
            //{
            //    for (var j = 0; j < supportReaction.GetLength(1); j++)
            //    {
            //        Debug.WriteLine(supportReaction[i, j]);
            //    }
            //}

            PostAnalysis();
        }

        private void SolveNonLinearAnalysis(double[] force, ref double[] displacement, decimal maximumConvergenceTolerance, int maximumIterationCount, out double convergenceTolerance, out int IterationCounter)
        {
            var F_Unb = (double[])force.Clone();

            ApplyStiffnessModificationNonlinear(displacement);

            for (var i = 0; i < force.Length; i++)
            {
                F_Unb[i] = force[i];
                for (var i2 = 0; i2 < force.Length; i2++)
                {
                    F_Unb[i] -= MainStiffness[i, i2] * displacement[i2];
                }
            }

            var n1 = GetNormalVector(force);
            var n2 = GetNormalVector(F_Unb);

            convergenceTolerance = n2 / n1;

            IterationCounter = 1;
            while (convergenceTolerance > (double)maximumConvergenceTolerance && IterationCounter < maximumIterationCount)
            {
                var effFrameStiffnessM = Matrix<double>.Build.DenseOfArray(MainStiffness);
                var nodalForceM = Vector<double>.Build.DenseOfArray(F_Unb);
                var displacementM = effFrameStiffnessM.Solve(nodalForceM);
                var supportReactionM = Matrix<double>.Build.DenseOfArray(MainStiffness) * displacementM;

                var Displacement = displacementM.ToArray();
                var supportReaction = supportReactionM.ToArray();

                ApplyStiffnessModificationNonlinear(displacement);

                for (var i = 0; i < force.Length; i++)
                {
                    F_Unb[i] = force[i];
                    for (var j = 0; j < force.Length; j++)
                    {
                        F_Unb[i] -= MainStiffness[i, j] * displacement[j];
                    }
                }

                n2 = GetNormalVector(F_Unb);

                convergenceTolerance = n2 / n1;
                IterationCounter += 1;
            }
        }


        #region "General"
        public override void PostAnalysis()
        {
            const double TOLERANCE = 0.0000000001;
            for (var i = 0; i < LoadCases.Count; i++)
            {
                for (var j = 0; j < MainStiffness.GetLength(0); j++)
                {
                    for (var k = 0; k < MainStiffness.GetLength(1); k++)
                    {
                        Reaction[j, i] += MainStiffness[j, k] * Displacement[k, i];
                    }
                }
            }

            for (var i = 0; i < NumberOfNodes; i++)
            {
                Nodes[i].Displacement = new double[nDOF, LoadCases.Count + 1];
                for (var j = 0; j < LoadCases.Count; j++)
                {
                    for (var k = 0; k < nDOF; k++)
                    {
                        Nodes[i].Displacement[k, j] = Displacement[i * nDOF + k, j];
                        Nodes[i].Reaction[k, j] = Reaction[i * nDOF + k, j];
                    }

                    //if (LoadCaseElement[j].CaseType == AnalysisTypes.Nonlinear && Displacement[i * nDOF, j] >= 0)
                    //{
                    //    Nodes[i].SoilReactionPerArea[j] = 0;
                    //    Nodes[i].SoilReaction[j] = 0;
                    //}
                    //else
                    //{
                    //    Nodes[i].SoilReactionPerArea[j] = Displacement[i * nDOF, j] * Nodes[i].SpringStiffness;
                    //    Nodes[i].SoilReaction[j] = Displacement[i * nDOF, j] * Nodes[i].AreaSpringStiffness;
                    //}
                }
            }

            for (var i0 = 0; i0 < LoadCases.Count; i0++)
            {
                for (var i1 = 0; i1 < AreaElements.Count(); i1++)
                {
                    for (var i2 = 0; i2 < AreaElements[i1].Nodes.Count(); i2++)
                    {
                        for (var i3 = 0; i3 < nDOF; i3++)
                        {
                            AreaElements[i1].Displacement[i2 * nDOF + i3, i0] = Nodes[AreaElements[i1].Nodes[i2]].Displacement[i3, i0];
                            AreaElements[i1].Reaction[i2 * nDOF + i3, i0] = Nodes[AreaElements[i1].Nodes[i2]].Reaction[i3, i0];
                        }
                        AreaElements[i1].SoilReaction[i2, i0] = Nodes[AreaElements[i1].Nodes[i2]].SoilReactionPerArea[i0];
                    }
                }
            }

            //Uplift Ratio
            var globalUpliftRatioList = new List<double>();
            for (var i = 0; i < LoadCases.Count; i++)
            {
                //if (LoadCaseElement[i].CaseType == AnalysisTypes.Nonlinear)
                //{
                //    var pointCounter = 0;
                //    for (var j = 0; j < NumberOfNodes; j++)
                //    {
                //        if (Math.Abs(Nodes[j].SoilReaction[i]) < TOLERANCE)
                //        {
                //            pointCounter += 1;
                //        }
                //    }
                //    globalUpliftRatioList.Add(pointCounter / (double)NumberOfNodes);
                //}
                //else
                //{
                //    globalUpliftRatioList.Add(0);
                //}
            }

            GlobalUpliftRatio = globalUpliftRatioList.AsReadOnly();

            //Maximum and Minimum Soil Pressure
            var maxSoilPressureList = new List<double>();
            var minSoilPressureList = new List<double>();
            var maxDisplacamentList = new List<double>();
            var minDisplacamentList = new List<double>();

            for (var i0 = 0; i0 < LoadCases.Count; i0++)
            {
                var maxSoilPressure = Nodes[0].SoilReactionPerArea[i0];
                var minSoilPressure = Nodes[0].SoilReactionPerArea[i0];
                var maxDisplacement = Nodes[0].Displacement[0, i0];
                var minDisplacement = Nodes[0].Displacement[0, i0];
                for (var i1 = 1; i1 < NumberOfNodes; i1++)
                {
                    if (maxSoilPressure < Nodes[i1].SoilReactionPerArea[i0])
                    {
                        maxSoilPressure = Nodes[i1].SoilReactionPerArea[i0];
                    }
                    if (minSoilPressure > Nodes[i1].SoilReactionPerArea[i0])
                    {
                        minSoilPressure = Nodes[i1].SoilReactionPerArea[i0];
                    }

                    if (maxDisplacement < Nodes[i1].Displacement[0, i0])
                    {
                        maxDisplacement = Nodes[i1].Displacement[0, i0];
                    }
                    if (minDisplacement > Nodes[i1].Displacement[0, i0])
                    {
                        minDisplacement = Nodes[i1].Displacement[0, i0];
                    }
                }

                maxSoilPressureList.Add(maxSoilPressure);
                minSoilPressureList.Add(minSoilPressure);

                maxDisplacamentList.Add(maxDisplacement);
                minDisplacamentList.Add(minDisplacement);
            }

            MaximumSoilPressurePerArea = maxSoilPressureList.AsReadOnly();
            MinimumSoilPressurePerArea = minSoilPressureList.AsReadOnly();

            MaximumDisplacement = maxDisplacamentList.AsReadOnly();
            MinimumDisplacement = minDisplacamentList.AsReadOnly();

            //For i0 As Integer = 0 To NumberOfLoadCases - 1
            //    For i1 As Integer = 0 To AreaElement.Count - 1
            //        If AreaElement(i1).NumberOfNodes = 4 Then
            //            Dim x1, x2 As Double
            //            Dim y1, y2 As Double
            //            Dim z1, z2, z3, z4 As Double
            //            Dim A(,), B(), P() As Double

            //            x1 = PointElement(AreaElement(i1).Elements(0)).Coord.x
            //            x2 = PointElement(AreaElement(i1).Elements(1)).Coord.x

            //            y1 = PointElement(AreaElement(i1).Elements(0)).Coord.y
            //            y2 = PointElement(AreaElement(i1).Elements(3)).Coord.y

            //            z1 = AreaElement(i1).Displacement(0 * nDOF, i0)
            //            z2 = AreaElement(i1).Displacement(1 * nDOF, i0)
            //            z3 = AreaElement(i1).Displacement(2 * nDOF, i0)
            //            z4 = AreaElement(i1).Displacement(3 * nDOF, i0)

            //            A = {{1, x1, y1, x1 * y1}, _
            //                 {1, x1, y2, x1 * y2}, _
            //                 {1, x2, y1, x2 * y1}, _
            //                 {1, x2, y2, x2 * y2}}

            //            B = {z1, z4, z2, z3}

            //            P = MatrixOperation.Solve4(A, B)
            //            P = P
            //            Dim z As Double
            //            Dim x, y As Double

            //            x = x2
            //            y = y2

            //            z = P(0) + P(1) * x + P(1) * y + P(2) * x * y

            //            For i2 As Integer = 0 To AreaElement(i1).NumberOfNodes - 1
            //                AreaElement(i1).InterpolationCoeff(i2, i0) = P(i2)
            //            Next i2
            //        Else
            //            MsgBox("111")
            //        End If
            //    Next i1
            //Next i0

        }

        private int GetElementIndexByPosition(double X, double Y)
        {
            for (var i1 = 0; i1 < AreaElements.Count; i1++)
            {
                if (X >= AreaElements[i1].X0 && X <= AreaElements[i1].X0 + AreaElements[i1].b && Y >= AreaElements[i1].Y0 && Y <= AreaElements[i1].Y0 + AreaElements[i1].c)
                {
                    return i1;
                }
            }

            return -1;
        }

        private int GetNodeIndexByPosition(double x, double y, decimal detectTolerance)
        {
            var j = -1;
            var MinDist = double.MaxValue;
            for (var i1 = 0; i1 < Nodes.Count; i1++)
            {
                var Dist = Math.Sqrt(Math.Pow(Nodes[i1].X - x, 2) + Math.Pow(Nodes[i1].Y - y, 2));

                if (MinDist > Dist)
                {
                    MinDist = Dist;
                    j = i1;
                }
            }

            return MinDist <= (double)detectTolerance ? j : -1;
        }

        #endregion

        private IntPtr GetIntPtr(double Value)
        {
            return System.Runtime.InteropServices.GCHandle.Alloc(Value, System.Runtime.InteropServices.GCHandleType.Pinned).AddrOfPinnedObject();
        }

        private double GetNormalVector(IReadOnlyList<double> value)
        {
            var result = 0.0;
            for (var i = 0; i < value.Count; i++)
            {
                result += Math.Pow(value[i], 2);
            }

            return Math.Sqrt(result);
        }


        #endregion

        #endregion

        #region Function
        private int SetGaussIntergration(int gaussPointNo)
        {
            double m0;
            double m1;
            double m2;
            double r1;
            double r2;

            switch (gaussPointNo)
            {
                case 1:
                    GaussPoints = new double[] { 0 };
                    GaussWeight = new double[] { 2 };
                    break;
                case 2:
                    GaussPoints = new[] { -1 / Math.Sqrt(3), 1 / Math.Sqrt(3) };
                    GaussWeight = new double[] { 1, 1 };
                    break;
                case 3:
                    GaussPoints = new[] { -Math.Sqrt(3 / 5.0), 0, Math.Sqrt(3 / 5.0) };
                    GaussWeight = new[] { 5 / 9.0, 8 / 9.0, 5 / 9.0 };
                    break;
                case 4:
                    r1 = Math.Sqrt(3 / 7.0 - 2 / 7.0 * Math.Sqrt(6 / 5.0));
                    r2 = Math.Sqrt(3 / 7.0 + 2 / 7.0 * Math.Sqrt(6 / 5.0));

                    m1 = (18 + Math.Sqrt(30)) / 36;
                    m2 = (18 - Math.Sqrt(30)) / 36;

                    GaussPoints = new[] { -r2, -r1, r1, r2 };
                    GaussWeight = new[] { m2, m1, m1, m2 };
                    break;
                case 5:
                    r1 = 1 / 3.0 * Math.Sqrt(5 - 2 * Math.Sqrt(10 / 7.0));
                    r2 = 1 / 3.0 * Math.Sqrt(5 + 2 * Math.Sqrt(10 / 7.0));

                    m0 = 128 / 225.0;
                    m1 = (322 + 13 * Math.Sqrt(70)) / 900;
                    m2 = (322 - 13 * Math.Sqrt(70)) / 900;

                    GaussPoints = new[] { -r2, -r1, 0, r1, r2 };
                    GaussWeight = new[] { m2, m1, m0, m1, m2 };
                    break;
                default:
                    return 1;
            }
            return 0;
        }

        private double[,] GetCwinv(int[,] Phiw, double dx, double dy)
        {
            const int n = 12;

            var x0 = 0.0;
            var x1 = dx;
            var y0 = 0.0;
            var y1 = dy;

            var w1 = Phider(Phiw, 0, 0, x0, y0);
            var w2 = Phider(Phiw, 0, 0, x1, y0);
            var w3 = Phider(Phiw, 0, 0, x1, y1);
            var w4 = Phider(Phiw, 0, 0, x0, y1);
            var wx1 = Phider(Phiw, 1, 0, x0, y0);
            var wx2 = Phider(Phiw, 1, 0, x1, y0);
            var wx3 = Phider(Phiw, 1, 0, x1, y1);
            var wx4 = Phider(Phiw, 1, 0, x0, y1);
            var wy1 = Phider(Phiw, 0, 1, x0, y0);
            var wy2 = Phider(Phiw, 0, 1, x1, y0);
            var wy3 = Phider(Phiw, 0, 1, x1, y1);
            var wy4 = Phider(Phiw, 0, 1, x0, y1);

            var matrixBuilder = Matrix<double>.Build;
            var C = matrixBuilder.Dense(n, n);
            for (var i = 0; i < n; i++)
            {
                C[0, i] = w1[i];
                C[1, i] = wx1[i];
                C[2, i] = wy1[i];
                C[3, i] = w2[i];
                C[4, i] = wx2[i];
                C[5, i] = wy2[i];
                C[6, i] = w3[i];
                C[7, i] = wx3[i];
                C[8, i] = wy3[i];
                C[9, i] = w4[i];
                C[10, i] = wx4[i];
                C[11, i] = wy4[i];
            }

            return C.Inverse().ToArray();
        }

        private double[,] GetCuinv(int[,] Phiu, double dx, double dy)
        {
            var C = new double[8, 8];
            double x0, x1, y0, y1;
            double[] u1, u2, u3, u4;

            x0 = 0;
            x1 = dx;
            y0 = 0;
            y1 = dy;

            u1 = Phider(Phiu, 0, 0, x0, y0);
            u2 = Phider(Phiu, 0, 0, x1, y0);
            u3 = Phider(Phiu, 0, 0, x1, y1);
            u4 = Phider(Phiu, 0, 0, x0, y1);

            for (var i1 = 0; i1 < u1.GetLength(0); i1++)
            {
                C[0, i1] = u1[i1];
                C[1, i1] = u2[i1];
                C[2, i1] = u3[i1];
                C[3, i1] = u4[i1];

                C[4 + 0, 4 + i1] = u1[i1];
                C[4 + 1, 4 + i1] = u2[i1];
                C[4 + 2, 4 + i1] = u3[i1];
                C[4 + 3, 4 + i1] = u4[i1];
            }

            //PrintString(C, "0.##")

            //Dim C1(8 - 1, 8 - 1) As Double
            //Dim V() As Integer = {0, 4, 1, 5, 2, 6, 3, 7}
            //For i1 As Integer = 0 To C.GetLength(0) - 1
            //    For i2 As Integer = 0 To C.GetLength(0) - 1
            //        C1(i1, i2) = C(V(i1), V(i2))
            //    Next i2
            //Next i1

            //C = C1

            //PrintString(C1, "0.##")

            //Dim CC As New MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(C.GetLength(0))
            //For i1 As Integer = 0 To C.GetLength(0) - 1
            //    For i2 As Integer = 0 To C.GetLength(1) - 1
            //        CC(i1, i2) = C(i1, i2)
            //    Next i2
            //Next i1

            //Return CC.Inverse.ToArray
            return new double[,] { { } };
        }

        private double[] Phider(int[,] Phi, int nx, int ny, double X, double Y)
        {
            var powersX = new double[Phi.GetLength(0)];
            var powersY = new double[Phi.GetLength(0)];
            var coeffX = new int[Phi.GetLength(0)];
            var coeffY = new int[Phi.GetLength(0)];
            var result = new double[Phi.GetLength(0)];

            for (var i = 0; i < coeffX.Length; i++)
            {
                coeffX[i] = 1;
                coeffY[i] = 1;
            }

            for (var i = 0; i < powersX.Length; i++)
            {
                powersX[i] = Phi[i, 0];
                powersY[i] = Phi[i, 1];
            }

            while (nx > 0)
            {
                for (var i = 0; i < coeffX.Length; i++)
                {
                    coeffX[i] = Convert.ToInt32(coeffX[i] * powersX[i]);
                    powersX[i] = Math.Max(0, powersX[i] - 1);
                }
                nx -= 1;
            }

            while (ny > 0)
            {
                for (var i = 0; i < coeffY.Length; i++)
                {
                    coeffY[i] = Convert.ToInt32(coeffY[i] * powersY[i]);
                    powersY[i] = Math.Max(0, powersY[i] - 1);
                }
                ny -= 1;
            }

            for (var i = 0; i < coeffX.Length; i++)
            {
                result[i] = coeffX[i] * Math.Pow(X, powersX[i]) * coeffY[i] * Math.Pow(Y, powersY[i]);
            }

            return result;
        }

        private double[,] GetBendingStiffnessHard(int ElementID, double h, double E, double nu)
        {
            var tmp = E * Math.Pow(h, 3) / (12 * (1 - Math.Pow(nu, 2)));
            var Db = new[,] { { tmp, nu * tmp, 0 }, { nu * tmp, tmp, 0 }, { 0, 0, (1 - nu) / 2 * tmp } };

            //w(x,y) = α1 + α2x + α3y + α4x2 + α5xy + α6y2 + α7x3 + α8x2y + α9xy2 + α10y3 + α11x3y + α12xy3
            var Phiw = new[,] {
                {0,0},{1,0},{0,1},{2,0},{0,2},{1,1},{2,1},{1,2},
                {3,0},{0,3},{3,1},{1,3}
            };
            //powers of {x,y} of polynomial

            var dx = (double)(Nodes[AreaElements[ElementID].Nodes[1]].X - Nodes[AreaElements[ElementID].Nodes[0]].X);
            var dy = (double)(Nodes[AreaElements[ElementID].Nodes[3]].Y - Nodes[AreaElements[ElementID].Nodes[0]].Y);

            var Cwinv = GetCwinv(Phiw, dx, dy);

            var LocalGaussPoints = new double[GaussPoints.Length];
            for (var i1 = 0; i1 < GaussPoints.Length; i1++)
            {
                LocalGaussPoints[i1] = (GaussPoints[i1] + 1) / 2;
            }

            var matrixBuilder = Matrix<double>.Build;
            var Db_matrix = matrixBuilder.DenseOfArray(Db);
            var Cwinv_matrix = matrixBuilder.DenseOfArray(Cwinv);


            var B_matrix = matrixBuilder.Dense(Cwinv.GetLength(0), Cwinv.GetLength(1));
            for (var i1 = 0; i1 < GaussPoints.Length; i1++)
            {
                for (var i2 = 0; i2 < GaussPoints.Length; i2++)
                {
                    var rx = LocalGaussPoints[i2] * dx;
                    var ry = LocalGaussPoints[i1] * dy;

                    var wx = GaussWeight[i2];
                    var wy = GaussWeight[i1];

                    var H1 = Phider(Phiw, 2, 0, rx, ry);
                    var H2 = Phider(Phiw, 0, 2, rx, ry);
                    var H3 = Phider(Phiw, 1, 1, rx, ry);

                    for (var i3 = 0; i3 < H1.Length; i3++)
                    {
                        H1[i3] = -H1[i3];
                        H2[i3] = -H2[i3];
                        H3[i3] = -2 * H3[i3];
                    }


                    var HH = new double[3, H1.Length];
                    for (var j = 0; j < H1.Length; j++)
                    {
                        HH[0, j] = H1[j];
                        HH[1, j] = H2[j];
                        HH[2, j] = H3[j];
                    }

                    var HHH = matrixBuilder.DenseOfArray(HH);

                    B_matrix = B_matrix + dx * dy / 4 * wx * wy * HHH.Transpose() * (Db_matrix * HHH);
                }
            }

            return (Cwinv_matrix.Transpose() * B_matrix * Cwinv_matrix).ToArray();
        }

        //Private Function GetAxialStiffness(ByVal ElementID As Integer, ByVal h As Double, ByVal E As Double, ByVal nu As Double) As Double(,)
        //    Dim Cuinv(,) As Double
        //    Dim dx, dy As Decimal
        //    Dim Phiu(,) As Integer
        //    Dim Dp(,) As Double
        //    Dim tmp As Double

        //    tmp = E * h / (1 - nu ^ 2)
        //    Dp = {{tmp, nu * tmp, 0}, {nu * tmp, tmp, 0}, {0, 0, (1 - nu) / 2 * tmp}}


        //    dx = Node(Element(ElementID).Elements(1)).Coord.x - Node(Element(ElementID).Elements(0)).Coord.x
        //    dy = Node(Element(ElementID).Elements(3)).Coord.y - Node(Element(ElementID).Elements(0)).Coord.y

        //    Phiu = {{0, 0}, {1, 0}, {0, 1}, {1, 1}}

        //    Cuinv = GetCuinv(Phiu, dx, dy)

        //    Dim Bi(Cuinv.GetLength(0), Cuinv.GetLength(1)) As Double
        //    Dim LocalGaussPoints(GaussPointNo - 1) As Double

        //    For i1 As Integer = 0 To GaussPointNo - 1
        //        LocalGaussPoints(i1) = (GaussPoints(i1) + 1) / 2
        //    Next i1

        //    Dim HH, BB, DDP, CCuinv As Matrix

        //    BB = New Matrix(Cuinv.GetLength(0), Cuinv.GetLength(1))
        //    DDP = New Matrix(Dp)
        //    CCuinv = New Matrix(Cuinv)

        //    For i1 As Integer = 0 To GaussPointNo - 1
        //        For i2 As Integer = 0 To GaussPointNo - 1
        //            Dim rx, ry As Double
        //            Dim wx, wy As Double
        //            Dim H1, H2 As Double()

        //            rx = LocalGaussPoints(i2) * dx
        //            ry = LocalGaussPoints(i1) * dy
        //            wx = GaussWeight(i2)
        //            wy = GaussWeight(i1)

        //            H1 = Phider(Phiu, 1, 0, CDec(rx), CDec(ry))
        //            H2 = Phider(Phiu, 0, 1, CDec(rx), CDec(ry))

        //            HH = New Matrix({{-H1(0), -H1(1), -H1(2), -H1(3), 0, 0, 0, 0}, {0, 0, 0, 0, -H2(0), -H2(1), -H2(2), -H2(3)}, {-H2(0), -H2(1), -H2(2), -H2(3), -H1(0), -H1(1), -H1(2), -H1(3)}})

        //            BB = BB + ((dx * dy / 4) * wx * wy * HH.Transposed) * (DDP * HH)
        //        Next i2
        //    Next i1

        //    'Dim kk(,) As Double = (CCuinv.Transposed * (BB * CCuinv)).ToArray

        //    'Dim jj As New Matrix(kk)
        //    '' MsgBox(jj.IsSymmetric(0.000000001D).ToString)

        //    Return (CCuinv.Transposed * BB * CCuinv).ToArray
        //End Function
        #endregion
    }
}