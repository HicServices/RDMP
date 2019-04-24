// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Text.RegularExpressions;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CatalogueLibrary.DataFlowPipeline;
using Rdmp.Core.CatalogueLibrary.Repositories;
using Rdmp.Core.CommandLine.Options.Abstracts;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace Rdmp.Core.CommandLine.Runners
{
    class ListRunner :IRunner
    {
        private ListOptions _options;

        public ListRunner(ListOptions options)
        {
            _options = options;
        }

        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            var dbType = repositoryLocator.CatalogueRepository.MEF.GetTypeByNameFromAnyLoadedAssembly(_options.Type);

            if(!typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(dbType))
                throw new NotSupportedException("Only Types derrived from IMapsDirectlyToDatabaseTable can be listed");

            if (repositoryLocator.CatalogueRepository.SupportsObjectType(dbType))
                ListObjects(repositoryLocator.CatalogueRepository, dbType);
            else if (repositoryLocator.DataExportRepository.SupportsObjectType(dbType))
                ListObjects(repositoryLocator.DataExportRepository, dbType);
            else
                throw new NotSupportedException("No IRepository owned up to supporting Type '" + dbType.FullName + "'");

            return 0;
        }

        private void ListObjects(IRepository repository, Type dbType)
        {
            var regex = new Regex(_options.Pattern);
            Console.WriteLine(string.Format("[ID]\t- Name"));

            if (_options.ID != null)
                Show(repository.GetObjectByID(dbType, _options.ID.Value));
            else
                foreach (IMapsDirectlyToDatabaseTable o in repository.GetAllObjects(dbType))
                    if (regex.IsMatch(o.ToString()))
                        Show(o);
        }

        private void Show(IMapsDirectlyToDatabaseTable o)
        {
            Console.WriteLine("[{0}]\t - {1}", o.ID, o);
        }
    }
}
