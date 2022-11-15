using System.Collections.Generic;
using System.Linq;
using FEA.AreaElements;
using FEA.Force;
using FEA.LineElements;
using FEA.LoadCases;
using FEA.Materials;
using FEA.Nodes;
using FEA.SectionProfiles;
using MathNet.Numerics.LinearAlgebra;

namespace FEA.Models
{
    public abstract class Model
    {
        protected int nDOF { get; }
        protected int nDim { get; }

        public List<Node> Nodes { get; }

        public List<LineElement> LineElements { get; }
        public List<AreaElement> AreaElements { get; }

        public List<Material> Materials { get; }

        public List<ProfileSection> ProfileSections { get; }

        public List<LoadCase> LoadCases { get; }

        public List<NodalForce> NodalForces { get; }

        //public List<NodalDisplacement> 

        private bool[] isEssentialBoundaryCondition;
        private double[] essentialBoundaryConditionValue;
        private double[,] nodalForce;
        private Matrix<double> frameStiffness;
        private int numberOfLoadCases { get; set; }

        private double[,] Displacement;
        private double[,] SupportReaction;

        private bool isBuilt = false;

        protected Model(int ndim, int nDof)
        {
            nDim = ndim;
            nDOF = nDof;

            Nodes = new List<Node>();
            LineElements = new List<LineElement>();
            AreaElements = new List<AreaElement>();
            Materials = new List<Material>();
            ProfileSections = new List<ProfileSection>();
            LoadCases = new List<LoadCase>();
            NodalForces = new List<NodalForce>();
        }

        public int Build()
        {
            foreach (var element in LineElements)
            {
                var node1 = this.Nodes[element.Node1];
                var node2 = this.Nodes[element.Node2];

                element.Build(node1.Coordinates.ToArray(), node2.Coordinates.ToArray());
            }

            
            numberOfLoadCases = LoadCases.Count;

            frameStiffness = Matrix<double>.Build.Sparse(Nodes.Count * nDOF, Nodes.Count * nDOF);
            isEssentialBoundaryCondition = new bool[Nodes.Count * nDOF];
            essentialBoundaryConditionValue = new double[Nodes.Count * nDOF];
            nodalForce = new double[numberOfLoadCases, Nodes.Count * nDOF];
            Displacement = new double[numberOfLoadCases, Nodes.Count * nDOF];
            SupportReaction = new double[numberOfLoadCases, Nodes.Count * nDOF];

            for (var i = 0; i < Nodes.Count; i++)
            {
                for (var j = 0; j < nDOF; j++)
                {
                    isEssentialBoundaryCondition[nDOF * i + j] = Nodes[i].IsSupport[j];
                    if (isEssentialBoundaryCondition[nDOF * i + j])
                    {
                        essentialBoundaryConditionValue[nDOF * i + j] = Nodes[i].Support[j];
                    }
                }
            }

            foreach (var element in LineElements)
            {
                for (var i = 0; i < nDOF * 2; i++)
                {
                    for (var j = 0; j < nDOF * 2; j++)
                    {
                        var p = element.EFT[i];
                        var q = element.EFT[j];
                        frameStiffness[p, q] += element.Stiffness[i, j];
                    }
                }
            }

            //---------------------------------
            //Applying nodal forces to the total force vector
            //for (var i = 0; i < Nodes.Count; i++)
            //{
            //    for (var k = 0; k < nDOF; k++)
            //    {
            //        nodalForce[nDOF * i + k] += Nodes[i].Force[k];
            //    }
            //}

            for (var i = 0; i < NodalForces.Count; i++)
            {
                for (var j = 0; j < nDOF; j++)
                {
                    nodalForce[NodalForces[i].LoadCaseIndex, nDOF * NodalForces[i].NodeIndex + j] += NodalForces[i].Force[j];
                }
            }


            //Converting nodal initial displacement (settlement) to the nodal force
            for (var i = 0; i < Nodes.Count * nDOF; i++)
            {
                if (!isEssentialBoundaryCondition[i])
                {
                    continue;
                }

                //for (var k = 0; k < Nodes.Count * nDOF; k++)
                //{
                //   nodalForce[k] -= frameStiffness[k, i] * essentialBoundaryConditionValue[i];
                //}
            }

            isBuilt = true;

            return 0;
        }

        public int RunAnalysis()
        {
            var effectiveIndices = isEssentialBoundaryCondition
                .Select((p, i) => (p, i))
                .Where(p => !p.p)
                .Select(p => p.i)
                .ToList();

            var effNodalForce = Matrix<double>.Build.Dense(LoadCases.Count, effectiveIndices.Count);
            var effFrameStiffness = Matrix<double>.Build.Dense(effectiveIndices.Count, effectiveIndices.Count);

            for (var i = 0; i < effectiveIndices.Count; i++)
            {
                for (var j = 0; j < effectiveIndices.Count; j++)
                {
                    effFrameStiffness[i, j] = frameStiffness[effectiveIndices[i], effectiveIndices[j]];
                }
            }

            for (var ii = 0; ii < LoadCases.Count; ii++)
            {
                for (var i = 0; i < effectiveIndices.Count; i++)
                {
                    effNodalForce[ii, i] = nodalForce[ii, effectiveIndices[i]];
                }
            }

            //for (var i = 0; i < Nodes.Count * nDOF; i++)
            //{
            //    if (IsEssentialBoundaryCondition[i])
            //    {
            //        _nodalForce[i] = EssentialBoundaryConditionValue[i];
            //    }
            //}

            var displacementM = effFrameStiffness.Solve(effNodalForce.Transpose()).Transpose();

            for (var ii = 0; ii < LoadCases.Count; ii++)
            {
                for (var i = 0; i < effectiveIndices.Count; i++)
                {
                    Displacement[ii, effectiveIndices[i]] = displacementM[ii, i];
                }
            }

            displacementM = Matrix<double>.Build.SparseOfArray(Displacement);
            var supportReactionM = (frameStiffness * displacementM.Transpose()).Transpose();

            SupportReaction = supportReactionM.ToArray();

            for (var ii = 0; ii < LoadCases.Count; ii++)
            {
                for (var i = 0; i < isEssentialBoundaryCondition.Length; i++)
                {
                    SupportReaction[ii,i] -= nodalForce[ii,i];
                }
            }

            for (var ii = 0; ii < LoadCases.Count; ii++)
            {
                for (var i = 0; i < isEssentialBoundaryCondition.Length; i++)
                {
                    if (!isEssentialBoundaryCondition[i])
                    {
                        SupportReaction[ii,i] = nodalForce[ii,i];
                    }
                }
            }

            for (var ii = 0; ii < LoadCases.Count; ii++)
            {
                for (var i = 0; i < isEssentialBoundaryCondition.Length; i++)
                {
                    if (isEssentialBoundaryCondition[i])
                    {
                        Displacement[ii,i] = essentialBoundaryConditionValue[i];
                        //supportReaction[i] = supportReaction[i] != 0 ? nodalForce[i] + supportReaction[i] : nodalForce[i];
                    }
                    else
                    {
                        SupportReaction[ii,i] = 0.0;
                    }
                }
            }

            //Set nodal reaction and displacements

            for (var i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].ReactionForce = new double[numberOfLoadCases][];
                Nodes[i].ResultantDisplacement = new double[numberOfLoadCases][];
                for (var j = 0; j < LoadCases.Count; j++)
                {
                    Nodes[i].ReactionForce[j] = new double[nDOF];
                    Nodes[i].ResultantDisplacement[j] = new double[nDOF];
                    for (var k = 0; k < nDOF; k++)
                    {
                        Nodes[i].ReactionForce[j][k] = SupportReaction[j, nDOF * i + k];
                        Nodes[i].ResultantDisplacement[j][k] = Displacement[j, nDOF * i + k];
                    }
                }
            }

            //Set element displacement in global and local coordinates
            for (var ii = 0; ii < LoadCases.Count; ii++)
            {
                foreach (var element in LineElements)
                {
                    element.GlobalDisplacement = new double[element.EFT.Length];
                    for (var i = 0; i < element.EFT.Length; i++)
                    {
                        element.GlobalDisplacement[i] = Displacement[ii, element.EFT[i]];
                    }

                    element.LocalDisplacement = (element.Transformation * Vector<double>.Build.DenseOfArray(element.GlobalDisplacement)).ToArray();
                }
            }
            
            return 0;
        }

        public abstract void PostAnalysis();

        private double[,] ReduceStiffnessMatrix(double[,] frameStiffness, ref double[] essentialBoundaryConditionValue, ref bool[] isEssentialBoundaryCondition, ref double[] nodalForce)
        {
            var diagonalAverageValue = 0.0;
            for (var i = 0; i < Nodes.Count * nDOF; i++)
            {
                diagonalAverageValue += frameStiffness[i, i];
            }

            diagonalAverageValue = diagonalAverageValue / Nodes.Count / nDOF;
            for (var i = 0; i < Nodes.Count * nDOF; i++)
            {
                if (isEssentialBoundaryCondition[i])
                {
                    nodalForce[i] = diagonalAverageValue * essentialBoundaryConditionValue[i];
                }
            }

            var nEBC = isEssentialBoundaryCondition.Count(p => p);

            var effFrameStiffness = new double[Nodes.Count * nDOF, Nodes.Count * nDOF];
            for (var i = 0; i < Nodes.Count * nDOF; i++)
            {
                for (var j = i; j < Nodes.Count * nDOF; j++)
                {
                    effFrameStiffness[i, j] = frameStiffness[i, j];
                    effFrameStiffness[j, i] = frameStiffness[j, i];
                }
            }

            for (var i = 0; i < Nodes.Count * nDOF; i++)
            {
                if (isEssentialBoundaryCondition[i])
                {
                    for (var j = 0; j < Nodes.Count * nDOF; j++)
                    {
                        effFrameStiffness[i, j] = 0;
                        effFrameStiffness[j, i] = 0;
                    }

                    effFrameStiffness[i, i] = diagonalAverageValue;
                }
            }

            //var report = new StringBuilder();
            //for (var i = 0; i < effFrameStiffness.GetLength(0); i++)
            //{
            //    for (var j = 0; j < effFrameStiffness.GetLength(1); j++)
            //    {
            //        report.Append($"{(effFrameStiffness[i, j] >= 0 ? "+" : "-")}{Math.Abs(effFrameStiffness[i, j]):F3}\t");
            //    }

            //    report.AppendLine();
            //}

            //Debug.Print(report.ToString());
            return effFrameStiffness;

            var reducedFrameStiffness = new double[Nodes.Count * nDOF - nEBC, Nodes.Count * nDOF - nEBC];

            var i01 = 0;
            for (var i = 0; i < Nodes.Count * nDOF; i++)
            {
                if (!isEssentialBoundaryCondition[i])
                {
                    var i02 = 0;
                    for (var j = 0; j < Nodes.Count * nDOF; j++)
                    {
                        if (isEssentialBoundaryCondition[j] == false)
                        {
                            reducedFrameStiffness[i01, i02++] = effFrameStiffness[i, j];
                        }
                    }

                    i01++;
                }
            }

            return effFrameStiffness;
        }
    }
}
