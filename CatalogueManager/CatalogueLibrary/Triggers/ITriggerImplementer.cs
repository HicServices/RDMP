using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Triggers
{
    public interface ITriggerImplementer
    {

        void DropTrigger(out string problemsDroppingTrigger, out string thingsThatWorkedDroppingTrigger);
        void CreateTrigger(ICheckNotifier notifier, int createArchiveIndexTimeout = 30);
        TriggerStatus GetTriggerStatus();
        bool CheckUpdateTriggerIsEnabledAndHasExpectedBody();
    }
}