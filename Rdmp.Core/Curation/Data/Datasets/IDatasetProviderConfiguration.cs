using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.Data.Datasets;


/// <summary>
/// Remords configuration for accessing external dataset providers, such as PURE
/// </summary>
public interface IDatasetProviderConfiguration: IMapsDirectlyToDatabaseTable
{
    /// <summary>
    /// The assembly name of the provider this configuration is used for
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// A friendly name for the configuration
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The API url
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// Reference to which credentials should be used to access the API
    /// </summary>
    public int DataAccessCredentials_ID { get; }

    /// <summary>
    /// The organisation ID to use with the remote provider
    /// </summary>
    public string Organisation_ID { get; }


}
