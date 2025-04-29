using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using System.Linq;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.Curation.Data.Datasets;
using System;
using static Rdmp.Core.Curation.Data.Catalogue;

namespace Rdmp.UI.Collections;

public partial class ConfigurationsCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
{

    private IActivateItems _activator;

    public ConfigurationsCollectionUI()
    {
        InitializeComponent();
    }

    private IAtomicCommand[] GetWhitespaceRightClickMenu()
    {
        var datasetProviders = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(PluginDatasetProvider).IsAssignableFrom(p) && !p.IsAbstract);

        var options = new IAtomicCommand[]
        {
            new ExecuteCommandCreateNewDatasetUI(_activator){
                OverrideCommandName="Add New Dataset"
            },
            new ExecuteCommandAddNewRegexRedactionConfigurationUI(_activator)
            {
                OverrideCommandName="Add New Regex Redaction Configuration"
            },
            new ExecuteCommandCreateNewJiraConfigurationUI(_activator){
 OverrideCommandName="Create New Jira Configuration", SuggestedCategory="Jira Integration"
 },
            new ExecuteCommandImportExistingJiraDatasetUI(_activator)
 {
     OverrideCommandName="Import Existing Jira Dataset", SuggestedCategory="Jira Integration"
 },

        };
        foreach(var provider in datasetProviders)
        {
            options = options.Append(new ExecuteCommandAddNewDatasetProviderUI(_activator,provider)
            {
                OverrideCommandName = $"Add New {System.Text.RegularExpressions.Regex.Replace(provider.Name, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim()}",
                SuggestedCategory = "Dataset Provider Configurations"
            }).ToArray();
            options = options.Append(new ExecuteCommandAddNewDatasetUI(_activator, provider)
            {
                OverrideCommandName = $"Add Existing {System.Text.RegularExpressions.Regex.Replace(provider.Name, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim().Replace("Provider","")}",
                SuggestedCategory = "Datasets"
            }).ToArray();
        }

        return options;
    }

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);
        _activator = activator;
        CommonTreeFunctionality.SetUp(RDMPCollection.Configurations, tlvConfigurations, activator, olvName, olvName,
            new RDMPCollectionCommonFunctionalitySettings());
        CommonTreeFunctionality.WhitespaceRightClickMenuCommandsGetter = e => GetWhitespaceRightClickMenu();
        Activator.RefreshBus.EstablishLifetimeSubscription(this);
        tlvConfigurations.AddObject(Activator.CoreChildProvider.AllDatasetsNode);
        tlvConfigurations.AddObject(Activator.CoreChildProvider.AllDatasetProviderConfigurationsNode);
        tlvConfigurations.AddObject(Activator.CoreChildProvider.AllRegexRedactionConfigurationsNode);
        tlvConfigurations.Refresh();
    }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        switch (e.Object)
        {
            case Dataset:
                tlvConfigurations.RefreshObject(tlvConfigurations.Objects.OfType<AllDatasetsNode>());
                break;
            case RegexRedactionConfiguration:
                tlvConfigurations.RefreshObject(tlvConfigurations.Objects.OfType<AllRegexRedactionConfigurationsNode>());
                break;
        }
    }

    public static bool IsRootObject(object root) => root is AllDatasetsNode;
}