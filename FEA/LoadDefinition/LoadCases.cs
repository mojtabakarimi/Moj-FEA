using System.Collections.Generic;
using FEA.Plate;

namespace FEA.LoadDefinition
{
    public struct LoadCases
    {
        public string Name;

        public AnalysisTypes CaseType;
        public List<string> LoadPatternNames;

        public List<double> LoadPatternCoeff;
        public decimal NormalError;

        public int MaximumIteration;
        public LoadCases(string Name, AnalysisTypes CaseType, decimal NormalError, int MaximumIteration)
        {
            this.Name = Name;
            this.CaseType = CaseType;
            this.NormalError = NormalError;
            this.MaximumIteration = MaximumIteration;

            this.LoadPatternNames = new List<string>();
            this.LoadPatternCoeff = new List<double>();
        }
    }
}
