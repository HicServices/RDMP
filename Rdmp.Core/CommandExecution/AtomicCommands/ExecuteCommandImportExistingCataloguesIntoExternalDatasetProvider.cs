using NPOI.OpenXmlFormats.Spreadsheet;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Datasets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandImportExistingCataloguesIntoExternalDatasetProvider : BasicCommandExecution, IAtomicCommand
{

    private readonly IBasicActivateItems _activator;
    private readonly PluginDatasetProvider _provider;
    private readonly bool _includeExtractable;
    private readonly bool _includeInternal;
    private readonly bool _includeProjectSpecific;
    private readonly bool _includeDeprecated;

    public ExecuteCommandImportExistingCataloguesIntoExternalDatasetProvider(IBasicActivateItems activator, PluginDatasetProvider provider, bool includeExtractable, bool includeInternal, bool includeProjectSpecific, bool includeDeprecated)
    {
        _activator = activator;
        _provider = provider;
        _includeExtractable = includeExtractable;
        _includeInternal = includeInternal;
        _includeProjectSpecific = includeProjectSpecific;
        _includeDeprecated = includeDeprecated;
    }


    public override void Execute()
    {
        var catalogues = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>().ToList();
        if (!_includeInternal)
        {
            catalogues = catalogues.Where(c => !c.IsInternalDataset).ToList();
        }
        if (!_includeProjectSpecific)
        {
            catalogues = catalogues.Where(c => !c.IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository)).ToList();
        }
        if (!_includeDeprecated)
        {
            catalogues = catalogues.Where(c => !c.IsDeprecated).ToList();
        }
        if (!_includeExtractable)
        {
            catalogues = catalogues.Where(c => !c.GetExtractabilityStatus(_activator.RepositoryLocator.DataExportRepository).IsExtractable).ToList();
        }
        //todo check this catalogue filtering works
        foreach (var catalogue in catalogues)
        {
            var dataset = _provider.Create(catalogue);

            //jira
            if (_provider is JiraDatasetProvider)
            {
                var jds = (JiraDataset)dataset;
                var ds = _provider.AddExistingDatasetWithReturn(null, jds.id);
                var cmd = new ExecuteCommandLinkCatalogueToDataset(_activator, catalogue, ds);
                cmd.Execute();
            }
            if (_provider is HDRDatasetProvider)
            {
                var hdrds = (HDRDataset)dataset;
                var ds = _provider.AddExistingDatasetWithReturn(null, hdrds.data.id.ToString());
                var cmd = new ExecuteCommandLinkCatalogueToDataset(_activator, catalogue, ds);
                cmd.Execute();
            }
        }
    }
}
