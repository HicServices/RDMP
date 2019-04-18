// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs.NavigateTo;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Providers;
using DataExportLibrary.Providers.Nodes.ProjectCohortNodes;
using DataExportManager.CommandExecution.AtomicCommands;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Settings;

namespace DataExportManager.Collections
{
    /// <summary>
    /// Contains a list of all the currently configured data export projects you have.  A data export Project is a collection of one or more datasets combined with a cohort (or multiple
    /// if you have sub ExtractionConfigurations within the same Project e.g. cases/controls).
    /// 
    /// <para>Data in these datasets will be linked against the cohort and anonymised on extraction (to flat files / database etc).</para>
    /// </summary>
    public partial class DataExportCollectionUI : RDMPCollectionUI
    {
        private IActivateItems _activator;

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
            _activator = activator;

            CommonTreeFunctionality.SetUp(
                RDMPCollection.DataExport, 
                tlvDataExport,
                _activator,
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
            
            NavigateToObjectUI.RecordThatTypeIsNotAUsefulParentToShow(typeof(ProjectCohortIdentificationConfigurationAssociationsNode));
        }
        
        public static bool IsRootObject(object root)
        {
            return root is Project || root is ExtractableDataSetPackage;
        }
    }

    
}
