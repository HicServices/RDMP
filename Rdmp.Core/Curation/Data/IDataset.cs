using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data;

public interface IDataset
{
    /// <summary>
    /// Returns where the object exists (e.g. database) as <see cref="ICatalogueRepository"/> or null if the object does not exist in a catalogue repository.
    /// </summary>
    ICatalogueRepository CatalogueRepository { get; }

    string Name { get; }
    string DigitalObjectIdentifier { get; }
}
