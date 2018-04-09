using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.Pipelines;
using RDMPObjectVisualisation.Pipelines;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;

namespace RDMPObjectVisualisation.DataObjects
{

    /// <summary>
    /// Shows the state of a single pipeline component within a data flow pipeline (See 'A Brief Overview Of What A Pipeline Is' in UserManual.docx).  This includes the Type of the pipeline
    /// component, whether the software was able to create an instance of the type and whether it passed checking.  Components can be either a Source, Middle (of which there can be any 
    /// number) or Destination.  Objects (usually DataTables) flow through the pipeline and are processed by each component in turn.
    /// 
    /// <para>But first the pipeline must be constructed, this component is used as part of PipelineDiagram to let you adjust the order / add new components etc into your pipeline.  The pipeline
    /// itself is stored as a reusable resource in the Catalogue Database. </para>
    /// </summary>
    [TechnicalUI]
    class PipelineComponentVisualisation:DataFlowComponentVisualisation
    {
        private readonly Exception _constructionExceptionIfAny;
        private System.Windows.Forms.Label lblPipelineComponent;
        private bool _isSelected;
        private Label label2;
        private Label label1;
        private LinkLabel llConstruction;
        private LinkLabel llInitialization;

        private Pen _origFullPen;
        private Exception _exInitialization;

        public bool AllowDrag { get; set; }
        public bool AllowSelection { get; set; }
        public IPipelineComponent PipelineComponent { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                _fullPen = value ? new Pen(new SolidBrush(Color.Blue), 3.0f) : _origFullPen;
                Invalidate(true);
            }
        }

        public Exception ExInitialization
        {
            get { return _exInitialization; }
            set
            {
                _exInitialization = value;

                if (value == null)
                {
                    llInitialization.Text = "Successful";
                    llInitialization.ForeColor = Color.Green;
                    llInitialization.Links[0].Enabled = false;
                }
                else
                {
                    llInitialization.Text = "Failed";
                    llInitialization.ForeColor = Color.Red;
                    llInitialization.Links[0].Enabled = true;
                }
            }
        }


        public PipelineComponentVisualisation(IPipelineComponent component, Role role, object valueOrNullIfBroken, Exception constructionExceptionIfAny, Func<DragEventArgs, DataFlowComponentVisualisation, DragDropEffects> shouldAllowDrop)
            : base(role, valueOrNullIfBroken, shouldAllowDrop)
        {
            _constructionExceptionIfAny = constructionExceptionIfAny;
            PipelineComponent = component;
            InitializeComponent();

            _origFullPen = _fullPen;

            if (component == null)
                return;

            lblPipelineComponent.Text = component.GetClassNameLastPart();
            this.lblPipelineComponent.Anchor =(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left| System.Windows.Forms.AnchorStyles.Right);

            lblText.Visible = false;

            if (valueOrNullIfBroken != null)
            {

                llConstruction.Text = "Successful";
                llConstruction.ForeColor = Color.Green;
                llConstruction.Links[0].Enabled = false;
            }
            else
            {
                llConstruction.Text = "Failed";
                llConstruction.ForeColor = Color.Red;
            }
            
            llInitialization.Text = "N/A";
            llInitialization.ForeColor = Color.Orange;
            llInitialization.Links[0].Enabled = false;

            this.Width = Math.Max(lblText.PreferredWidth + 80,lblPipelineComponent.PreferredWidth + 180);
            _isEmpty = false;

            MouseDown += Anywhere_MouseDown;
            lblText.MouseDown += Anywhere_MouseDown;
            lblPipelineComponent.MouseDown += Anywhere_MouseDown;
            prongRight1.MouseDown += Anywhere_MouseDown;
            prongRight2.MouseDown += Anywhere_MouseDown;
            prongLeft1.MouseDown += Anywhere_MouseDown;
            prongLeft2.MouseDown += Anywhere_MouseDown;
            pComponent.MouseDown += Anywhere_MouseDown;
        }
        private void InitializeComponent()
        {
            this.lblPipelineComponent = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.llConstruction = new System.Windows.Forms.LinkLabel();
            this.llInitialization = new System.Windows.Forms.LinkLabel();
            this.pComponent.SuspendLayout();
            this.SuspendLayout();
            // 
            // pComponent
            // 
            this.pComponent.Controls.Add(this.label2);
            this.pComponent.Controls.Add(this.label1);
            this.pComponent.Controls.SetChildIndex(this.lblText, 0);
            this.pComponent.Controls.SetChildIndex(this.label1, 0);
            this.pComponent.Controls.SetChildIndex(this.label2, 0);
            // 
            // lblText
            // 
            this.lblText.Size = new System.Drawing.Size(89, 37);
            // 
            // lblPipelineComponent
            // 
            this.lblPipelineComponent.AutoSize = true;
            this.lblPipelineComponent.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.lblPipelineComponent.Location = new System.Drawing.Point(40, 20);
            this.lblPipelineComponent.Name = "lblPipelineComponent";
            this.lblPipelineComponent.Size = new System.Drawing.Size(98, 13);
            this.lblPipelineComponent.TabIndex = 6;
            this.lblPipelineComponent.Text = "PipelineComponent";
            this.lblPipelineComponent.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(22, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Construction:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Initialization:";
            // 
            // llConstruction
            // 
            this.llConstruction.AutoSize = true;
            this.llConstruction.Location = new System.Drawing.Point(104, 37);
            this.llConstruction.Name = "llConstruction";
            this.llConstruction.Size = new System.Drawing.Size(55, 13);
            this.llConstruction.TabIndex = 7;
            this.llConstruction.TabStop = true;
            this.llConstruction.Text = "linkLabel1";
            this.llConstruction.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llConstruction_LinkClicked);
            // 
            // llInitialization
            // 
            this.llInitialization.AutoSize = true;
            this.llInitialization.Location = new System.Drawing.Point(104, 54);
            this.llInitialization.Name = "llInitialization";
            this.llInitialization.Size = new System.Drawing.Size(55, 13);
            this.llInitialization.TabIndex = 8;
            this.llInitialization.TabStop = true;
            this.llInitialization.Text = "linkLabel1";
            this.llInitialization.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llInitialization_LinkClicked);
            // 
            // PipelineComponentVisualisation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.llConstruction);
            this.Controls.Add(this.llInitialization);
            this.Controls.Add(this.lblPipelineComponent);
            this.IsLocked = true;
            this.Name = "PipelineComponentVisualisation";
            this.Controls.SetChildIndex(this.pComponent, 0);
            this.Controls.SetChildIndex(this.prongRight2, 0);
            this.Controls.SetChildIndex(this.prongLeft2, 0);
            this.Controls.SetChildIndex(this.prongRight1, 0);
            this.Controls.SetChildIndex(this.prongLeft1, 0);
            this.Controls.SetChildIndex(this.lblPipelineComponent, 0);
            this.Controls.SetChildIndex(this.llInitialization, 0);
            this.Controls.SetChildIndex(this.llConstruction, 0);
            this.pComponent.ResumeLayout(false);
            this.pComponent.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }


        public event PipelineComponentSelectedHandler ComponentSelected;
        
        private void Anywhere_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button != MouseButtons.Left)
                return;

            if (AllowSelection)
            {
                IsSelected = true;
                ComponentSelected(this, PipelineComponent);
            }

            if (AllowDrag)
            {
                DoDragDrop(this, DragDropEffects.Move);
            }
        }

        private void llConstruction_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if(_constructionExceptionIfAny != null)
                ExceptionViewer.Show(_constructionExceptionIfAny);
        }

        private void llInitialization_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (ExInitialization != null)
                ExceptionViewer.Show(ExInitialization);
        }
    }
}
