using DataLoadEngine.Job;

namespace DataLoadEngine.LoadExecution.Delegates
{
    public delegate void JobFinishedHandler(object sender, IDataLoadJob job);
}