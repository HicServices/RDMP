using System.Data;
using CatalogueLibrary.Data;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Checks.SyntaxChecking
{
    /// <summary>
    /// Checks bracket parity of where SQL of IFilter and syntax validity of parameters which are char based 
    /// </summary>
    public class FilterSyntaxChecker : SyntaxChecker
    {
        private readonly IFilter _filter;

        public FilterSyntaxChecker(IFilter filter)
        {
            _filter = filter;
        }

        /// <summary>
        /// Checks to see if the WhereSQL contains a closing bracket for every opening bracket (see ParityCheckCharacterPairs for more detail) and also checks the syntax validity of each parameter if it is char based (see CheckSyntax for more detail)
        /// </summary>
        /// <param name="notifier"></param>
        public override void Check(ICheckNotifier notifier)
        {
            try
            {
                ParityCheckCharacterPairs(new[] { '(', '[' }, new[] { ')', ']' }, _filter.WhereSQL);
            }
            catch (SyntaxErrorException exception)
            {
                throw new SyntaxErrorException("Failed to validate the bracket parity of filter " + _filter, exception);
            }

            foreach (ISqlParameter parameter in _filter.GetAllParameters())
                CheckSyntax(parameter);
        }
    }
}