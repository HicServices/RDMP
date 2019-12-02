// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

namespace Rdmp.Core.CommandLine.Interactive
{
    /// <summary>
    /// autocomplete direction, e.g. when pressing up/down on console
    /// </summary>
    public enum CyclingDirections
    {
        /// <summary>
        /// Down the list of suggestions
        /// </summary>
        Forward,

        /// <summary>
        /// Back up the list of suggestions
        /// </summary>
        Backward
    }
}