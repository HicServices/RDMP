using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.Core.Curation.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.OpenXmlFormats.Vml.Office;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandImportExistingCataloguesIntoExternalDatasetProvider : BasicCommandExecution, IAtomicCommand
    {

        private readonly IBasicActivateItems _activator;
        private readonly IDatasetProvider _provider;
        private readonly bool _includeExtractable;
        private readonly bool _includeInternal;
        private readonly bool _includeProjectSpecific;
        private readonly bool _includeDeprecated;
        private readonly bool _autoUpdate;

        public ExecuteCommandImportExistingCataloguesIntoExternalDatasetProvider(IBasicActivateItems activator, IDatasetProvider provider, bool includeExtractable, bool includeInternal, bool includeProjectSpecific, bool includeDeprecated, bool autoUpdate)
        {
            _activator = activator;
            _provider = provider;
            _includeExtractable = includeExtractable;
            _includeInternal = includeInternal;
            _includeProjectSpecific = includeProjectSpecific;
            _includeDeprecated = includeDeprecated;
            _autoUpdate = autoUpdate;
        }

        public List<Catalogue> GetCatalogues()
        {
            var cataloguesToReturn = new List<Catalogue>();
            var catalogues = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>().ToList();
            if (_includeInternal)
            {
                cataloguesToReturn.AddRange(catalogues.Where(c => c.IsInternalDataset).ToList());
            }
            if (_includeProjectSpecific)
            {
                cataloguesToReturn.AddRange(catalogues.Where(c => c.IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository)).ToList());
            }
            if (_includeDeprecated)
            {
                cataloguesToReturn.AddRange(catalogues.Where(c => c.IsDeprecated).ToList());
            }
            if (_includeExtractable)
            {
                cataloguesToReturn.AddRange(catalogues.Where(c => c.GetExtractabilityStatus(_activator.RepositoryLocator.DataExportRepository).IsExtractable).ToList());
            }
            return cataloguesToReturn.Distinct().ToList();
        }


        public override void Execute()
        {
            var catalogues = GetCatalogues();
            foreach (var catalogue in catalogues)
            {
                var dataset = _provider.Create(catalogue);
                var ds = _provider.AddExistingDatasetWithReturn(null, dataset.GetID());
                var cmd = new ExecuteCommandLinkCatalogueToDataset(_activator, catalogue, ds,_autoUpdate);
                cmd.Execute();
            }
        }
    }
}
