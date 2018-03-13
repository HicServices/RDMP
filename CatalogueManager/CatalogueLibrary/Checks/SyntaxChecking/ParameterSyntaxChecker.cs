using CatalogueLibrary.Data;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Checks.SyntaxChecking
{
    public class ParameterSyntaxChecker : SyntaxChecker
    {
        private readonly ISqlParameter _parameter;

        public ParameterSyntaxChecker(ISqlParameter parameter)
        {
            _parameter = parameter;
        }

        public override void Check(ICheckNotifier notifier)
        {
            CheckSyntax(_parameter);
        }
    }
}