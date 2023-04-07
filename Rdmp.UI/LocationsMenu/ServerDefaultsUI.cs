// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Databases;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.UI.Versioning;


namespace Rdmp.UI.LocationsMenu;

/// <summary>
/// The RDMP Data Catalogue database is the central resource for storing all information about what is where, what datasets there are, what servers they are on etc.  This includes 
/// keeping track of the locations of other servers such as the Logging server/database, Data Quality Engine reporting database, anonymisation databases, query caching databases
/// etc. 
/// 
/// <para>This dialog lets you set which server references (ExternalDatabaseServer) are used for each of the defaults that RDMP has (e.g. which logging server should be used by default)</para>
///  
/// </summary>
public partial class ServerDefaultsUI : RDMPForm
{
    IServerDefaults defaults;
        
    public ServerDefaultsUI(IActivateItems activator):base(activator)
    {
        InitializeComponent();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (VisualStudioDesignMode)
            return;

        RefreshUIFromDatabase();


    }

    private void RefreshUIFromDatabase()
    {
        try
        {
            defaults = Activator.RepositoryLocator.CatalogueRepository;

            var allServers = Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<ExternalDatabaseServer>().ToArray();
                
            InitializeServerDropdown(ddDefaultLoggingServer, PermissableDefaults.LiveLoggingServer_ID, allServers);
            InitializeServerDropdown(ddDQEServer, PermissableDefaults.DQE, allServers);
            InitializeServerDropdown(ddWebServiceQueryCacheServer, PermissableDefaults.WebServiceQueryCachingServer_ID, allServers);
            InitializeServerDropdown(ddCohortIdentificationQueryCacheServer, PermissableDefaults.CohortIdentificationQueryCachingServer_ID, allServers);
            InitializeServerDropdown(ddDefaultIdentifierDump, PermissableDefaults.IdentifierDumpServer_ID, allServers);
            InitializeServerDropdown(ddOverrideRawServer, PermissableDefaults.RAWDataLoadServer, allServers);
            InitializeServerDropdown(ddDefaultANOStore, PermissableDefaults.ANOStore, allServers);

            btnCreateNewDQEServer.Enabled = ddDQEServer.SelectedItem == null;
            btnClearDQEServer.Enabled = ddDQEServer.SelectedItem != null;

            btnCreateNewWebServiceQueryCache.Enabled = ddWebServiceQueryCacheServer.SelectedItem == null;
            btnClearWebServiceQueryCache.Enabled = ddWebServiceQueryCacheServer.SelectedItem != null;

            btnCreateNewCohortIdentificationQueryCache.Enabled = ddCohortIdentificationQueryCacheServer.SelectedItem == null;
            btnClearCohortIdentificationQueryCache.Enabled = ddCohortIdentificationQueryCacheServer.SelectedItem !=null;
                
        }
        catch (Exception ex)
        {
            ExceptionViewer.Show(ex);
        }
    }

    private void InitializeServerDropdown(ComboBox comboBox, PermissableDefaults permissableDefault, ExternalDatabaseServer[] allServers)
    {
        comboBox.Items.Clear();

        var currentDefault = defaults.GetDefaultFor(permissableDefault);
        var patcher = permissableDefault.ToTier2DatabaseType();
            
        var toAdd = allServers;
            
        if(patcher != null) //we expect an explicit type e.g. a HIC.Logging.Database 
        {
            var compatibles = Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<ExternalDatabaseServer>().Where(s=>s.WasCreatedBy(patcher)).ToArray();

            if (currentDefault == null || compatibles.Contains(currentDefault))//if there is not yet a default or the existing default is of the correct type
                toAdd = compatibles;//then we can go ahead and use the restricted type

            //otherwise what we have is a default of the wrong server type! eep.
        }

        comboBox.Items.AddRange(toAdd);

        //select the server
        if (currentDefault != null)
            comboBox.SelectedItem = comboBox.Items.Cast<ExternalDatabaseServer>().Single(s => s.ID == currentDefault.ID);
    }

        
    private void ddDefault_SelectedIndexChanged(object sender, EventArgs e)
    {
        PermissableDefaults toChange;

        if(sender == ddDefaultIdentifierDump)
            toChange = PermissableDefaults.IdentifierDumpServer_ID;
        else
        if (sender == ddDefaultLoggingServer)
            toChange = PermissableDefaults.LiveLoggingServer_ID;
        else if(sender == ddOverrideRawServer)
            toChange = PermissableDefaults.RAWDataLoadServer;
        else if (sender == ddDefaultANOStore)
            toChange = PermissableDefaults.ANOStore;
        else if (sender == ddWebServiceQueryCacheServer)
            toChange = PermissableDefaults.WebServiceQueryCachingServer_ID;
        else if (sender == ddCohortIdentificationQueryCacheServer)
            toChange = PermissableDefaults.CohortIdentificationQueryCachingServer_ID;
        else
            throw new Exception("Did not recognise sender:" + sender);

        var selectedItem = ((ComboBox) sender).SelectedItem as ExternalDatabaseServer;

        //user selected nothing
        if(selectedItem == null)
            return;

        defaults.SetDefault(toChange, selectedItem);
    }

    private void btnClearServer_Click(object sender, EventArgs e)
    {
        PermissableDefaults toClear;

        if(sender == btnClearLoggingServer)
        {
            toClear = PermissableDefaults.LiveLoggingServer_ID;
            ddDefaultLoggingServer.SelectedItem = null;
        }
        else
        if(sender == btnClearIdentifierDump)
        {
            toClear = PermissableDefaults.IdentifierDumpServer_ID;
            ddDefaultIdentifierDump.SelectedItem = null;
        }
        else if (sender == btnClearDQEServer)
        {
            toClear = PermissableDefaults.DQE;
            ddDQEServer.SelectedItem = null;

        }
        else if (sender == btnClearRAWServer)
        {
            toClear = PermissableDefaults.RAWDataLoadServer;
            ddOverrideRawServer.SelectedItem = null;
        }
        else if (sender == btnClearANOStore)
        {
            toClear = PermissableDefaults.ANOStore;
            ddDefaultANOStore.SelectedItem = null;
        }
        else if (sender == btnClearWebServiceQueryCache)
        {
            toClear = PermissableDefaults.WebServiceQueryCachingServer_ID;
            ddWebServiceQueryCacheServer.SelectedItem = null;
        }
        else if (sender == btnClearCohortIdentificationQueryCache)
        {
            toClear = PermissableDefaults.CohortIdentificationQueryCachingServer_ID;
            ddCohortIdentificationQueryCacheServer.SelectedItem = null;
        }
        else
            throw new Exception("Did not recognise sender:" + sender);

        defaults.ClearDefault(toClear);
        RefreshUIFromDatabase();
    }
        
    private void CreateNewExternalServer(PermissableDefaults defaultToSet, IPatcher patcher)
    {
        if (CreatePlatformDatabase.CreateNewExternalServer(Activator.RepositoryLocator.CatalogueRepository, defaultToSet, patcher) != null)
            RefreshUIFromDatabase();
    }


    private void ddDQEServer_SelectedIndexChanged(object sender, EventArgs e)
    {
        if(ddDQEServer.SelectedItem != null)
            defaults.SetDefault(PermissableDefaults.DQE, (ExternalDatabaseServer) ddDQEServer.SelectedItem);
    }

    private void btnCreateNewDQEServer_Click(object sender, EventArgs e)
    {
        CreateNewExternalServer(PermissableDefaults.DQE, new DataQualityEnginePatcher());
    }
    private void btnCreateNewWebServiceQueryCache_Click(object sender, EventArgs e)
    {
        CreateNewExternalServer(PermissableDefaults.WebServiceQueryCachingServer_ID, new QueryCachingPatcher());
    }

    private void btnCreateNewLoggingServer_Click(object sender, EventArgs e)
    {
        CreateNewExternalServer(PermissableDefaults.LiveLoggingServer_ID, new LoggingDatabasePatcher());
    }
        
    private void btnCreateNewIdentifierDump_Click(object sender, EventArgs e)
    {
        CreateNewExternalServer(PermissableDefaults.IdentifierDumpServer_ID, new IdentifierDumpDatabasePatcher());
    }

    private void btnCreateNewANOStore_Click(object sender, EventArgs e)
    {
        CreateNewExternalServer(PermissableDefaults.ANOStore, new ANOStorePatcher());
    }

    private void btnCreateNewCohortIdentificationQueryCache_Click(object sender, EventArgs e)
    {
        CreateNewExternalServer(PermissableDefaults.CohortIdentificationQueryCachingServer_ID, new QueryCachingPatcher());
    }


}