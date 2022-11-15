using System;
using System.Collections.Generic;

namespace FEA.Plate.Objects
{
    public class AreaObjects
    {
        public readonly string Name;

        public readonly string MaterialName;
        public readonly string SoilName;

        public readonly double Thickness;

        public readonly List<string> Nodes;

        public AreaObjects(string Name, string MaterialName, string SoilName, double Thickness, params string[] NodeName)
        {
            if (NodeName == null || NodeName.Length != 4)
            {
                throw new Exception("Node names should have at least 4 node name");
            }

            this.Name = Name;
            this.MaterialName = MaterialName;
            this.SoilName = SoilName;
            this.Thickness = Thickness;

            Nodes = new List<string>();
            for (var i = 0; i < NodeName.Length; i++)
            {
                Nodes.Add(NodeName[i]);
            }
        }
    }
}
