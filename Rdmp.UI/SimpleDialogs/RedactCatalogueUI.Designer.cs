using BrightIdeasSoftware;
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
        label4 = new System.Windows.Forms.Label();
        tbMaxCount = new System.Windows.Forms.TextBox();
        btnRedact = new System.Windows.Forms.Button();
        btnIdentify = new System.Windows.Forms.Button();
        label3 = new System.Windows.Forms.Label();
        checkedListBox1 = new System.Windows.Forms.CheckedListBox();
        comboBox1 = new System.Windows.Forms.ComboBox();
        label2 = new System.Windows.Forms.Label();
        tbCatalogueName = new System.Windows.Forms.TextBox();
        label1 = new System.Windows.Forms.Label();
        folv = new BrightIdeasSoftware.FastObjectListView();
        folvFoundValue = new OLVColumn();
        folvReplacmentValue = new OLVColumn();
        folvRedactButton = new OLVColumn();
        panel1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)folv).BeginInit();
        SuspendLayout();
        // 
        // panel1
        // 
        panel1.Controls.Add(folv);
        panel1.Controls.Add(label4);
        panel1.Controls.Add(tbMaxCount);
        panel1.Controls.Add(btnRedact);
        panel1.Controls.Add(btnIdentify);
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
        panel1.Size = new System.Drawing.Size(628, 220);
        panel1.TabIndex = 18;
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Location = new System.Drawing.Point(509, 176);
        label4.Name = "label4";
        label4.Size = new System.Drawing.Size(33, 15);
        label4.TabIndex = 9;
        label4.Text = "Max:";
        // 
        // tbMaxCount
        // 
        tbMaxCount.Location = new System.Drawing.Point(548, 172);
        tbMaxCount.Name = "tbMaxCount";
        tbMaxCount.Size = new System.Drawing.Size(77, 23);
        tbMaxCount.TabIndex = 8;
        // 
        // btnRedact
        // 
        btnRedact.Location = new System.Drawing.Point(373, 172);
        btnRedact.Name = "btnRedact";
        btnRedact.Size = new System.Drawing.Size(113, 23);
        btnRedact.TabIndex = 7;
        btnRedact.Text = "Redact All";
        btnRedact.UseVisualStyleBackColor = true;
        btnRedact.Click += btnRedact_Click;
        // 
        // btnIdentify
        // 
        btnIdentify.Location = new System.Drawing.Point(223, 172);
        btnIdentify.Name = "btnIdentify";
        btnIdentify.Size = new System.Drawing.Size(144, 23);
        btnIdentify.TabIndex = 6;
        btnIdentify.Text = "Identify Redactions";
        btnIdentify.UseVisualStyleBackColor = true;
        btnIdentify.Click += btnIdentify_Click;
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
        // folv
        // 
        folv.Location = new System.Drawing.Point(13, 201);
        folv.Name = "folv";
        folv.ShowGroups = false;
        folv.Size = new System.Drawing.Size(612, 287);
        folv.TabIndex = 10;
        folv.View = System.Windows.Forms.View.Details;
        folv.VirtualMode = true;
        folv.AllColumns.Add(folvFoundValue);
        folv.AllColumns.Add(folvReplacmentValue);
        folv.AllColumns.Add(folvRedactButton);
        // 
        // folvFoundValue
        // 
        folvFoundValue.AspectName = "FoundValue";
        folvFoundValue.FillsFreeSpace = true;
        folvFoundValue.MinimumWidth = 100;
        folvFoundValue.Text = "FoundValue";
        folvFoundValue.Width = 100;
        // 
        // folvReplacmentValue
        // 
        folvReplacmentValue.AspectName = "RedactionValue";
        folvReplacmentValue.FillsFreeSpace = true;
        folvReplacmentValue.MinimumWidth = 100;
        folvReplacmentValue.Text = "Redaction Value";
        folvReplacmentValue.Width = 100;
        // 
        // folvRedactButton
        // 
        folvRedactButton.AspectName = "";
        folvRedactButton.FillsFreeSpace = true;
        folvRedactButton.MinimumWidth = 100;
        folvRedactButton.Text = "Redact";
        folvRedactButton.Width = 100;
        // 
        // RedactCatalogueUI
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        Controls.Add(panel1);
        Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
        Name = "RedactCatalogueUI";
        Size = new System.Drawing.Size(628, 220);
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
    private System.Windows.Forms.Button btnIdentify;
    private BrightIdeasSoftware.FastObjectListView folv;
    private OLVColumn folvFoundValue;
    private OLVColumn folvReplacmentValue;
    private OLVColumn folvRedactButton;
}
