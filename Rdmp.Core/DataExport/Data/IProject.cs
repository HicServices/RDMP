// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Repositories;
using Rdmp.Core.Ticketing;
using ReusableLibraryCode;

namespace Rdmp.Core.DataExport.Data
{
    /// <summary>
    /// See Project
    /// </summary>
    public interface IProject:IHasDependencies, INamed
    {
        /// <summary>
        /// Optional ticket identifier for auditing time, project requirements etc.  Should be compatible with your currently configured <see cref="ITicketingSystem"/>
        /// </summary>
        string MasterTicket { get; set; }

        /// <summary>
        /// Location on disk that the extracted artifacts for the project (csv files , <see cref="SupportingDocument"/> etc) are put in.
        /// </summary>
        string ExtractionDirectory { get; set; }

        /// <summary>
        /// The number you want to associate with Project, a Project is not in a legal state if it doesn't have one (you can't upload cohorts, do extradctions etc).
        /// You can have multiple <see cref="IProject"/> with the same number (in which case they will have shared access to the same cohorts, anonymisation mappings etc).
        /// </summary>
        int? ProjectNumber { get; set; }
        
        /// <summary>
        /// A <see cref="IProject"/> can have multiple <see cref="IExtractionConfiguration"/> defined (e.g. Cases / Controls or multiple extractions over time).  This
        /// returns all current and frozen (released) configurations.
        /// </summary>
        IExtractionConfiguration[] ExtractionConfigurations { get; }

        /// <summary>
        /// Returns all association links to <see cref="CohortIdentificationConfiguration"/> (cohort queries that are associated with the project).  These are 
        /// association objects not the actual configuration itself.
        /// </summary>
        IProjectCohortIdentificationConfigurationAssociation[] ProjectCohortIdentificationConfigurationAssociations {get; }

        /// <summary>
        /// The database in which the object is persisted
        /// </summary>
        IDataExportRepository DataExportRepository { get; }

        /// <summary>
        /// Returns all datasets which are selected in any <see cref="ExtractionConfigurations"/> in the project
        /// </summary>
        /// <returns></returns>
        ICatalogue[] GetAllProjectCatalogues();

        /// <summary>
        /// Returns all <see cref="ExtractionInformation"/> in all <see cref="Catalogue"/> which are marked as project specific (for this <see cref="IProject"/>)
        /// </summary>
        /// <param name="any"></param>
        /// <returns></returns>
        ExtractionInformation[] GetAllProjectCatalogueColumns(ExtractionCategory any);
    }
}