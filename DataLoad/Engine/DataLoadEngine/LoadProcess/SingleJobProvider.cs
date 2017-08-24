using DataLoadEngine.Job;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadProcess
{
    public class SingleJobProvider : IJobFactory
    {
        private readonly IJobFactory _jobFactory;
        private IDataLoadJob _jobToReturn;

        public SingleJobProvider(IJobFactory jobFactory)
        {
            _jobFactory = jobFactory;
            _jobToReturn = null;
        }

        // Only ever returns one job
        public IDataLoadJob Create(IDataLoadEventListener listener)
        {
            if (_jobToReturn == null)
            {
                _jobToReturn = _jobFactory.Create(listener);
                return _jobToReturn;
            }

            return null;
        }
    }
}