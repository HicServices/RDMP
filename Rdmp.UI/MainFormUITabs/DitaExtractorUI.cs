// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.Reports;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode;

using PopupChecksUI = Rdmp.UI.ChecksUI.PopupChecksUI;

namespace Rdmp.UI.MainFormUITabs;

/// <summary>
/// RDMP supports extracting all your metadata into DITA format (http://dita.xml.org/ - DITA OASIS Standard).  This is an XML standard with good tool support.  This form lets you
/// export your entire metadata descriptive database into a collection of DITA files.  This might be useful to you for some reason (e.g. to produce offline PDFs etc) but really 
/// the recommended route is to use the built in metadata reports (e.g. MetadataReportUI).  Alternatively you can run queries directly on the RDMP Data Catalogue database
/// which is a super relational database with many tables (Catalogue, CatalogueItem, SupportingDocument etc etc).
/// 
/// <para>NOTE: Make sure that you have set a Resource Acronym for each of your datasets (Catalogues) before attempting to extract in DITA format.</para>
/// </summary>
public partial class DitaExtractorUI : RDMPUserControl
{
    public DitaExtractorUI()
    {
        InitializeComponent();
    }

    private void btnBrowse_Click(object sender, EventArgs e)
    {
        var ofd = new FolderBrowserDialog();
            
            
        var d = ofd.ShowDialog();

        if (d == DialogResult.OK || d == DialogResult.Yes)
            tbExtractionDirectory.Text = ofd.SelectedPath;
    }

    private void btnExtract_Click(object sender, EventArgs e)
    {
        try
        {
            var outputPath = new DirectoryInfo(tbExtractionDirectory.Text);

            if (outputPath.GetFiles("*.dita*").Any())
            {
                if(Activator.YesNo("There are files already in this directory, do you want to delete them?","Clear Directory?"))
                    foreach (var file in outputPath.GetFiles("*.dita*"))
                        file.Delete();
            }

            var extractor = new DitaCatalogueExtractor(Activator.RepositoryLocator.CatalogueRepository, outputPath);
            extractor.Extract(progressBarsUI1);
        }
        catch (Exception ex)
        {

            MessageBox.Show(ex.Message);
        }
            
    }

    private void btnShowDirectory_Click(object sender, EventArgs e)
    {
        if(string.IsNullOrWhiteSpace(tbExtractionDirectory.Text))
            return;

        var d = new DirectoryInfo(tbExtractionDirectory.Text);

        UsefulStuff.GetInstance().ShowFolderInWindowsExplorer(d);
    }

    private void btnCheck_Click(object sender, EventArgs e)
    {
        var popup = new PopupChecksUI("Checking Dita extraction",false);

        var extractor = new DitaCatalogueExtractor(Activator.RepositoryLocator.CatalogueRepository, null);
        popup.StartChecking(extractor);
    }
}