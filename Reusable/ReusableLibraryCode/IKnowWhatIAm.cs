namespace ReusableLibraryCode
{
    /// <summary>
    /// Interface for classes who do not want to rely on thier own xmldoc comments to tell the user what they are (see ExecuteCommandShowKeywordHelp)
    /// </summary>
    public interface IKnowWhatIAm
    {
        /// <summary>
        /// Return an alternative description of yourself (e.g. dependent on your state) that serves to describe the general purpose of your object.  
        /// This will be provided to consumers as an alternative to your class xmldoc (summary comments).
        /// </summary>
        /// <returns></returns>
        string WhatIsThis();
    }
}