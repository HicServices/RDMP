using Rdmp.Core.Curation.Data;
using System;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateDataset : BasicCommandExecution, IAtomicCommand
{
    private readonly string _doi;
    private readonly string _name;
    private readonly string _source;
    private readonly IBasicActivateItems _activator;

    public ExecuteCommandCreateDataset(IBasicActivateItems activator, [DemandsInitialization("The name of the dataset")]string name, string doi = null,string source = null) : base(activator)
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