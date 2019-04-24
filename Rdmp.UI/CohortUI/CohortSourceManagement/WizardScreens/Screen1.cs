// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;

namespace DataExportManager.CohortUI.CohortSourceManagement.WizardScreens
{
    /// <summary>
    /// Describes what a cohort is in terms of the RDMP (a list of patient identifiers for a project with accompanying release identifiers).  It is important that you understand
    /// what a cohort is and how the RDMP will use the cohort database you are creating so that you can make the correct decisions in Screen2.
    /// </summary>
    partial class Screen1 : UserControl
    {
        public Screen1()
        {
            InitializeComponent();
        }
    }
}
