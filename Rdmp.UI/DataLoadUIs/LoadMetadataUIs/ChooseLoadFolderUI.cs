// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs;

/// <summary>
/// Allows you to either create a new LoadDirectory or point the software to an existing one.  These folders have a special hierarchy including Cache,ForArchiving, ForLoading, 
/// Executables etc.  In almost all cases you want to have a different directory for each load, this prevents simultaneous loads tripping over one another.
/// 
/// <para>To create a new directory with all the appropriate folders and example configuration files enter the path to an empty folder.  If the folder does not exist yet it will be created
/// when you click Ok.</para>
/// 
/// <para>Alternatively if you want to reuse an existing directory (for example if you have accidentally deleted your old data load configuration and lost the reference to its folder) then
/// you can select the 'use existing' checkbox and enter the path to the existing folder (this should be the root folder i.e. not the Data folder).  This will run Checks on the folder
/// to confirm that it is has an intact structure and then use it for your load.</para>
/// 
/// </summary>
public partial class ChooseLoadDirectoryUI : RDMPForm
{
    /// <summary>
    /// The users final choice of project directory, also check DialogResult for Ok / Cancel
    /// </summary>
    public string Result { get; private set; }

    Regex _endsWithDataFolder = new Regex(@"[/\\]Data[/\\ ]*$", RegexOptions.IgnoreCase);

    public ChooseLoadDirectoryUI(IActivateItems activator, ILoadMetadata loadMetadata)
    {
        InitializeComponent();
            
        SetItemActivator(activator);

        var help = loadMetadata.CatalogueRepository.CommentStore.GetDocumentationIfExists("ILoadMetadata.LocationOfFlatFiles",false,true);
            
        helpIcon1.SetHelpText("Location Of Flat Files",help);

        if(!string.IsNullOrWhiteSpace(loadMetadata.LocationOfFlatFiles))
        {
            tbUseExisting.Text = loadMetadata.LocationOfFlatFiles;
            CheckExistingProjectDirectory();
        }
    }

    private void rb_CheckedChanged(object sender, EventArgs e)
    {
        tbCreateNew.Enabled = rbCreateNew.Checked;
        tbUseExisting.Enabled = rbUseExisting.Checked;
        btnOk.Enabled = true;

    }

    private void tbUseExisting_Leave(object sender, EventArgs e)
    {
        CheckExistingProjectDirectory();
    }

    private void CheckExistingProjectDirectory()
    {
        ragSmiley1.Visible = true;
        try
        {
            new LoadDirectory(tbUseExisting.Text);
            ragSmiley1.Reset();
        }
        catch (Exception ex)
        {
            ragSmiley1.Fatal(ex);
        }
    }


    private void btnOk_Click(object sender, EventArgs e)
    {
        if (rbCreateNew.Checked)
        {
            try
            {
                var dir = new DirectoryInfo(tbCreateNew.Text);
                
                if(!dir.Exists)
                    dir.Create();

                Result = LoadDirectory.CreateDirectoryStructure(dir.Parent,dir.Name).RootPath.FullName;

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }

        if (rbUseExisting.Checked)
        {
            try
            {
                var dir = new LoadDirectory(tbUseExisting.Text);
                Result = dir.RootPath.FullName;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception)
            {
                if(Activator.YesNo($"Path is invalid, use anyway? ({exception.Message})","Invalid Path"))
                {
                    Result = tbUseExisting.Text;
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void btnCreateNewBrowse_Click(object sender, EventArgs e)
    {
        var fbd = new FolderBrowserDialog();
            
        if (fbd.ShowDialog() == DialogResult.OK)
            tbCreateNew.Text = fbd.SelectedPath;
    }

    private void btnBrowseForExisting_Click(object sender, EventArgs e)
    {
        var fbd = new FolderBrowserDialog();
        fbd.ShowNewFolderButton = false;

        if (fbd.ShowDialog() == DialogResult.OK)
        {
            tbUseExisting.Text = fbd.SelectedPath;
            CheckExistingProjectDirectory();
        }
    }

    private void tbUseExisting_TextChanged(object sender, EventArgs e)
    {
        lblDataIsReservedWordExisting.Visible = _endsWithDataFolder.IsMatch(tbUseExisting.Text);
    }

    private void tbCreateNew_TextChanged(object sender, EventArgs e)
    {
        lblDataIsReservedWordNew.Visible = _endsWithDataFolder.IsMatch(tbCreateNew.Text);
    }
}