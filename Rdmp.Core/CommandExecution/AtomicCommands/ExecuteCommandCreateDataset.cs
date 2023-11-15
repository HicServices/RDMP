using NPOI.OpenXmlFormats.Spreadsheet;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories.Construction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateDataset : BasicCommandExecution
{


    private string _doi;
    private string _name;
    private string _source;

    public ExecuteCommandCreateDataset(IBasicActivateItems basicActivator) : base(basicActivator)
    {
    }

    public ExecuteCommandCreateDataset(IBasicActivateItems activator, string name, string doi = null,string source = null) : base(activator)
    {
        _name = name;
        _doi = doi;
        _source = source;
    }


    public override void Execute()
    {
        base.Execute();
        var dataset = new Curation.Data.Dataset(BasicActivator.RepositoryLocator.CatalogueRepository, _name) { DigitalObjectIdentifier = _doi, Source = _source };
        dataset.SaveToDatabase();
    }
}