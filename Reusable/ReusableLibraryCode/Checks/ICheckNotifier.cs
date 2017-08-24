namespace ReusableLibraryCode.Checks
{
    public interface ICheckNotifier
    {
        bool OnCheckPerformed(CheckEventArgs args);
    }
}