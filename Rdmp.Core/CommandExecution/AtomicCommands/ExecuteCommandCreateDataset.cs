using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

public class ExecuteCommandCreateDataset : BasicCommandExecution
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

        if (string.IsNullOrWhiteSpace(_name))
            SetImpossible("Datasets require a name");
    }


    public override void Execute()
    {
        base.Execute();
        var dataset = new Curation.Data.Datasets.Dataset(BasicActivator.RepositoryLocator.CatalogueRepository, _name) { DigitalObjectIdentifier = _doi, Source = _source };
        dataset.SaveToDatabase();
        _activator.Publish(dataset);
    }
}