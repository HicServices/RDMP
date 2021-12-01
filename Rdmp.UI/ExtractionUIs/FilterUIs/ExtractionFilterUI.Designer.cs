using Rdmp.UI.ExtractionUIs.FilterUIs.ParameterUIs;
using Rdmp.UI.Refreshing;

using Rdmp.UI.SimpleControls;

namespace Rdmp.UI.ExtractionUIs.FilterUIs
{
    partial class ExtractionFilterUI : ILifetimeSubscriber
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.cbIsMandatory = new System.Windows.Forms.CheckBox();
            this.tbFilterName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbFilterDescription = new System.Windows.Forms.TextBox();
            this.lblWhere = new System.Windows.Forms.Label();
            this.pQueryEditor = new System.Windows.Forms.Panel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pParameters = new System.Windows.Forms.Panel();
            this.parameterCollectionUI1 = new Rdmp.UI.ExtractionUIs.FilterUIs.ParameterUIs.ParameterCollectionUI();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.pParameters.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Filter Name:";
            // 
            // cbIsMandatory
            // 
            this.cbIsMandatory.AutoSize = true;
            this.cbIsMandatory.Location = new System.Drawing.Point(550, 5);
            this.cbIsMandatory.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.cbIsMandatory.Name = "cbIsMandatory";
            this.cbIsMandatory.Size = new System.Drawing.Size(92, 19);
            this.cbIsMandatory.TabIndex = 9;
            this.cbIsMandatory.Text = "IsMandatory";
            this.toolTip1.SetToolTip(this.cbIsMandatory, "Mandatory filters are automatically added to ExtractionConfigurations and Cohort " +
        "Identification Queries when the parent dataset is selected");
            this.cbIsMandatory.UseVisualStyleBackColor = true;
            // 
            // tbFilterName
            // 
            this.tbFilterName.Location = new System.Drawing.Point(92, 3);
            this.tbFilterName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbFilterName.Name = "tbFilterName";
            this.tbFilterName.Size = new System.Drawing.Size(450, 23);
            this.tbFilterName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 35);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(70, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Description:";
            // 
            // tbFilterDescription
            // 
            this.tbFilterDescription.AcceptsReturn = true;
            this.tbFilterDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilterDescription.Location = new System.Drawing.Point(92, 32);
            this.tbFilterDescription.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.tbFilterDescription.Multiline = true;
            this.tbFilterDescription.Name = "tbFilterDescription";
            this.tbFilterDescription.Size = new System.Drawing.Size(1405, 102);
            this.tbFilterDescription.TabIndex = 3;
            // 
            // lblWhere
            // 
            this.lblWhere.AutoSize = true;
            this.lblWhere.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblWhere.ForeColor = System.Drawing.Color.Blue;
            this.lblWhere.Location = new System.Drawing.Point(22, 5);
            this.lblWhere.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblWhere.Name = "lblWhere";
            this.lblWhere.Size = new System.Drawing.Size(53, 13);
            this.lblWhere.TabIndex = 5;
            this.lblWhere.Text = "WHERE";
            // 
            // pQueryEditor
            // 
            this.pQueryEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pQueryEditor.Location = new System.Drawing.Point(83, 3);
            this.pQueryEditor.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pQueryEditor.Name = "pQueryEditor";
            this.pQueryEditor.Size = new System.Drawing.Size(1405, 330);
            this.pQueryEditor.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.splitContainer1);
            this.panel1.Controls.Add(this.tbFilterName);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.tbFilterDescription);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.cbIsMandatory);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1505, 706);
            this.panel1.TabIndex = 30;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(9, 138);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pParameters);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pQueryEditor);
            this.splitContainer1.Panel2.Controls.Add(this.lblWhere);
            this.splitContainer1.Size = new System.Drawing.Size(1492, 564);
            this.splitContainer1.SplitterDistance = 226;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 30;
            // 
            // pParameters
            // 
            this.pParameters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pParameters.Controls.Add(this.parameterCollectionUI1);
            this.pParameters.Location = new System.Drawing.Point(83, 3);
            this.pParameters.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.pParameters.Name = "pParameters";
            this.pParameters.Size = new System.Drawing.Size(1409, 219);
            this.pParameters.TabIndex = 6;
            // 
            // parameterCollectionUI1
            // 
            this.parameterCollectionUI1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.parameterCollectionUI1.Location = new System.Drawing.Point(0, 0);
            this.parameterCollectionUI1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.parameterCollectionUI1.Name = "parameterCollectionUI1";
            this.parameterCollectionUI1.Size = new System.Drawing.Size(1409, 219);
            this.parameterCollectionUI1.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label3.ForeColor = System.Drawing.Color.Blue;
            this.label3.Location = new System.Drawing.Point(27, 8);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Params";
            // 
            // ExtractionFilterUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ExtractionFilterUI";
            this.Size = new System.Drawing.Size(1505, 706);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.pParameters.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbIsMandatory;
        private System.Windows.Forms.TextBox tbFilterName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbFilterDescription;
        private System.Windows.Forms.Label lblWhere;
        private System.Windows.Forms.Panel pQueryEditor;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel pParameters;
        private ParameterCollectionUI parameterCollectionUI1;

    }
}
