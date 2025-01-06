using BrightIdeasSoftware;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using System.Text.RegularExpressions;

namespace Rdmp.UI.SimpleDialogs;

partial class RedactCatalogueUI
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
        panel1 = new System.Windows.Forms.Panel();
        btnNewRegex = new System.Windows.Forms.Button();
        folv = new FastObjectListView();
        folvFoundValue = new OLVColumn();
        folvReplacmentValue = new OLVColumn();
        folvRedactButton = new OLVColumn();
        folvConfiguration = new OLVColumn();
        folvColumn = new OLVColumn();
        label4 = new System.Windows.Forms.Label();
        tbMaxCount = new System.Windows.Forms.TextBox();
        btnRedact = new System.Windows.Forms.Button();
        label3 = new System.Windows.Forms.Label();
        checkedListBox1 = new System.Windows.Forms.CheckedListBox();
        comboBox1 = new System.Windows.Forms.ComboBox();
        label2 = new System.Windows.Forms.Label();
        tbCatalogueName = new System.Windows.Forms.TextBox();
        label1 = new System.Windows.Forms.Label();
        btnRestoreAll = new System.Windows.Forms.Button();
        panel1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)folv).BeginInit();
        SuspendLayout();
        // 
        // panel1
        // 
        panel1.Controls.Add(btnRestoreAll);
        panel1.Controls.Add(btnNewRegex);
        panel1.Controls.Add(folv);
        panel1.Controls.Add(label4);
        panel1.Controls.Add(tbMaxCount);
        panel1.Controls.Add(btnRedact);
        panel1.Controls.Add(label3);
        panel1.Controls.Add(checkedListBox1);
        panel1.Controls.Add(comboBox1);
        panel1.Controls.Add(label2);
        panel1.Controls.Add(tbCatalogueName);
        panel1.Controls.Add(label1);
        panel1.Dock = System.Windows.Forms.DockStyle.Fill;
        panel1.Location = new System.Drawing.Point(0, 0);
        panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        panel1.Name = "panel1";
        panel1.Size = new System.Drawing.Size(682, 556);
        panel1.TabIndex = 18;
        // 
        // btnNewRegex
        // 
        btnNewRegex.Location = new System.Drawing.Point(388, 43);
        btnNewRegex.Name = "btnNewRegex";
        btnNewRegex.Size = new System.Drawing.Size(154, 23);
        btnNewRegex.TabIndex = 11;
        btnNewRegex.Text = "New Regex Configuration";
        btnNewRegex.UseVisualStyleBackColor = true;
        // 
        // folv
        // 
        folv.AllColumns.Add(folvFoundValue);
        folv.AllColumns.Add(folvReplacmentValue);
        folv.AllColumns.Add(folvRedactButton);
        folv.AllColumns.Add(folvConfiguration);
        folv.AllColumns.Add(folvColumn);
        folv.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { folvColumn, folvFoundValue, folvReplacmentValue, folvRedactButton, folvConfiguration });
        folv.Location = new System.Drawing.Point(13, 201);
        folv.Name = "folv";
        folv.ShowGroups = false;
        folv.Size = new System.Drawing.Size(612, 287);
        folv.TabIndex = 10;
        folv.View = System.Windows.Forms.View.Details;
        folv.VirtualMode = true;
        // 
        // folvFoundValue
        // 
        folvFoundValue.AspectName = "RedactedValue";
        folvFoundValue.FillsFreeSpace = true;
        folvFoundValue.MinimumWidth = 100;
        folvFoundValue.Text = "Redacted Value";
        folvFoundValue.Width = 100;
        // 
        // folvReplacmentValue
        // 
        folvReplacmentValue.AspectName = "ReplacementValue";
        folvReplacmentValue.FillsFreeSpace = true;
        folvReplacmentValue.MinimumWidth = 100;
        folvReplacmentValue.Text = "Replacement Value";
        folvReplacmentValue.Width = 100;
        // 
        // folvRedactButton
        // 
        folvRedactButton.AspectName = "ID";
        folvRedactButton.ButtonSizing = OLVColumn.ButtonSizingMode.CellBounds;
        folvRedactButton.FillsFreeSpace = true;
        folvRedactButton.IsButton = true;
        folvRedactButton.MinimumWidth = 100;
        folvRedactButton.Text = "Restore";
        folvRedactButton.Width = 100;
        folvRedactButton.AspectGetter = delegate (object rowObjct) { return "Restore"; };
        // 
        // folvConfiguration
        // 
        folvConfiguration.AspectGetter = delegate (object rowObject)
        {
            var redaction = (RegexRedaction)rowObject;
            return _activator.RepositoryLocator.CatalogueRepository.GetObjectByID<RegexRedactionConfiguration>(redaction.RedactionConfiguration_ID).Name;
        };
        folvConfiguration.ButtonSizing = OLVColumn.ButtonSizingMode.CellBounds;
        folvConfiguration.FillsFreeSpace = true;
        folvConfiguration.MinimumWidth = 100;
        folvConfiguration.Text = "Configuration";
        folvConfiguration.Width = 100;
        // 
        // folvColumn
        // 
        folvColumn.AspectGetter = delegate (object rowObject)
        {
            var redaction = (RegexRedaction)rowObject;
            return _activator.RepositoryLocator.CatalogueRepository.GetObjectByID<ColumnInfo>(redaction.ColumnInfo_ID).GetRuntimeName();
        };
        folvColumn.ButtonSizing = OLVColumn.ButtonSizingMode.CellBounds;
        folvColumn.FillsFreeSpace = true;
        folvColumn.MinimumWidth = 100;
        folvColumn.Text = "Column";
        folvColumn.Width = 100;
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Location = new System.Drawing.Point(325, 176);
        label4.Name = "label4";
        label4.Size = new System.Drawing.Size(33, 15);
        label4.TabIndex = 9;
        label4.Text = "Max:";
        // 
        // tbMaxCount
        // 
        tbMaxCount.Location = new System.Drawing.Point(364, 172);
        tbMaxCount.Name = "tbMaxCount";
        tbMaxCount.Size = new System.Drawing.Size(77, 23);
        tbMaxCount.TabIndex = 8;
        // 
        // btnRedact
        // 
        btnRedact.Location = new System.Drawing.Point(208, 171);
        btnRedact.Name = "btnRedact";
        btnRedact.Size = new System.Drawing.Size(113, 23);
        btnRedact.TabIndex = 7;
        btnRedact.Text = "Redact All";
        btnRedact.UseVisualStyleBackColor = true;
        btnRedact.Click += btnRedact_Click;
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Location = new System.Drawing.Point(22, 83);
        label3.Name = "label3";
        label3.Size = new System.Drawing.Size(102, 15);
        label3.TabIndex = 5;
        label3.Text = "Colums To Redact";
        // 
        // checkedListBox1
        // 
        checkedListBox1.FormattingEnabled = true;
        checkedListBox1.Location = new System.Drawing.Point(13, 101);
        checkedListBox1.Name = "checkedListBox1";
        checkedListBox1.Size = new System.Drawing.Size(189, 94);
        checkedListBox1.TabIndex = 4;
        // 
        // comboBox1
        // 
        comboBox1.FormattingEnabled = true;
        comboBox1.Location = new System.Drawing.Point(82, 44);
        comboBox1.Name = "comboBox1";
        comboBox1.Size = new System.Drawing.Size(300, 23);
        comboBox1.TabIndex = 3;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Location = new System.Drawing.Point(35, 47);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(42, 15);
        label2.TabIndex = 2;
        label2.Text = "Regex:";
        // 
        // tbCatalogueName
        // 
        tbCatalogueName.Enabled = false;
        tbCatalogueName.Location = new System.Drawing.Point(83, 8);
        tbCatalogueName.Name = "tbCatalogueName";
        tbCatalogueName.Size = new System.Drawing.Size(299, 23);
        tbCatalogueName.TabIndex = 1;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new System.Drawing.Point(13, 11);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(64, 15);
        label1.TabIndex = 0;
        label1.Text = "Catalogue:";
        // 
        // btnRestoreAll
        // 
        btnRestoreAll.Enabled = false;
        btnRestoreAll.Location = new System.Drawing.Point(512, 168);
        btnRestoreAll.Name = "btnRestoreAll";
        btnRestoreAll.Size = new System.Drawing.Size(113, 23);
        btnRestoreAll.TabIndex = 12;
        btnRestoreAll.Text = "Restore All";
        btnRestoreAll.UseVisualStyleBackColor = true;
        btnRestoreAll.Click += btnRestoreAll_Click;
        // 
        // RedactCatalogueUI
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        Controls.Add(panel1);
        Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        Name = "RedactCatalogueUI";
        Size = new System.Drawing.Size(682, 556);
        panel1.ResumeLayout(false);
        panel1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)folv).EndInit();
        ResumeLayout(false);
    }

    #endregion
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.TextBox tbCatalogueName;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label3;
    private System.Windows.Forms.CheckedListBox checkedListBox1;
    private System.Windows.Forms.ComboBox comboBox1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.TextBox tbMaxCount;
    private System.Windows.Forms.Button btnRedact;
    private BrightIdeasSoftware.FastObjectListView folv;
    private OLVColumn folvFoundValue;
    private OLVColumn folvReplacmentValue;
    private OLVColumn folvRedactButton;
    private OLVColumn folvColumn;
    private OLVColumn folvConfiguration;
    private System.Windows.Forms.Button btnNewRegex;
    private System.Windows.Forms.Button btnRestoreAll;
}
