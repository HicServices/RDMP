using System;

namespace ReusableLibraryCode.Checks
{
    public class DoesNotThrowCheckable : ICheckable
    {
        private Action _action;

        public DoesNotThrowCheckable(Action action)
        {
            _action = action;
        }

        public void Check(ICheckNotifier notifier)
        {
            try
            {
                _action.Invoke();
            }
            catch (Exception ex)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Method crashed", CheckResult.Fail,ex));
            }

            notifier.OnCheckPerformed(new CheckEventArgs("Succesfuly executed method without crashing", CheckResult.Success));
        }
    }
}