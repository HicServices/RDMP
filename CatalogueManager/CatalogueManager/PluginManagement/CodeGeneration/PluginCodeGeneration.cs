// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;

namespace CatalogueManager.PluginManagement.CodeGeneration
{
    /// <summary>
    /// TECHNICAL: Allows you as a C# programmer to generate the class code from a table that will inherit from DatabaseEntity.  This lets you create new objects and have the 
    /// database record automatically created.  It gives you support for revert, save, check still exists, delete, property change events etc.
    /// </summary>
    public partial class PluginCodeGeneration : Form
    {
        public PluginCodeGeneration()
        {
            InitializeComponent();
        }
    }
}
