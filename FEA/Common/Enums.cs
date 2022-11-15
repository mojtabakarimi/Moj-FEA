namespace FEA.Plate
{
    public enum SolverTypes
    {
        Default = 10,
        GaussEliminationSlyLine = 11,
        GaussEliminationBand = 12

    }


    public enum AnalysisTypes
    {
        Linear = 1001,
        Nonlinear = 1002
    }

    public enum ShearTypes
    {
        V13 = 10022,
        V23 = 10023
    }
}
