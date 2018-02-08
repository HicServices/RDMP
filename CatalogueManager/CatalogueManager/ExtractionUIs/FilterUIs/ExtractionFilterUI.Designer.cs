using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs;
using CatalogueManager.Refreshing;
using ReusableUIComponents;

namespace CatalogueManager.ExtractionUIs.FilterUIs
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
            this.btnPublishToCatalogue = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tbFilterDescription = new System.Windows.Forms.TextBox();
            this.lblWhere = new System.Windows.Forms.Label();
            this.pQueryEditor = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.autocompleteReminder = new ReusableUIComponents.KeyboardReminder();
            this.btnParametersExpand = new System.Windows.Forms.Button();
            this.lblParams = new System.Windows.Forms.Label();
            this.ragSmiley1 = new ReusableUIComponents.RAGSmiley();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.objectSaverButton1 = new CatalogueManager.SimpleControls.ObjectSaverButton();
            this.parameterCollectionUI1 = new CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs.ParameterCollectionUI();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Filter Name:";
            // 
            // cbIsMandatory
            // 
            this.cbIsMandatory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbIsMandatory.AutoSize = true;
            this.cbIsMandatory.Location = new System.Drawing.Point(1044, 289);
            this.cbIsMandatory.Name = "cbIsMandatory";
            this.cbIsMandatory.Size = new System.Drawing.Size(84, 17);
            this.cbIsMandatory.TabIndex = 9;
            this.cbIsMandatory.Text = "IsMandatory";
            this.toolTip1.SetToolTip(this.cbIsMandatory, "Mandatory filters are automatically added to ExtractionConfigurations and Cohort " +
        "Identification Queries when the parent dataset is selected");
            this.cbIsMandatory.UseVisualStyleBackColor = true;
            this.cbIsMandatory.CheckedChanged += new System.EventHandler(this.cbIsMandatory_CheckedChanged);
            // 
            // tbFilterName
            // 
            this.tbFilterName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilterName.Location = new System.Drawing.Point(76, 5);
            this.tbFilterName.Name = "tbFilterName";
            this.tbFilterName.Size = new System.Drawing.Size(1194, 20);
            this.tbFilterName.TabIndex = 1;
            this.tbFilterName.TextChanged += new System.EventHandler(this.tbFilterName_TextChanged);
            // 
            // btnPublishToCatalogue
            // 
            this.btnPublishToCatalogue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPublishToCatalogue.Enabled = false;
            this.btnPublishToCatalogue.Location = new System.Drawing.Point(1134, 285);
            this.btnPublishToCatalogue.Name = "btnPublishToCatalogue";
            this.btnPublishToCatalogue.Size = new System.Drawing.Size(56, 23);
            this.btnPublishToCatalogue.TabIndex = 11;
            this.btnPublishToCatalogue.Text = "Publish";
            this.toolTip1.SetToolTip(this.btnPublishToCatalogue, "Filters declared at data export / cohort identification can be elevated (as a cop" +
        "y) to Catalogue level which makes them reusable  in other configurations.");
            this.btnPublishToCatalogue.UseVisualStyleBackColor = true;
            this.btnPublishToCatalogue.Click += new System.EventHandler(this.btnPublishToCatalogue_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Description:";
            // 
            // tbFilterDescription
            // 
            this.tbFilterDescription.AcceptsReturn = true;
            this.tbFilterDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilterDescription.Location = new System.Drawing.Point(76, 28);
            this.tbFilterDescription.Multiline = true;
            this.tbFilterDescription.Name = "tbFilterDescription";
            this.tbFilterDescription.Size = new System.Drawing.Size(1195, 93);
            this.tbFilterDescription.TabIndex = 3;
            this.tbFilterDescription.TextChanged += new System.EventHandler(this.tbFilterDescription_TextChanged);
            // 
            // lblWhere
            // 
            this.lblWhere.AutoSize = true;
            this.lblWhere.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWhere.ForeColor = System.Drawing.Color.Blue;
            this.lblWhere.Location = new System.Drawing.Point(13, 130);
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
            this.pQueryEditor.Location = new System.Drawing.Point(0, 0);
            this.pQueryEditor.Name = "pQueryEditor";
            this.pQueryEditor.Size = new System.Drawing.Size(1193, 282);
            this.pQueryEditor.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer1.Location = new System.Drawing.Point(79, 130);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.autocompleteReminder);
            this.splitContainer1.Panel1.Controls.Add(this.pQueryEditor);
            this.splitContainer1.Panel1.Controls.Add(this.btnPublishToCatalogue);
            this.splitContainer1.Panel1.Controls.Add(this.cbIsMandatory);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.parameterCollectionUI1);
            this.splitContainer1.Size = new System.Drawing.Size(1195, 716);
            this.splitContainer1.SplitterDistance = 313;
            this.splitContainer1.TabIndex = 26;
            // 
            // autocompleteReminder
            // 
            this.autocompleteReminder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.autocompleteReminder.Location = new System.Drawing.Point(3, 288);
            this.autocompleteReminder.Name = "autocompleteReminder";
            this.autocompleteReminder.Size = new System.Drawing.Size(10, 20);
            this.autocompleteReminder.TabIndex = 27;
            // 
            // btnParametersExpand
            // 
            this.btnParametersExpand.Location = new System.Drawing.Point(56, 451);
            this.btnParametersExpand.Name = "btnParametersExpand";
            this.btnParametersExpand.Size = new System.Drawing.Size(17, 23);
            this.btnParametersExpand.TabIndex = 7;
            this.btnParametersExpand.Text = "-";
            this.btnParametersExpand.UseVisualStyleBackColor = true;
            this.btnParametersExpand.Click += new System.EventHandler(this.btnParametersExpand_Click);
            // 
            // lblParams
            // 
            this.lblParams.AutoSize = true;
            this.lblParams.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblParams.ForeColor = System.Drawing.Color.Blue;
            this.lblParams.Location = new System.Drawing.Point(3, 456);
            this.lblParams.Name = "lblParams";
            this.lblParams.Size = new System.Drawing.Size(48, 13);
            this.lblParams.TabIndex = 6;
            this.lblParams.Text = "Params";
            // 
            // ragSmiley1
            // 
            this.ragSmiley1.AlwaysShowHandCursor = false;
            this.ragSmiley1.BackColor = System.Drawing.Color.Transparent;
            this.ragSmiley1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.ragSmiley1.Location = new System.Drawing.Point(39, 146);
            this.ragSmiley1.Name = "ragSmiley1";
            this.ragSmiley1.Size = new System.Drawing.Size(25, 25);
            this.ragSmiley1.TabIndex = 27;
            // 
            // objectSaverButton1
            // 
            this.objectSaverButton1.Location = new System.Drawing.Point(11, 92);
            this.objectSaverButton1.Margin = new System.Windows.Forms.Padding(0);
            this.objectSaverButton1.Name = "objectSaverButton1";
            this.objectSaverButton1.Size = new System.Drawing.Size(57, 29);
            this.objectSaverButton1.TabIndex = 4;
            // 
            // parameterCollectionUI1
            // 
            this.parameterCollectionUI1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parameterCollectionUI1.Location = new System.Drawing.Point(0, 0);
            this.parameterCollectionUI1.Name = "parameterCollectionUI1";
            this.parameterCollectionUI1.Size = new System.Drawing.Size(1193, 397);
            this.parameterCollectionUI1.TabIndex = 0;
            // 
            // ExtractionFilterUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ragSmiley1);
            this.Controls.Add(this.objectSaverButton1);
            this.Controls.Add(this.btnParametersExpand);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbFilterName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbFilterDescription);
            this.Controls.Add(this.lblParams);
            this.Controls.Add(this.lblWhere);
            this.Name = "ExtractionFilterUI";
            this.Size = new System.Drawing.Size(1274, 849);
            this.SizeChanged += new System.EventHandler(this.ExtractionFilterUI_SizeChanged);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbIsMandatory;
        private System.Windows.Forms.TextBox tbFilterName;
        private System.Windows.Forms.Button btnPublishToCatalogue;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbFilterDescription;
        private System.Windows.Forms.Label lblWhere;
        private System.Windows.Forms.Panel pQueryEditor;
        private ParameterCollectionUI parameterCollectionUI1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button btnParametersExpand;
        private System.Windows.Forms.Label lblParams;
        private SimpleControls.ObjectSaverButton objectSaverButton1;
        private KeyboardReminder autocompleteReminder;
        private RAGSmiley ragSmiley1;
        private System.Windows.Forms.ToolTip toolTip1;

    }
}
