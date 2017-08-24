using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.Job;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.Attachers
{
    public interface IAttacher: IDisposeAfterDataLoad, ICheckable
    {
        ExitCodeType Attach(IDataLoadJob job);
        void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo);
        
        IHICProjectDirectory HICProjectDirectory { get; set; }
        bool RequestsExternalDatabaseCreation { get; }
    }
}