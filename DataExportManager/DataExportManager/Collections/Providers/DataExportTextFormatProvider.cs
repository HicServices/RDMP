using System.Drawing;
using System.Linq;
using DataExportLibrary.Data.DataTables;
using DataExportManager.Collections.Nodes;

namespace DataExportManager.Collections.Providers
{
    internal class DataExportTextFormatProvider
    {
        private readonly DataExportChildProvider _childProvider;
        private readonly DataExportProblemProvider _problemProvider;

        public DataExportTextFormatProvider(DataExportChildProvider childProvider, DataExportProblemProvider problemProvider)
        {
            _childProvider = childProvider;
            _problemProvider = problemProvider;
        }

        public Color GetForeColor(Project p)
        {
            if (_problemProvider.HasProblems(p))
                return Color.OrangeRed;

            if (!_childProvider.GetActiveConfigurationsOnly(p).Any())
                return Color.LightGray;

            return Color.Black;
        }

        public Color GetForeColor(ExtractionConfiguration config)
        {
            if (_problemProvider.HasProblems(config))
                return Color.OrangeRed;

            if (config.IsReleased)
                return Color.LightGray;

            return Color.Black;
        }

        public Color GetForeColor(LinkedCohortNode cohort)
        {
            //todo check for problems with cohort usage e.g. project number mismatch

            //return forecolor of the configuration
            return GetForeColor(cohort.Configuration);
        }
    }
}