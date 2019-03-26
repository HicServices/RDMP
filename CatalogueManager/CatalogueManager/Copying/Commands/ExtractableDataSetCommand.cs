// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.Copying.Commands
{
    public class ExtractableDataSetCommand : ICommand
    {
        public ExtractableDataSet[] ExtractableDataSets { get; set; }

        public ExtractableDataSetCommand(ExtractableDataSet extractableDataSet)
        {
            ExtractableDataSets = new ExtractableDataSet[]{extractableDataSet};
        }

        public ExtractableDataSetCommand(ExtractableDataSet[] extractableDataSetArray)
        {
            ExtractableDataSets = extractableDataSetArray;
        }

        public ExtractableDataSetCommand(ExtractableDataSetPackage extractableDataSetPackage)
        {
            var repository = (IDataExportRepository) extractableDataSetPackage.Repository;
            var packagecontents = new ExtractableDataSetPackageManager(repository);
            ExtractableDataSets = packagecontents.GetAllDataSets(extractableDataSetPackage,repository.GetAllObjects<ExtractableDataSet>());
        }

        public string GetSqlString()
        {
            return null;
        }
    }
}