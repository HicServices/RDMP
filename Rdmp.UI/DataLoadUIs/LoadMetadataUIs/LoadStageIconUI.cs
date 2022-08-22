// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Icons.IconProvision;
using ReusableLibraryCode.Icons;

namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs
{
    /// <summary>
    /// Icon for describing something relates to a given data LoadStage (e.g. AdjustRaw, AdjustStaging etc)
    /// </summary>
    [TechnicalUI]
    public partial class LoadStageIconUI : UserControl
    {
        public LoadStageIconUI()
        {
            InitializeComponent();
        }

        public void Setup(ICoreIconProvider iconProvider,LoadStage stage)
        {
            pictureBox1.Image = iconProvider.GetImage(stage).ImageToBitmap();
            lblLoadStage.Text = stage.ToString();
            this.Width = lblLoadStage.Right;
        }
    }
}
