using System;
using CatalogueLibrary.Data;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs.NavigateTo;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using DataExportLibrary.Providers;
using DataExportLibrary.Providers.Nodes.ProjectCohortNodes;
using DataExportManager.CommandExecution.AtomicCommands;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

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

            olvCohortSource.IsEditable = false;
            olvCohortSource.AspectGetter = CohortSourceAspectGetter;

            olvCohortVersion.IsEditable = false;
            olvCohortVersion.AspectGetter = CohortVersionAspectGetter;
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

            CommonFunctionality.SetUp(
                RDMPCollection.DataExport, 
                tlvDataExport,
                _activator,
                olvName,
                olvName
                );

            CommonFunctionality.WhitespaceRightClickMenuCommandsGetter =(a)=> new IAtomicCommand[]
            {
                new ExecuteCommandCreateNewDataExtractionProject(a),
                new ExecuteCommandCreateNewExtractableDataSetPackage(a)
            };
            
            CommonFunctionality.MaintainRootObjects = new Type[]{typeof(ExtractableDataSetPackage),typeof(Project)};

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
