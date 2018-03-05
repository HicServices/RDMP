
namespace ReusableLibraryCode.Checks
{
    //important to keep these in order of severity from least sever to most severe so > opeartions can be applied to Enum
    public enum CheckResult
    {
        Success,
        Warning,
        Fail
    };

    /// <summary>
    /// An object that can check it's own state for problems and summarise this through the Checking Events system (See CheckEventArgs)
    /// </summary>
    public interface ICheckable
    {
        /// <summary>
        /// Use the OnCheckPerformed method on the notifier to inform of all the things you are checking and the results, possible fixes and severity
        /// </summary>
        /// <param name="notifier">The manager that will receive your messages about problems/fixes and decide how/if to present them to the user</param>
        void Check(ICheckNotifier notifier);
    }
}
