namespace CatalogueLibrary.Providers
{
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