// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.WindowArranging;
using DataExportLibrary.Data.DataTables;
using MapsDirectlyToDatabaseTable;

namespace CatalogueManager.ItemActivation.Arranging
{
    public interface IArrangeWindows
    {
        //Advanced cases where you want to show multiple windows at once
        void SetupEditCatalogue(object sender, Catalogue catalogue);
        void SetupEditDataExtractionProject(object sender, Project project);
        void SetupEditLoadMetadata(object sender, LoadMetadata loadMetadata);


        //basic case where you only want to Emphasise and Activate it (after closing all other windows)
        void SetupEditAnything(object sender, IMapsDirectlyToDatabaseTable o);
        void Setup(WindowLayout target);
    }
}
