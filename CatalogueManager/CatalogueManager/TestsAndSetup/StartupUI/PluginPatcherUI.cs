// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.Core.Startup.Events;
using ReusableUIComponents;

namespace CatalogueManager.TestsAndSetup.StartupUI
{
    /// <summary>
    /// There are 3 'tiers' of platform database in the RDMP:
    ///  
    /// <para>Tier 1 - Catalogue Manager database and Data Export Manager database (optionally).  These are stored in user settings and configured through ChoosePlatformDatabases</para>
    /// 
    /// <para>Tier 2 - Databases created and referenced in the Catalogue Manager database such as Logging databases, ANO databases, Data Quality Engine Reporting databases etc (See ManageExternalServers)</para>
    /// 
    /// <para>Tier 3 - Plugin databases, these are wild and uncontrollable by RDMP.  All functionality to interact with these is stores in the plugin that created them.  For example you
    /// might decide that you wanted to do your own unique anonymisation method and create a plugin which uses its own database schema to store/generate anonymous identifiers.</para>
    /// 
    /// <para>This control indicates that the RDMP startup process has found a plugin which has tier 3 databases associated with it.  You will see any/all tier 3 databases found by the 
    /// plugin in StartupUIMainForm under tier 3 (See ManagedDatabaseUI).</para>
    /// </summary>
    public partial class PluginPatcherUI : UserControl
    {
        public PluginPatcherUI()
        {
            InitializeComponent();

            DoTransparencyProperly.ThisHoversOver(ragSmiley1,lblTypeName);
        }

        public void HandlePatcherFound(PluginPatcherFoundEventArgs eventArgs)
        {
            lblTypeName.Text = eventArgs.Type.Name;

            switch (eventArgs.Status)
            {
                case PluginPatcherStatus.CouldNotConstruct:
                    ragSmiley1.Fatal(eventArgs.Exception);
                    break;
                case PluginPatcherStatus.Healthy:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
