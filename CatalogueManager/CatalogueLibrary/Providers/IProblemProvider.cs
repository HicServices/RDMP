namespace CatalogueLibrary.Providers
{
    /// <summary>
    /// Identifies problems with objects held in an ICoreChildProvider e.g. Projects missing cohorts, orphan ExtractionInformations etc.  This class
    /// differs from ICheckable etc because it is designed to identify and record a large number of problems very quickly among a large number of 
    /// objects and then later report about the problems e.g. when rendering a UI. 
    /// </summary>
    public interface IProblemProvider
    {
        /// <summary>
        /// Finds all the problems with all relevant objects known about by the child provider (Stored results are returned through
        /// HasProblem and DescribeProblem.
        /// </summary>
        /// <param name="childProvider"></param>
        void RefreshProblems(ICoreChildProvider childProvider);
        
        /// <summary>
        /// True if the supplied object has problems with it
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        bool HasProblem(object o);

        /// <summary>
        /// Returns the problem with object o or null if there are no problems
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        string DescribeProblem(object o);

    }
}