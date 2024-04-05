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
    //public string Result { get; private set; }
    public LoadDirectory ResultDirectory { get; private set; }

    private Regex _endsWithDataFolder = new(@"[/\\]Data[/\\ ]*$", RegexOptions.IgnoreCase);

    public ChooseLoadDirectoryUI(IActivateItems activator, ILoadMetadata loadMetadata)
    {
        InitializeComponent();

        SetItemActivator(activator);

        var help = loadMetadata.CatalogueRepository.CommentStore.GetDocumentationIfExists(
            "ILoadMetadata.LocationOfFlatFiles", false, true);

        helpIcon1.SetHelpText("Location Of Flat Files", help);

        tbForLoadingPath.Text = loadMetadata.LocationOfForLoadingDirectory;
        tbForArchivingPath.Text = loadMetadata.LocationOfForArchivingDirectory;
        tbExecutablesPath.Text = loadMetadata.LocationOfExecutablesDirectory;
        tbCachePath.Text = loadMetadata.LocationOfCacheDirectory;
        rbChooseYourOwn.Checked = true;
        rbCreateNew.Checked = false;
        rbUseExisting.Checked = false;
        rb_CheckedChanged(null, null);
    }

    private void rb_CheckedChanged(object sender, EventArgs e)
    {
        tbCreateNew.Enabled = rbCreateNew.Checked;
        tbUseExisting.Enabled = rbUseExisting.Checked;
        tbForLoadingPath.Enabled = rbChooseYourOwn.Checked;
        tbForArchivingPath.Enabled = rbChooseYourOwn.Checked;
        tbExecutablesPath.Enabled = rbChooseYourOwn.Checked;
        tbCachePath.Enabled = rbChooseYourOwn.Checked;
        btnOk.Enabled = true;
    }

    private void tbUseExisting_Leave(object sender, EventArgs e)
    {
        if (rbUseExisting.Checked)
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
            try
            {
                var dir = new DirectoryInfo(tbCreateNew.Text);

                if (!dir.Exists)
                    dir.Create();

                ResultDirectory = LoadDirectory.CreateDirectoryStructure(dir.Parent, dir.Name);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }

        if (rbUseExisting.Checked)
            try
            {
                var dir = new LoadDirectory(tbUseExisting.Text);
                ResultDirectory = dir;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception)
            {
                if (Activator.YesNo($"Path is invalid, use anyway? ({exception.Message})", "Invalid Path"))
                {
                    ResultDirectory = new LoadDirectory(tbUseExisting.Text);
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        if (rbChooseYourOwn.Checked)
        {
            var hasError = false;
            lblForLoadingError.Visible = false;
            lblForArchivingError.Visible = false;
            lblExecutablesError.Visible = false;
            lblCacheError.Visible = false;

            if (string.IsNullOrWhiteSpace(tbForLoadingPath.Text))
            {
                lblForLoadingError.Visible = true;
                hasError = true;
            }
            if (string.IsNullOrWhiteSpace(tbForArchivingPath.Text))
            {
                lblForArchivingError.Visible = true;
                hasError = true;
            }
            if (string.IsNullOrWhiteSpace(tbExecutablesPath.Text))
            {
                lblExecutablesError.Visible = true;
                hasError = true;
            }
            if (string.IsNullOrWhiteSpace(tbCachePath.Text))
            {
                lblCacheError.Visible = true;
                hasError = true;
            }
            if (hasError) return;
            var dir = new LoadDirectory(tbForLoadingPath.Text, tbForArchivingPath.Text, tbExecutablesPath.Text, tbCachePath.Text);
            ResultDirectory = dir;
            DialogResult = DialogResult.OK;
            Close();
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
        var fbd = new FolderBrowserDialog
        {
            ShowNewFolderButton = false
        };

        if (fbd.ShowDialog() == DialogResult.OK)
        {
            tbUseExisting.Text = fbd.SelectedPath;
            CheckExistingProjectDirectory();
        }
    }

    private void btnBrowseForLoading_Click(object sender, EventArgs e)
    {
        var fbd = new FolderBrowserDialog
        {
            ShowNewFolderButton = false
        };

        if (fbd.ShowDialog() == DialogResult.OK)
        {
            tbForLoadingPath.Text = fbd.SelectedPath;
            CheckExistingProjectDirectory();
        }
    }

    private void btnBrowseForArchiving_Click(object sender, EventArgs e)
    {
        var fbd = new FolderBrowserDialog
        {
            ShowNewFolderButton = false
        };

        if (fbd.ShowDialog() == DialogResult.OK)
        {
            tbForArchivingPath.Text = fbd.SelectedPath;
            CheckExistingProjectDirectory();
        }
    }

    private void btnBrowseForExecutables_Click(object sender, EventArgs e)
    {
        var fbd = new FolderBrowserDialog
        {
            ShowNewFolderButton = false
        };

        if (fbd.ShowDialog() == DialogResult.OK)
        {
            tbExecutablesPath.Text = fbd.SelectedPath;
            CheckExistingProjectDirectory();
        }
    }

    private void btnBrowseForCache_Click(object sender, EventArgs e)
    {
        var fbd = new FolderBrowserDialog
        {
            ShowNewFolderButton = false
        };

        if (fbd.ShowDialog() == DialogResult.OK)
        {
            tbCachePath.Text = fbd.SelectedPath;
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

    private void tbForLoadingPath_TextChanged(object sender, EventArgs e)
    {
        lblForLoadingError.Visible = string.IsNullOrWhiteSpace(tbForLoadingPath.Text);
    }

    private void tbForArchivingPath_TextChanged(object sender, EventArgs e)
    {
        lblForArchivingError.Visible = string.IsNullOrWhiteSpace(tbForArchivingPath.Text);
    }

    private void tbCachePath_TextChanged(object sender, EventArgs e)
    {
        lblCacheError.Visible = string.IsNullOrWhiteSpace(tbCachePath.Text);
    }

    private void tbExecutablesPath_TextChanged(object sender, EventArgs e)
    {
        lblExecutablesError.Visible = string.IsNullOrWhiteSpace(tbExecutablesPath.Text);
    }


    private void label3_Click(object sender, EventArgs e)
    {

    }

    private void label7_Click(object sender, EventArgs e)
    {

    }

    private void label8_Click(object sender, EventArgs e)
    {

    }
}