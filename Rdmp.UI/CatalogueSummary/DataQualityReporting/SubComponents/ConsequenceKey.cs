// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;

namespace Rdmp.UI.CatalogueSummary.DataQualityReporting.SubComponents;

/// <summary>
/// All validation rules configured in the RDMP have an associated 'Consequence', this is like a severity level.  The lowest is 'Missing' and indicates that the failure of the
/// validation rule means that an expected value is not present.  The worst is 'Invalidates Row' which indicates that the validation failure is so serious that the entire row
/// is useless (e.g. a hospital admissions record with no patient identifier making it unlinkable).  See SecondaryConstraintUI for more information on how validation rules are
/// interpreted.
/// 
/// <para>This control documents which colours are used to render each of these consequences in ColumnStatesChart. </para>
/// </summary>
public partial class ConsequenceKey : UserControl
{
    public ConsequenceKey()
    {
        InitializeComponent();
        lblCorrect.BackColor = ConsequenceBar.CorrectColor;
        lblMissing.BackColor = ConsequenceBar.MissingColor;
        lblWrong.BackColor = ConsequenceBar.WrongColor;
        lblInvalid.BackColor = ConsequenceBar.InvalidColor;
        lblHasValue.BackColor = ConsequenceBar.HasValuesColor;
        lblIsNull.BackColor = ConsequenceBar.IsNullColor;
    }
}