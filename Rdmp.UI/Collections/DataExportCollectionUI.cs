// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.Core.Providers.Nodes.ProjectCohortNodes;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.AtomicCommands.UIFactory;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleDialogs.NavigateTo;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableLibraryCode.Settings;

namespace Rdmp.UI.Collections
{
    /// <summary>
    /// Contains a list of all the currently configured data export projects you have.  A data export Project is a collection of one or more datasets combined with a cohort (or multiple
    /// if you have sub ExtractionConfigurations within the same Project e.g. cases/controls).
    /// 
    /// <para>Data in these datasets will be linked against the cohort and anonymised on extraction (to flat files / database etc).</para>
    /// </summary>
    public partial class DataExportCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
    {
        public DataExportCollectionUI()
        {
            InitializeComponent();


            olvProjectNumber.IsEditable = false;
            olvProjectNumber.AspectGetter = ProjectNumberAspectGetter;
            olvProjectNumber.IsVisible = UserSettings.ShowColumnProjectNumber;
            olvProjectNumber.VisibilityChanged += (s, e) => UserSettings.ShowColumnProjectNumber = ((OLVColumn) s).IsVisible;

            olvCohortSource.IsEditable = false;
            olvCohortSource.AspectGetter = CohortSourceAspectGetter;
            olvCohortSource.IsVisible = UserSettings.ShowColumnCohortSource;
            olvCohortSource.VisibilityChanged += (s, e) => UserSettings.ShowColumnCohortSource = ((OLVColumn)s).IsVisible;

            olvCohortVersion.IsEditable = false;
            olvCohortVersion.AspectGetter = CohortVersionAspectGetter;
            olvCohortVersion.IsVisible = UserSettings.ShowColumnCohortVersion;
            olvCohortVersion.VisibilityChanged += (s, e) => UserSettings.ShowColumnCohortVersion = ((OLVColumn)s).IsVisible;
        }

        private object CohortSourceAspectGetter(object rowObject)
        {
            //if it is a cohort or something masquerading as a cohort
            var masquerader = rowObject as IMasqueradeAs;
            var cohort = masquerader != null
                ? masquerader.MasqueradingAs() as ExtractableCohort
                : rowObject as ExtractableCohort;

            //serve up the ExternalCohortTable name
            if (cohort != null)
                return cohort.ExternalCohortTable.Name;

            return null;
        }

        private object ProjectNumberAspectGetter(object rowObject)
        {
            var p = rowObject as Project;
            
            var masquerade = rowObject as IMasqueradeAs;

            if (p != null)
                return p.ProjectNumber;

            if(masquerade != null)
            {
                var c = masquerade.MasqueradingAs() as ExtractableCohort;
                if (c != null)
                    return c.ExternalProjectNumber;
            }

            return null;
        }

        private object CohortVersionAspectGetter(object rowObject)
        {
            var masquerade = rowObject as IMasqueradeAs;

            if (masquerade != null)
            {
                var c = masquerade.MasqueradingAs() as ExtractableCohort;
                if (c != null)
                    return c.ExternalVersion;
            }

            return null;
        }

        public override void SetItemActivator(IActivateItems activator)
        {
            base.SetItemActivator(activator);

            CommonTreeFunctionality.SetUp(
                RDMPCollection.DataExport, 
                tlvDataExport,
                Activator,
                olvName,
                olvName
                );

            CommonTreeFunctionality.WhitespaceRightClickMenuCommandsGetter =(a)=> new IAtomicCommand[]
            {
                new ExecuteCommandCreateNewDataExtractionProject(a),
                new ExecuteCommandCreateNewExtractableDataSetPackage(a)
            };
            
            CommonTreeFunctionality.MaintainRootObjects = new Type[]{typeof(ExtractableDataSetPackage),typeof(Project)};

            var dataExportChildProvider = activator.CoreChildProvider as DataExportChildProvider;

            if(dataExportChildProvider != null)
            {
                tlvDataExport.AddObjects(dataExportChildProvider.AllPackages);
                tlvDataExport.AddObjects(dataExportChildProvider.Projects);
            }

            SetupToolStrip();

            Activator.RefreshBus.EstablishLifetimeSubscription(this);

        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            if (Activator.CoreChildProvider is DataExportChildProvider dataExportChildProvider)
            {
                // remove packages and projects which don't exist any more according to child provider
                tlvDataExport.RemoveObjects(tlvDataExport.Objects.OfType<ExtractableDataSetPackage>().Except(dataExportChildProvider.AllPackages).ToArray());
                tlvDataExport.RemoveObjects(tlvDataExport.Objects.OfType<Project>().Except(dataExportChildProvider.Projects).ToArray());
            }

            SetupToolStrip();
        }

        private void SetupToolStrip()
        {
            CommonFunctionality.ClearToolStrip();
            CommonFunctionality.Add(new ExecuteCommandCreateNewDataExtractionProject(Activator),"Project",null,"New...");
            CommonFunctionality.Add(new ExecuteCommandCreateNewExtractionConfigurationForProject(Activator),"Extraction",null,"New...");
            CommonFunctionality.Add(new ExecuteCommandCreateNewExtractableDataSetPackage(Activator),"Package",null,"New...");
            var mi = new ToolStripMenuItem("Project Specific Catalogue",Activator.CoreIconProvider.GetImage(RDMPConcept.ProjectCatalogue,OverlayKind.Add));

            var factory = new AtomicCommandUIFactory(Activator);
            mi.DropDownItems.Add(factory.CreateMenuItem(new ExecuteCommandCreateNewCatalogueByImportingFileUI(Activator)
            {
                OverrideCommandName = "From File...",
                PromptForProject = true
            }));

            mi.DropDownItems.Add(factory.CreateMenuItem(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(Activator)
            {
                OverrideCommandName = "From Database...",
                PromptForProject = true
            }));

            CommonFunctionality.Add(mi,"New...");
        }
        
        public static bool IsRootObject(object root)
        {
            return root is Project || root is ExtractableDataSetPackage;
        }
    }

    
}
