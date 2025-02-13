// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints;
using Rdmp.Core.Validation.Constraints.Secondary;
using Rdmp.Core.Validation.Constraints.Secondary.Predictor;
using Rdmp.Core.Validation.UIAttributes;

namespace Rdmp.UI.Validation;

internal delegate void RequestDeletionHandler(object sender);

/// <summary>
/// Part of <see cref="ValidationSetupUI"/>, this control is for configuring/viewing a single validation rule on a column/transform of a dataset (Catalogue).  For example it might be a NotNull
/// validation constraint which means rows with a null value in this field will fail validation.  Why would you do this you ask? when you can have a database constraint in your data
/// repository that prevents null values? well sometimes research data is quite dirty and if a problematic field(especially if it is a non-essential column) is sometimes worth allowing
/// it through even though it's null and highlighting problem records with the validation rule NotNull.
/// 
/// <para>Other secondary constraints include Regex patterns, standard regexes (See StandardRegexUI), referential integrity constraints etc.</para>
/// 
/// <para>Each constraint has a Consequence (Missing, Wrong, Invalidates Row) these are used to classify the state of each row in the Data Quality Engine when running validation.  For example
/// if you have 2 cells in a row that are both failing validation, one with a consequence of Missing and one with a consequence of Wrong then the entire row is classified as 'Wrong'
/// overall.</para>
/// </summary>
public partial class SecondaryConstraintUI : UserControl
{
    /// <summary>
    /// this UI exists to modify this property, the secondary constraint, it is entirely driven by reflection so should handle any SecondaryConstraint you throw at it
    /// </summary>
    public SecondaryConstraint SecondaryConstriant;

    /// <summary>
    /// A record of the writeable properties in the SecondaryConstraint you threw at it
    /// </summary>
    private PropertyInfo[] _requiredProperties;

    internal event RequestDeletionHandler RequestDeletion;


    private bool loadingComplete;

    public SecondaryConstraintUI(ICatalogueRepository repository, SecondaryConstraint secondaryConstriant,
        string[] otherColumns)
    {
        const int rowHeight = 30;
        //the amount of additional space required to accommodate description labels
        var inflation = 0;
        SecondaryConstriant = secondaryConstriant;

        InitializeComponent();

        if (repository == null)
            return;

        cbxConsequence.DataSource = Enum.GetValues(typeof(Consequence));
        cbxConsequence.SelectedItem = secondaryConstriant.Consequence;

        //put the name of the secondary constraint into the header
        lblType.Text = SecondaryConstriant.GetType().Name;

        lblConsequence.Left = lblType.Right + 5;
        cbxConsequence.Left = lblConsequence.Right + 5;

        //work out what properties can be set on this constraint and create the relevant controls using reflection
        _requiredProperties = secondaryConstriant.GetType().GetProperties().Where(p =>
            p.CanRead && p.CanWrite && p.GetSetMethod(true).IsPublic
            && p.Name != "Name" //skip this one, it is Writeable in order to support XMLSerialization...
            && p.Name != "Consequence" //skip this one because it is dealt with explicitly
            && !p.IsDefined(typeof(HideOnValidationUI), true)
        ).ToArray();

        for (var i = 0; i < _requiredProperties.Length; i++)
        {
            var currentRowPanel = new Panel
            {
                Bounds = new Rectangle(0, 0, tableLayoutPanel1.Width, rowHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Margin = Padding.Empty
            };

            tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;

            tableLayoutPanel1.Controls.Add(currentRowPanel, 0, i + 1);


            var lblName = new Label
            {
                Text = _requiredProperties[i].Name,
                AutoSize = true
            };

            var currentValue = _requiredProperties[i].GetValue(SecondaryConstriant, null);


            //Hard Typed properties - Bool
            if (_requiredProperties[i].PropertyType == typeof(bool))
            {
                var boolControl = new CheckBox
                {
                    Text = _requiredProperties[i].Name,
                    Tag = i,
                    Checked = (bool)currentValue
                };

                boolControl.CheckStateChanged += (s, e) =>
                    _requiredProperties[(int)boolControl.Tag].SetValue(SecondaryConstriant, boolControl.Checked, null);
                currentRowPanel.Controls.Add(boolControl);
            }
            else if (_requiredProperties[i].PropertyType == typeof(PredictionRule)) //Hard Typed property PredictionRule
            {
                //for prediction rules fields
                var cbx = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    DisplayMember = "Name",
                    Tag = i,
                    Width = 200
                };
                cbx.Items.AddRange(Validator.GetPredictionExtraTypes());
                cbx.Items.Add("");
                cbx.SelectedIndexChanged += (s, e) => _requiredProperties[(int)cbx.Tag].SetValue(SecondaryConstriant,
                    cbx.SelectedItem is Type type ? Activator.CreateInstance(type) : null);

                //The dropdown box is a list of Types but we are actually instantiating a value when user selects it (for XML Serialization).  Consequently we must now get the Type for selection purposes
                if (currentValue != null)
                    cbx.SelectedItem = currentValue.GetType();

                currentRowPanel.Controls.Add(lblName);

                cbx.Left = lblName.Right + 5;
                currentRowPanel.Controls.Add(cbx);
            }
            else
            {
                //it's a value control (basically anything that can be represented by text (i.e. not a boolean))
                Control valueControl;

                //if it is expects a column then create a dropdown box
                if (_requiredProperties[i].IsDefined(typeof(ExpectsColumnNameAsInput), true))
                {
                    //for column fields
                    var cbx = new ComboBox
                    {
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Tag = i,
                        Width = 350
                    };
                    cbx.Items.AddRange(otherColumns);
                    cbx.Items.Add("");
                    cbx.SelectedIndexChanged += (s, e) =>
                        _requiredProperties[(int)cbx.Tag].SetValue(SecondaryConstriant,
                            UsefulStuff.ChangeType(cbx.SelectedItem, _requiredProperties[(int)cbx.Tag].PropertyType),
                            null);

                    valueControl = cbx;
                }
                else if (typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(_requiredProperties[i]
                             .PropertyType)) //it is a Catalogue type
                {
                    var dd = new ComboBox
                    {
                        Tag = i,
                        Width = 350,
                        AutoCompleteSource = AutoCompleteSource.ListItems,
                        AutoCompleteMode = AutoCompleteMode.Suggest
                    };
                    var entities = repository.GetAllObjects(_requiredProperties[i].PropertyType).ToArray();

                    if (!entities.Any())
                    {
                        if (_requiredProperties[i].PropertyType == typeof(StandardRegex))
                            MessageBox.Show(
                                "You currently do not have any standard regex concepts in your database.  These can be created from the Table(Advanced) collection.",
                                "No StandardRegex configured in your Catalogue");
                        else
                            MessageBox.Show(
                                $"You currently do not have any {_requiredProperties[i].PropertyType} in your database",
                                "Catalogue Entity Collection Empty");
                    }
                    else
                    {
                        dd.Items.Add("<<Clear>>");
                        dd.Items.AddRange(entities);
                        dd.SelectedIndexChanged += (s, e) =>
                            _requiredProperties[(int)dd.Tag].SetValue(SecondaryConstriant,
                                dd.SelectedItem as IMapsDirectlyToDatabaseTable, null);
                    }

                    //See if it has a value

                    //It has a value, this is a dropdown control right here though so if the revertable state out of date then it means someone else made a change to the database while we were picking columns
                    if (_requiredProperties[i].GetValue(SecondaryConstriant, null) is IRevertable v)
                        v.RevertToDatabaseState();

                    valueControl = dd;
                }
                else //otherwise create a textbox
                {
                    //for string fields
                    valueControl = new TextBox
                    {
                        //if they edit this then write it to the SecondaryConstraint... we can't put i in directly because it goes out of scope so instead we stuff it into Tag and then
                        //get it back at delegate execution time when they change the text.
                        Tag = i
                    };
                    valueControl.TextChanged += setTextOrParseableProperty;

                    if (_requiredProperties[i].IsDefined(typeof(ExpectsLotsOfText), true))
                        valueControl.Width = 300;
                }

                if (currentValue != null)
                    valueControl.Text = _requiredProperties[i].GetValue(SecondaryConstriant, null).ToString()
                        .Replace("00:00:00", "");

                currentRowPanel.Controls.Add(lblName);

                valueControl.Left = lblName.Right + 5;
                currentRowPanel.Controls.Add(valueControl);
            }

            var desc = _requiredProperties[i].GetCustomAttribute<DescriptionAttribute>();

            if (desc != null)
            {
                var lbl = new Label
                {
                    AutoSize = true,
                    Text = desc.Description
                };

                lbl.Font = new Font(lbl.Font, FontStyle.Italic);

                //make some space for it
                inflation += lbl.Height - 7;
                lbl.Top = rowHeight - 7;

                currentRowPanel.Controls.Add(lbl);
                currentRowPanel.Height = rowHeight + lbl.Height;
            }
        }

        //first row
        tableLayoutPanel1.RowStyles[0].SizeType = SizeType.AutoSize;

        Height = _requiredProperties.Length * rowHeight + 45 + inflation;

        loadingComplete = true;
    }

    private void setTextOrParseableProperty(object sender, EventArgs e)
    {
        var senderAsControl = (Control)sender;
        var propertyIdx = (int)senderAsControl.Tag;

        try
        {
            if (string.IsNullOrWhiteSpace(senderAsControl.Text))
            {
                _requiredProperties[propertyIdx].SetValue(SecondaryConstriant, null, null);
            }
            else
            {
                var underlyingType = _requiredProperties[propertyIdx].PropertyType;
                _requiredProperties[propertyIdx].SetValue(SecondaryConstriant,
                    UsefulStuff.ChangeType(senderAsControl.Text, underlyingType), null);
            }


            senderAsControl.ForeColor = Color.Black;
            lblException.Text = "";
        }
        catch (Exception ex)
        {
            senderAsControl.ForeColor = Color.Red;

            //find the innermost exception
            var overflow = 0;
            var msg = ex.Message;
            while (ex.InnerException != null && overflow < 3) //only display up to 3 exception messages
            {
                ex = ex.InnerException;

                if (ex != null)
                    msg += $",{ex.Message}";

                overflow++;
            }

            lblException.Text = msg.Trim(',');
        }
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
        if (RequestDeletion != null)
            RequestDeletion(this);
        else
            throw new Exception("User requested to delete but nobody is listening to the RequestDeletion event!");
    }


    private void cbxConsequence_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!loadingComplete)
            return;

        if (cbxConsequence.SelectedItem != null)
            SecondaryConstriant.Consequence = (Consequence)cbxConsequence.SelectedItem;
    }
}