using System.Collections.Generic;
using ReusableLibraryCode.Checks;

namespace DataLoadEngine.Checks
{
    public class CheckGroup: ICheckable
    {
        private readonly IEnumerable<ICheckable> _checks;
        public string GroupName { get; set; }
        public string SuccessMessage { get; private set; }

        public CheckGroup(string groupName, IEnumerable<ICheckable> checks, string successMessage)
        {
            _checks = checks;
            GroupName = groupName;
            SuccessMessage = successMessage;
        }

        

        public void Check(ICheckNotifier notifier)
        {
            foreach (var check in _checks)
                check.Check(notifier);
        }
    }
}