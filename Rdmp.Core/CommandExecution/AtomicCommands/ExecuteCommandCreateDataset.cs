using NPOI.OpenXmlFormats.Spreadsheet;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Construction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

    public class ExecuteCommandCreateDataset :BasicCommandExecution
    {


    private string _doi;
    private string _name;
    private IBasicActivateItems _activator;
    public ExecuteCommandCreateDataset(IBasicActivateItems activator, string name, string doi = null): base(activator) {
        _activator = activator;
        _name = name;
        _doi = doi;
    }


    public override void Execute()
    {
        base.Execute();
        var dataset = new Dataset(BasicActivator.RepositoryLocator.CatalogueRepository, _name){ DigitalObjectIdentifier = _doi};
        dataset.SaveToDatabase();
    }
}