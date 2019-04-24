// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using Rdmp.Core.Startup.Events;
using ReusableUIComponents;

namespace CatalogueManager.TestsAndSetup.StartupUI
{
    /// <summary>
    /// Part of the 'Technical' view of StartupUIMainForm, this control tells you about the healthiness of a platform database (e.g. Data Catalogue database, Logging database, ANO database
    ///  etc).  In the RDMP every platform database is accompanied by both a HostAssembly and a HostAssembly.Database dll.  The .Database assembly includes all the SQL patches and initial
    /// creation script for the database type.  The HostAssembly contains the data access logic.  The versions of both dlls AND the database (See [RoundhousE].[Version] table in your 
    /// database) must match.  If the assemblies are ahead of the database then this control will offer the option to Patch (upgrade) the database to match the new assembly.
    /// 
    /// <para>A green face indicates that the correct host assemblies are found and the database is the correct version and reachable etc.
    /// A yellow face indicates that a new version of the host assemblies have been found and the database needs upgrading (patching)
    /// A red face indicates that the database is unreachable or the database version is ahead of the assemblies (you are using outdated assemblies?) or other catastrophic problem.</para>
    /// </summary>
    public partial class ManagedDatabaseUI : UserControl
    {
        private PlatformDatabaseFoundEventArgs _eventArgs;
        public event Action RequestRestart;

        public ManagedDatabaseUI()
        {
            InitializeComponent();
            DoTransparencyProperly.ThisHoversOver(ragSmiley1,pbDatabase);
            DoTransparencyProperly.ThisHoversOver(label1, pictureBox4);

        }
        
        private string Describe(ITableRepository repository,RDMPPlatformDatabaseStatus status)
        {
            var builderAsMsOnly = ((SqlConnectionStringBuilder)repository.ConnectionStringBuilder);

            string suffix = "";
            if (status == RDMPPlatformDatabaseStatus.Healthy || status == RDMPPlatformDatabaseStatus.RequiresPatching)
                suffix = ", Version = " + repository.GetVersion();

            return builderAsMsOnly.InitialCatalog + " (" + builderAsMsOnly.DataSource + ")" + suffix;
        }

        public void HandleDatabaseFound(PlatformDatabaseFoundEventArgs eventArgs)
        {
            _eventArgs = eventArgs;
            lblDatabaseType.Text = eventArgs.Patcher.Name;
            
            lblPatchingAssembly.ForeColor = Color.Black;
            
            if (eventArgs.Repository != null)
                lblDatabase.Text = Describe(eventArgs.Repository,eventArgs.Status);

            var assembly = eventArgs.Patcher?.GetDbAssembly();

            if (assembly != null)
                lblPatchingAssembly.Text = Describe(lblPatchingAssembly, assembly);
            else
            {
                lblPatchingAssembly.Text = "Unknown";
                lblPatchingAssembly.ForeColor = Color.Red;
            }

            llPatch.Visible = false;
            
            switch (eventArgs.Status)
            {
                case RDMPPlatformDatabaseStatus.Unreachable: 
                    ragSmiley1.Fatal(_eventArgs.Exception);
                    break;
                case RDMPPlatformDatabaseStatus.Broken:
                    ragSmiley1.Fatal(_eventArgs.Exception);
                    break;
                case RDMPPlatformDatabaseStatus.RequiresPatching:
                    ragSmiley1.Warning(_eventArgs.Exception);
                    llPatch.Visible = true;
                    break;
                case RDMPPlatformDatabaseStatus.Healthy:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string Describe(Label label, Assembly assembly)
        {
            string name = assembly.GetName().Name;
            string version = new Version(FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion).ToString();

            //the bit we definetly want to know e.g. ".MyPlugin(1.0.2.1)" then as much of the namespace as fits
            string lastBitOfName = "";

            Regex rdotDatabaseAssembly = new Regex(@"([^\.]*\.Database)$");
            Regex regularAssembly = new Regex(@"([^\.]*)$");

            //as much of the namespace as fits with [...] e.g. "HIC.Logg[...]"
            var firstBitOfName = "";

            var matchDatabase = rdotDatabaseAssembly.Match(name);
            var matchNormal = regularAssembly.Match(name);

            if (matchDatabase.Success)
                lastBitOfName = matchDatabase.Groups[1].Value;
            else if (matchNormal.Success)
                lastBitOfName = matchNormal.Groups[1].Value;
            else
                lastBitOfName = name;

            if (lastBitOfName.Length < name.Length)
                firstBitOfName = name.Substring(0,name.Length - lastBitOfName.Length);


            lastBitOfName += "(" + version + ")";

            //how much space is there in the Label
            var availableWidth = label.Width;

            //can we fit all the text?
            var fullWidth = TextRenderer.MeasureText(firstBitOfName + lastBitOfName, Font).Width;

            //yes
            if (fullWidth < availableWidth)
                return firstBitOfName + lastBitOfName; //great
            
            //we can't fit all the text so how much  is the last bit e.g. ".MyPlugin(1.0.3.5)"
            var lastBitWidth = TextRenderer.MeasureText(lastBitOfName, Font).Width;

            //also we don't want to bring in any of the first bit unless we can fit a decent amount of it
            var minimumExtraSpace = TextRenderer.MeasureText("Tes[...]", Font).Width;

            //we don't even have enough room for the assembly name + version + a tiny bit of the start string
            if (lastBitWidth  + minimumExtraSpace> availableWidth)
                return lastBitOfName; //so just return the last bit on it's own

            //ok how much of the first bit can we take
            for (int i = 1; i < firstBitOfName.Length; i++)
            {
                var toReturn = firstBitOfName.Substring(0, i) + "[...]" + lastBitOfName;
                var spaceRequired = TextRenderer.MeasureText(toReturn, Font).Width;

                //the most we can return
                if (spaceRequired > availableWidth)
                    return firstBitOfName.Substring(0, i - 1) + "[...]" + lastBitOfName;

            }
            
            //it all fits?... 
            return firstBitOfName + lastBitOfName;
        }

        private void llPatch_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PatchingUI.ShowIfRequired((SqlConnectionStringBuilder) _eventArgs.Repository.ConnectionStringBuilder,_eventArgs.Repository,_eventArgs.Patcher);

            RequestRestart();

        }

        public void Reset()
        {
            ragSmiley1.Reset();

        }
    }
}
