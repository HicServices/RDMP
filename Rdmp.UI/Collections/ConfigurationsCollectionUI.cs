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
using System.Text;

namespace Rdmp.UI.Collections;

public partial class ConfigurationsCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
{

    private IActivateItems _activator;

    public ConfigurationsCollectionUI()
    {
        InitializeComponent();
    }

    private static string FormatPascalAndAcronym(string input)
    {
        var builder = new StringBuilder(input[0].ToString());
        if (builder.Length > 0)
        {
            for (var index = 1; index < input.Length; index++)
            {
                char prevChar = input[index - 1];
                char nextChar = index + 1 < input.Length ? input[index + 1] : '\0';

                bool isNextLower = Char.IsLower(nextChar);
                bool isNextUpper = Char.IsUpper(nextChar);
                bool isPresentUpper = Char.IsUpper(input[index]);
                bool isPrevLower = Char.IsLower(prevChar);
                bool isPrevUpper = Char.IsUpper(prevChar);

                if (!string.IsNullOrWhiteSpace(prevChar.ToString()) &&
                    ((isPrevUpper && isPresentUpper && isNextLower) ||
                    (isPrevLower && isPresentUpper && isNextLower) ||
                    (isPrevLower && isPresentUpper && isNextUpper)))
                {
                    builder.Append(' ');
                    builder.Append(input[index]);
                }
                else
                {
                    builder.Append(input[index]);
                }
            }
        }
        return builder.ToString();
    }

    private IAtomicCommand[] GetWhitespaceRightClickMenu()
    {
        var datasetProviders = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes()).Where(p => typeof(PluginDatasetProvider).IsAssignableFrom(p) && !p.IsAbstract);

        var options = new IAtomicCommand[]
        {
            new ExecuteCommandAddNewRegexRedactionConfigurationUI(_activator)
            {
                OverrideCommandName="Add New Regex Redaction Configuration"
            }
        };
        foreach (var provider in datasetProviders)
        {
            options = options.Append(new ExecuteCommandAddNewDatasetProviderUI(_activator, provider)
            {
                OverrideCommandName = $"Add New {FormatPascalAndAcronym(provider.Name).Trim()}",
                SuggestedCategory = "Dataset Provider Configurations"
            }).ToArray();
            options = options.Append(new ExecuteCommandAddNewDatasetUI(_activator, provider)
            {
                OverrideCommandName = $"Add Existing {FormatPascalAndAcronym(provider.Name).Trim().Replace("Provider", "")}",
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