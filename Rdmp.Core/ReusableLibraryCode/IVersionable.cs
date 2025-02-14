using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.ReusableLibraryCode;

internal interface IVersionable
{

    /// <summary>
    /// Interface to provide the SaveNewVersion function 
    /// </summary>
    /// <returns>DatabaseEntity</returns>
    DatabaseEntity SaveNewVersion();
}
