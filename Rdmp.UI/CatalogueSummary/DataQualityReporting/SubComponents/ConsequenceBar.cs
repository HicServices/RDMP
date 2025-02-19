// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;


namespace Rdmp.UI.CatalogueSummary.DataQualityReporting.SubComponents;

/// <summary>
/// Part of ColumnStatesChart, shows what proportion of a given column in the dataset is passing/failing validation.  See ColumnStatesChart for a description of the use case.
/// </summary>
[TechnicalUI]
public partial class ConsequenceBar : UserControl
{
    public ConsequenceBar()
    {
        InitializeComponent();
    }

    public static Color CorrectColor = Color.Green;
    public static Color MissingColor = Color.Orange;
    public static Color WrongColor = Color.IndianRed;
    public static Color InvalidColor = Color.Red;

    public static Color HasValuesColor = Color.Black;
    public static Color IsNullColor = Color.LightGray;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

    public double Correct { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double Invalid { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double Missing { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double Wrong { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public double DBNull { get; set; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

    public string Label { get; set; }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        base.OnPaintBackground(e);

        //Control looks like this:
        //note that because null count is completely separate from consequence it has its own microbar

        /*****************************************************************/
        //        |.............................|################|,,,,,,,,|
        //Correct |..... Missing................|### Wrong ######|,Invalid|
        //        |.............................|################|,,,,,,,,|
        //        |.............................|################|,,,,,,,,|
        ////////////////////////////////////////////////////////////////////
        //.......Nulls Rectangle............|         Not Nulls Rectangle
        /******************************************************************/

        var bCorrect = new SolidBrush(CorrectColor);
        var bMissing = new SolidBrush(MissingColor);
        var bWrong = new SolidBrush(WrongColor);
        var bInvalid = new SolidBrush(InvalidColor);

        var bValues = new SolidBrush(HasValuesColor);
        var bNulls = new SolidBrush(IsNullColor);

        var totalRecords = Correct + Missing + Invalid + Wrong;

        var heightOfNullsBarStart = (int)(Height * 0.8);
        var heightOfNullsBar = (int)(Height / 5.0);


        //draw the nulls bar
        var valuesRatio = 1 - DBNull / totalRecords;
        var midPointOfNullsBar = (int)(valuesRatio * Width);

        //values
        e.Graphics.FillRectangle(bValues,
            new Rectangle(0, heightOfNullsBarStart, midPointOfNullsBar, heightOfNullsBar));
        e.Graphics.FillRectangle(bNulls,
            new Rectangle(midPointOfNullsBar, heightOfNullsBarStart, Width - midPointOfNullsBar, heightOfNullsBar));


        //draw the main bar
        var correctRightPoint = (int)(Correct / totalRecords * Width);

        var missingWidth = (int)(Missing / totalRecords * Width);
        var missingRightPoint = correctRightPoint + missingWidth;

        var wrongWidth = (int)(Wrong / totalRecords * Width);
        var wrongRightPoint = missingRightPoint + wrongWidth;

        var invalidWidth = (int)(Invalid / totalRecords * Width);

        e.Graphics.FillRectangle(bCorrect, new Rectangle(0, 0, correctRightPoint, heightOfNullsBarStart));
        e.Graphics.FillRectangle(bMissing, new Rectangle(correctRightPoint, 0, missingWidth, heightOfNullsBarStart));
        e.Graphics.FillRectangle(bWrong, new Rectangle(missingRightPoint, 0, wrongWidth, heightOfNullsBarStart));
        e.Graphics.FillRectangle(bInvalid, new Rectangle(wrongRightPoint, 0, invalidWidth, heightOfNullsBarStart));

        if (!string.IsNullOrWhiteSpace(Label))
        {
            var rect = e.Graphics.MeasureString(Label, Font);

            var textX = 0;
            var textY = 2;

            e.Graphics.FillRectangle(Brushes.LightGray, textX, textY, rect.Width, rect.Height);
            e.Graphics.DrawString(Label, Font, Brushes.Black, textX, textY);
        }
    }

    public void GenerateToolTip()
    {
        var toolTip = new ToolTip();

        //let's avoid divide by zero errors
        if (Correct + Missing + Invalid + Wrong < 1)
            return;

        toolTip.SetToolTip(this,
            $"{Label}{Environment.NewLine}Null:{DBNull:n0}{GetPercentageText(DBNull)}Correct:{Correct:n0}{GetPercentageText(Correct)}Missing:{Missing:n0}{GetPercentageText(Missing)}Wrong:{Wrong:n0}{GetPercentageText(Wrong)}Invalid:{Invalid:n0}{GetPercentageText(Invalid).TrimEnd()}"
        );
    }

    private string GetPercentageText(double fraction)
    {
        var totalRecords = Correct + Missing + Invalid + Wrong;
        return $"({Truncate(fraction / totalRecords * 100, 2):n2}%){Environment.NewLine}";
    }

    private static double Truncate(double value, int digits)
    {
        var mult = Math.Pow(10.0, digits);
        return Math.Truncate(value * mult) / mult;
    }
}