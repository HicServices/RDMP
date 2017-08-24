using System.Drawing;
using CatalogueLibrary.Data.Cohort;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class CohortIdentificationConfigurationStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Bitmap _cohortIdentificationConfiguration;
        private Bitmap _frozenCohortIdentificationConfiguration;

        public CohortIdentificationConfigurationStateBasedIconProvider()
        {
            _cohortIdentificationConfiguration = CatalogueIcons.CohortIdentificationConfiguration;
            _frozenCohortIdentificationConfiguration = CatalogueIcons.FrozenCohortIdentificationConfiguration;   
        }
        public Bitmap GetImageIfSupportedObject(object o)
        {
            var cic = o as  CohortIdentificationConfiguration;

            if (cic == null)
                return null;

            return cic.Frozen ? _frozenCohortIdentificationConfiguration : _cohortIdentificationConfiguration;

        }
    }
}