using System.Data;
using CatalogueLibrary.Data;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Checks.SyntaxChecking
{
    public class FilterSyntaxChecker : SyntaxChecker
    {
        private readonly IFilter _filter;

        public FilterSyntaxChecker(IFilter filter)
        {
            _filter = filter;
        }

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