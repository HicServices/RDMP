// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.Sharing;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.Menus.MenuItems;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Rdmp.UI.Menus;

[System.ComponentModel.DesignerCategory("")]
internal class CatalogueMenu : RDMPContextMenuStrip
{
    private const string CatalogueItems = "Catalogue Items";

    public CatalogueMenu(RDMPContextMenuStripArgs args, Catalogue catalogue) : base(args, catalogue)
    {
        var isApiCall = catalogue.IsApiCall();
        Add(new ExecuteCommandLinkCatalogueToDatasetUI(_activator, catalogue)
        {
            OverrideCommandName = "Link Catalogue to Dataset"
        });
        Add(new ExecuteCommandGenerateMetadataReport(_activator, catalogue)
        {
            Weight = -99.059f
        }, Keys.None, AtomicCommandFactory.Metadata);

        Add(new ExecuteCommandImportCatalogueDescriptionsFromShare(_activator, catalogue)
        {
            Weight = -95.09f
        }, Keys.None, AtomicCommandFactory.Metadata);


        Add(new ExecuteCommandExportInDublinCoreFormat(_activator, catalogue)
        {
            Weight = -90.10f
        }, Keys.None, AtomicCommandFactory.Metadata);
        Add(new ExecuteCommandImportDublinCoreFormat(_activator, catalogue)
        {
            Weight = -90.09f
        }, Keys.None, AtomicCommandFactory.Metadata);

        Add(new ExecuteCommandAddNewLookupTableRelationship(_activator, catalogue, null)
        {
            OverrideCommandName = "New Lookup Table Relationship",
            Weight = -86.9f
        }, Keys.None, AtomicCommandFactory.Add);

        if (!isApiCall)
        {
            Items.Add(new DQEMenuItem(_activator, catalogue));

            //create right click context menu
            Add(new ExecuteCommandViewCatalogueExtractionSqlUI(_activator)
            {
                Weight = -99.001f,
                OverrideCommandName = "Catalogue Extraction SQL",
                SuggestedCategory = AtomicCommandFactory.View
            }.SetTarget(catalogue));
        }

        ////////////////// UI Commands for the CatalogueItems submenu of the Catalogue context menu ///////////////////
        Add(new ExecuteCommandBulkProcessCatalogueItems(_activator, catalogue)
        { SuggestedCategory = CatalogueItems, Weight = -99.049f });
        Add(new ExecuteCommandUpdateCatalogueDataLocationUI(_activator, catalogue)
        { SuggestedCategory = CatalogueItems, Weight = -99.049f, OverrideCommandName = "Update Catalogue Data Location" });
        Add(new ExecuteCommandPasteClipboardAsNewCatalogueItems(_activator, catalogue, Clipboard.GetText)
        { SuggestedCategory = CatalogueItems, Weight = -99.047f });
        Add(new ExecuteCommandReOrderColumns(_activator, catalogue)
        { SuggestedCategory = CatalogueItems, Weight = -99.046f });
        Add(new ExecuteCommandRegexRedaction(_activator, catalogue)
        { SuggestedCategory = CatalogueItems, Weight = -99.046f, OverrideCommandName = "Regex Redactions" });
        Add(new ExecuteCommandGuessAssociatedColumns(_activator, catalogue, null)
        { SuggestedCategory = CatalogueItems, Weight = -99.045f, PromptForPartialMatching = true });
        Add(new ExecuteCommandChangeExtractionCategory(_activator,
                catalogue.GetAllExtractionInformation(ExtractionCategory.Any))
        { SuggestedCategory = CatalogueItems, Weight = -99.044f });
        Add(new ExecuteCommandImportCatalogueItemDescriptions(_activator, catalogue, null /*pick at runtime*/)
        { SuggestedCategory = CatalogueItems, Weight = -99.043f });


        var links = _activator.CoreChildProvider.AllLoadMetadataCatalogueLinkages.Where(lmdcl => lmdcl.CatalogueID == catalogue.ID).Select(lmdcl => lmdcl.LoadMetadataID);
        if (!links.Any())
        {
            foreach (var lmd in _activator.CoreChildProvider.AllLoadMetadatas.Where(lmd => links.Contains(lmd.ID)))
            {
                if (lmd.GetRootDirectory() == null) return;
                try
                {
                    var dirReal = lmd.GetRootDirectory();
                    Add(new ExecuteCommandOpenInExplorer(_activator, dirReal)
                    { OverrideCommandName = "Open Load Directory" });
                }
                catch (Exception)
                {
                    // if the directory name is bad or corrupt
                }
            }
        }
    }
}