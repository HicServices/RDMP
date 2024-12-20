﻿using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;

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
                OverrideCommandName="Add New Dataset"
            },
            new ExecuteCommandAddNewRegexRedactionConfigurationUI(_activator)
            {
                OverrideCommandName="Add New Regex Redaction Configuration"
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