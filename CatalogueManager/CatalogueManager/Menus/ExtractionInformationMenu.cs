using System;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.PerformanceImprovement;
using CatalogueLibrary.FilterImporting.Construction;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using HIC.Logging;
using MapsDirectlyToDatabaseTable.Attributes;
using MapsDirectlyToDatabaseTableUI;
using RDMPStartup;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableLibraryCode.Proxies;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class ExtractionInformationMenu : RDMPContextMenuStrip
    {
        private readonly ExtractionInformation _extractionInformation;

        public ExtractionInformationMenu(RDMPContextMenuStripArgs args, ExtractionInformation extractionInformation)
            : base(args,extractionInformation)
        {
            _extractionInformation = extractionInformation;

            var addFilter = new ToolStripMenuItem("Add New Extraction Filter", _activator.CoreIconProvider.GetImage(RDMPConcept.Filter,OverlayKind.Add), (s, e) => AddFilter());
            Items.Add(addFilter);
        }

        [LoggingAspect]
        private void AddFilter()
        {
            var newFilter = new ExtractionFilterFactory(_extractionInformation).CreateNewFilter("New Filter " + Guid.NewGuid());
            Publish((DatabaseEntity) newFilter);
            Activate((DatabaseEntity) newFilter);
        }
    }
}
