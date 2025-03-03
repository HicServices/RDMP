using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.Curation.Data.Datasets;

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
        return new IAtomicCommand[]
        {
            new ExecuteCommandCreateNewDatasetUI(_activator){
     OverrideCommandName="Add New Dataset", SuggestedCategory="Datasets"
 },
 new ExecuteCommandImportExistingPureDatasetUI(_activator)
 {
     OverrideCommandName="Import Existing Pure Dataset", SuggestedCategory="Pure Datasets"
 },
 new ExecuteCommandCreateNewPureConfigurationUI(_activator){
 OverrideCommandName="Create New Pure Configuration", SuggestedCategory="Pure Datasets"
 },
 new ExecuteCommandCreateNewHDRConfigurationUI(_activator){
 OverrideCommandName="Create New HDR Configuration", SuggestedCategory="HDR Integration"
 },
 new ExecuteCommandImportExistingHDRDatasetUI(_activator)
 {
     OverrideCommandName="Import Existing HDR Dataset", SuggestedCategory="HDR Integration"
 },
  new ExecuteCommandCreateNewJiraConfigurationUI(_activator){
 OverrideCommandName="Create New Jira Configuration", SuggestedCategory="Jira Integration"
 },
 new ExecuteCommandImportExistingJiraDatasetUI(_activator)
 {
     OverrideCommandName="Import Existing Jira Dataset", SuggestedCategory="Jira Integration"
 },
 new ExecuteCommandAddNewRegexRedactionConfigurationUI(_activator)
 {
     OverrideCommandName="Add New Regex Redaction Configuration", SuggestedCategory="Regex"
 }
        };
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