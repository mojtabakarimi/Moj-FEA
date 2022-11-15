using System.Collections.Generic;

namespace FEA.LoadDefinition
{
    public struct LoadCombinations
    {
        public string Name;
        public List<string> LoadCaseNameList;
        public List<double> LoadCaseCoeff;

        public LoadCombinations(string Name)
        {
            this.Name = Name;
            this.LoadCaseNameList = new List<string>();
            this.LoadCaseCoeff = new List<double>();
        }
    }
}
