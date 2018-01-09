namespace ReusableLibraryCode
{
    /// <summary>
    /// Indicates that ToString alone is insufficient for finding this class and that additional text will be useful for distinguishing this object from others
    /// during search operations.
    /// </summary>
    public interface ICustomSearchString
    {
        /// <summary>
        /// Return the full string that should be used for free text search matching during user driven find operations instead of the ToString method.
        /// </summary>
        /// <returns>Search string to match tokens against instead of ToString</returns>
        string GetSearchString();
    }
}