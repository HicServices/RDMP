using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Job
{
    public interface IJobFactory
    {
        IDataLoadJob Create(IDataLoadEventListener listener);
    }
}