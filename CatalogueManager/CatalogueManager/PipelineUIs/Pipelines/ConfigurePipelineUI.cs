// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;
using ReusableUIComponents.Progress;

namespace CatalogueManager.PipelineUIs.Pipelines
{
    /// <summary>
    /// Allows you to name, describe and configure a Data Flow Pipeline (IPipeline).  This is a flow of objects (usually DataTables) from a Source through 0 or more Components to a Destination.
    /// Depending on the context the source and/or/neither destination may be fixed.  There will also be zero or more initialization objects which components can consume.  For example if you
    /// are trying to build a pipeline to import a FlatFileToLoad into your database then you might use a DelimitedFlatFileDataFlowSource component to read the file (assuming it wasn't fixed 
    /// width or a database file or anything wierd) and a DataTableUploadDestination to put it into the endpoint.  
    /// </summary>
    public partial class ConfigurePipelineUI : Form 
    {
        private readonly IPipeline _pipeline;
        private readonly IPipelineUseCase _useCase;
        private IDataFlowPipelineContext _context;
        private readonly DataFlowPipelineEngineFactory _factory;

        private readonly CatalogueRepository _repository;

        private const string NO_COMPONENT = "Unknown";
        
        private PipelineWorkArea _workArea;
        

        public ConfigurePipelineUI(IPipeline pipeline, IPipelineUseCase useCase, CatalogueRepository repository)
        {
            _pipeline = pipeline;
            _useCase = useCase;
            _repository = repository;
            InitializeComponent();

            _factory = new DataFlowPipelineEngineFactory(_useCase,repository.MEF);

            _workArea = new PipelineWorkArea(pipeline, useCase,repository) {Dock = DockStyle.Fill};
            panel1.Controls.Add(_workArea);

            tbName.Text = pipeline.Name;
            tbDescription.Text = pipeline.Description;

            RefreshUIFromDatabase();
            _context = _useCase.GetContext();
         
            KeyPreview = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.W))
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

  
        private void RefreshUIFromDatabase()
        {
            _workArea.SetTo(_pipeline, _useCase);
            try
            {
                lblPreviewStatus.Text = "No Preview Availble";
                lblPreviewStatus.ForeColor = Color.Red;

                InitializeSourceWithPreviewObjects();
            }
            catch (Exception e)
            {
                ExceptionViewer.Show("Failed to initialize source with preview objects",e);
                lblPreviewStatus.Text = "Preview Generation Failed";
                lblPreviewStatus.ForeColor = Color.Red;
            }
        }



        private void InitializeSourceWithPreviewObjects()
        {
            //don't bother with previews in design time
            if(_useCase.IsDesignTime)
                return;

            var src = _useCase.ExplicitSource as IDataFlowSource<DataTable>;

            //fixed source
            if (src != null)
            {
                _workArea.SetPreview(src.TryGetPreview());
                lblPreviewStatus.Text = "Preview Generated successfully";
                lblPreviewStatus.ForeColor = Color.Green;
            }
            else
            {
                //custom source

                //don't bother trying if it's super design time
                if (_useCase.IsDesignTime)
                    return;

                src = _factory.CreateSourceIfExists(_pipeline) as IDataFlowSource<DataTable>;

                if(src == null)
                    return;
                
                //destination is a pipeline component, factory stamp me out an instance
                
                //now use the stamped out instance for preview generation
                InitializeSourceWithPreviewObjects(src);
            }

        }

        private void InitializeSourceWithPreviewObjects(IDataFlowSource<DataTable> s)
        {
            try
            {
                if (s == null)
                    return;

                _context.PreInitializeGeneric(new PopupErrorMessagesEventListener(), s, _useCase.GetInitializationObjects());
                _workArea.SetPreview(s.TryGetPreview());
                lblPreviewStatus.Text = "Preview Generated successfully";
                lblPreviewStatus.ForeColor = Color.Green;
            }
            catch (Exception)
            {
                return;//could not generate preview
            }
        }

       private void tbName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
                tbName.Text = "NoName";

           if (tbName.Text.StartsWith("TO DO:") || tbName.Text.Equals("NoName"))
               tbName.ForeColor = Color.Red;
           else
               tbName.ForeColor = Color.Black;

           _pipeline.Name = tbName.Text;
            _pipeline.SaveToDatabase();
        }
        
        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void tbDescription_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_pipeline.Description) && string.IsNullOrWhiteSpace(tbDescription.Text))
            {
                if (MessageBox.Show("Are you sure you want to delete the current Description entirely?",
                    "Confirm deleting description?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    _pipeline.Description = null;
                else
                {
                    tbDescription.Text = _pipeline.Description;
                }
            }
            else
                _pipeline.Description = tbDescription.Text;

            _pipeline.SaveToDatabase();
        }

        private void btnRetryPreview_Click(object sender, EventArgs e)
        {
            RefreshUIFromDatabase();
        }
    }
}