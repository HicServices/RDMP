using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary.Interfaces.Data.DataTables
{
    public interface IProjectCohortIdentificationConfigurationAssociation : IMasqueradeAs, IDeletableWithCustomMessage
    {
        /// <summary>
        /// The <see cref="IProject"/> to which the <see cref="CohortIdentificationConfiguration_ID"/> is associated with.
        /// </summary>
        int Project_ID { get; set; }

        /// <summary>
        /// The <see cref="CohortIdentificationConfiguration"/> which is associated with the given <see cref="Project_ID"/>.
        /// </summary>
        int CohortIdentificationConfiguration_ID { get; set; }

        /// <inheritdoc cref="Project_ID"/>
        [NoMappingToDatabase]
        IProject Project { get; }

        /// <inheritdoc cref="CohortIdentificationConfiguration_ID"/>
        [NoMappingToDatabase]
        CohortIdentificationConfiguration CohortIdentificationConfiguration { get; }

        /// <summary>
        /// Returns the <see cref="CohortIdentificationConfiguration"/> referenced by <see cref="CohortIdentificationConfiguration_ID"/> or null if it is an orphan
        /// </summary>
        /// <returns></returns>
        CohortIdentificationConfiguration GetCohortIdentificationConfigurationCached();
    }
}