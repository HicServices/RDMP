using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data.DataLoad
{
    public interface  ILoadMetadataCatalogueLinkage: IMapsDirectlyToDatabaseTable
    {
        /// <summary>
        /// Returns where the object exists (e.g. database) as <see cref="ICatalogueRepository"/> or null if the object does not exist in a catalogue repository.
        /// </summary>
        ICatalogueRepository CatalogueRepository { get; }

        int LoadMetatdataID{ get; }
        int CatalogueID { get; }

    }
}
