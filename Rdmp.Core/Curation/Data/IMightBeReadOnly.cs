using Rdmp.Core.Curation.Data.Cohort;

namespace Rdmp.Core.Curation.Data
{
    public interface IMightBeReadOnly
    {
        /// <summary>
        /// Returns true if changes to the container should be forbidden e.g. because the parent object is frozen (like <see cref="CohortIdentificationConfiguration.Frozen"/>)
        /// </summary>
        /// <returns></returns>
        bool ShouldBeReadOnly(out string reason);
    }
}