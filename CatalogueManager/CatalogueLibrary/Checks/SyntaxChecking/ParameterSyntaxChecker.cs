using CatalogueLibrary.Data;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Checks.SyntaxChecking
{
    /// <summary>
    /// Checks syntax validity of ISqlParameter
    /// </summary>
    public class ParameterSyntaxChecker : SyntaxChecker
    {
        private readonly ISqlParameter _parameter;

        /// <summary>
        /// Prepares the checker to check the ISqlParameter supplied
        /// </summary>
        /// <param name="parameter"></param>
        public ParameterSyntaxChecker(ISqlParameter parameter)
        {
            _parameter = parameter;
        }

        /// <summary>
        /// Checks to see if the syntax of char based parameters is valid (see CheckSyntax for more details)
        /// </summary>
        /// <param name="notifier"></param>
        public override void Check(ICheckNotifier notifier)
        {
            CheckSyntax(_parameter);
        }
    }
}