using System;
using System.Linq;
using FEA.Force;
using FEA.LineElements;
using FEA.LoadCases;
using FEA.Materials;
using FEA.Nodes;
using FEA.SectionProfiles;

namespace FEA.Models
{
    public class ModelTruss2D : Model
    {
        public ModelTruss2D()
            : base(2, 2)
        {

        }

        public int AddNode(string name, double x, double y)
        {
            {
                const double tolerance = 0.000001;
                foreach (var node in Nodes)
                {
                    if (Math.Abs(node.Coordinates[0] - x) < tolerance && Math.Abs(node.Coordinates[1] - y) < tolerance)
                    {
                        throw new ArgumentException("There is a node with the same coordination in the model!");
                    }
                }

                if (this.Nodes.Any(p => p.Name == name))
                {
                    throw new ArgumentException("Duplicated node name!");
                }

                this.Nodes.Add(new Node2D(name, new []{x, y}, 2));
                return this.Nodes.Count - 1;
            }
        }

        public int AddMaterial(string name, double modulusOfElasticity)
        {
            if (this.Materials.Any(p => p.Name == name))
            {
                throw new ArgumentException("Duplicated element name");
            }

            this.Materials.Add(new Material(name, modulusOfElasticity));
            return this.Materials.Count - 1;
        }

        public int AddProfileSection(string name, double area)
        {
            if (this.ProfileSections.Any(p => p.Name == name))
            {
                throw new ArgumentException("Duplicated element name");
            }

            this.ProfileSections.Add(new ProfileSection(name, area));
            return this.ProfileSections.Count - 1;
        }

        public int AddElement(string name, string node1, string node2, string materialName, string sectionName)
        {
            if (node1 == node2)
            {
                throw new ArgumentException("Start and end node can not be the same");
            }

            var i = this.Nodes.FindIndex(p => p.Name == node1);
            var j = this.Nodes.FindIndex(p => p.Name == node2);

            if (i == -1)
            {
                throw new ArgumentException("node1 is not exist");
            }

            if (j == -1)
            {
                throw new ArgumentException("node2 is not exist");
            }

            if (this.LineElements.Any(p => p.Name == name))
            {
                throw new ArgumentException("Duplicated element name");
            }

            var material = Materials.SingleOrDefault(p => p.Name == materialName);
            if (material == null)
            {
                throw new ArgumentException($"Unknown material name {materialName}");
            }

            var profileSection = ProfileSections.SingleOrDefault(p => p.Name == sectionName);
            if (profileSection == null)
            {
                throw new ArgumentException($"Unknown profile section name {profileSection}");
            }

            this.LineElements.Add(new Truss2D(name, i, j, profileSection.A, material.ModulusOfElasticity));
            return this.LineElements.Count - 1;
        }

        public void AddRestraint(string name, bool dx, bool dy)
        {
            var node = this.Nodes.SingleOrDefault(p => p.Name == name);
            if (node == null)
            {
                throw new ArgumentException("Name does not exist");
            }

            node.IsSupport = new[] { dx, dy };
        }

        public void AddLoadCase(string loadCaseName)
        {
            if (LoadCases.Any(p => p.Name == loadCaseName))
            {
                throw new ArgumentException("Duplicated load case name!");
            }

            this.LoadCases.Add(new LoadCase(loadCaseName));
        }

        public void AddNodalForce(string name, string loadCase, double fx, double fy)
        {
            var nodeIndex = this.Nodes.FindIndex(p => p.Name == name);
            if (nodeIndex == -1)
            {
                throw new ArgumentException("Name does not exist");
            }

            var loadCaseIndex = LoadCases.FindIndex(p => p.Name == loadCase);
            if (loadCaseIndex == -1)
            {
                throw new ArgumentException($"load case \"{loadCase}\" does not exist");
            }

            NodalForces.Add(new NodalForce(nodeIndex, loadCaseIndex, new[] { fx, fy }));

            //node.Force = new[] { fx, fy };
        }

        public override void PostAnalysis()
        {
            //foreach (var element in Elements)
            //{
            //    var largeDisplacement = Elements.Count != 0;

            //    element.Elongation = 0;
            //    if (largeDisplacement)
            //    {
            //        for (var i = 0; i < nDOF; i++)
            //        {
            //            var disp2 = Nodes[element.Node2].Coordinates[i] + Nodes[element.Node2].ResultantDisplacement[i];
            //            var disp1 = Nodes[element.Node1].Coordinates[i] + Nodes[element.Node1].ResultantDisplacement[i];
            //            element.Elongation += Math.Pow(disp2 - disp1, 2);
            //        }

            //        element.Elongation = Math.Sqrt(element.Elongation) - element.Length;
            //    }
            //    else
            //    {
            //        element.Elongation = element.LocalDisplacement[nDim] - element.LocalDisplacement[0];
            //    }

            //    element.InternalForce = element.ModulusOfElasticity * element.Area / element.Length * element.Elongation;
            //}
        }

   
    }
}
