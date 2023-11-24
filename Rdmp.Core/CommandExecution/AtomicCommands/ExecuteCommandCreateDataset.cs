using System;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateDataset : BasicCommandExecution, IAtomicCommand
{


    private string _doi;
    private string _name;
    private string _source;
    IBasicActivateItems _activator;
    public ExecuteCommandCreateDataset(IBasicActivateItems basicActivator) : base(basicActivator)
    {
        _activator = basicActivator;
    }

    public ExecuteCommandCreateDataset(IBasicActivateItems activator, string name, string doi = null,string source = null) : base(activator)
    {
        _name = name;
        _doi = doi;
        _source = source;
        _activator = activator;
    }


    public override void Execute()
    {
        if (string.IsNullOrWhiteSpace(_name))
        {
            throw new Exception("Datasets requires a name");
        }
        base.Execute();
        var dataset = new Curation.Data.Dataset(BasicActivator.RepositoryLocator.CatalogueRepository, _name) { DigitalObjectIdentifier = _doi, Source = _source };
        dataset.SaveToDatabase();
        _activator.Publish(dataset);

    }
}