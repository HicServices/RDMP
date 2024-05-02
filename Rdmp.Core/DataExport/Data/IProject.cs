// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.Ticketing;

namespace Rdmp.Core.DataExport.Data;

/// <summary>
///     All extractions through DataExportManager must be done through Projects.  A Project has a name, extraction
///     directory and optionally Tickets (if you have a ticketing system
///     configured).  A Project should never be deleted even after all ExtractionConfigurations have been executed as it
///     serves as an audit and a cloning point if you
///     ever need to clone any of the ExtractionConfigurations (e.g. to do an update of project data 5 years on).
///     <para>The <see cref="ProjectNumber" /> must match the project number of the cohorts in your cohort database.</para>
/// </summary>
public interface IProject : IHasDependencies, INamed, IHasFolder
{
    /// <summary>
    ///     Optional ticket identifier for auditing time, project requirements etc.  Should be compatible with your currently
    ///     configured <see cref="ITicketingSystem" />
    /// </summary>
    string MasterTicket { get; set; }

    /// <summary>
    ///     Location on disk that the extracted artifacts for the project (csv files , <see cref="SupportingDocument" /> etc)
    ///     are put in.
    /// </summary>
    string ExtractionDirectory { get; set; }

    /// <summary>
    ///     The number you want to associate with Project, a Project is not in a legal state if it doesn't have one (you can't
    ///     upload cohorts, do extradctions etc).
    ///     You can have multiple <see cref="IProject" /> with the same number (in which case they will have shared access to
    ///     the same cohorts, anonymisation mappings etc).
    /// </summary>
    int? ProjectNumber { get; set; }

    /// <summary>
    ///     A <see cref="IProject" /> can have multiple <see cref="IExtractionConfiguration" /> defined (e.g. Cases / Controls
    ///     or multiple extractions over time).  This
    ///     returns all current and frozen (released) configurations.
    /// </summary>
    IExtractionConfiguration[] ExtractionConfigurations { get; }

    /// <summary>
    ///     Returns all association links to <see cref="CohortIdentificationConfiguration" /> (cohort queries that are
    ///     associated with the project).  These are
    ///     association objects not the actual configuration itself.
    /// </summary>
    IProjectCohortIdentificationConfigurationAssociation[] ProjectCohortIdentificationConfigurationAssociations { get; }

    /// <summary>
    ///     The database in which the object is persisted
    /// </summary>
    IDataExportRepository DataExportRepository { get; }

    /// <summary>
    ///     Returns all datasets which are selected in any <see cref="ExtractionConfigurations" /> in the project
    /// </summary>
    /// <returns></returns>
    ICatalogue[] GetAllProjectCatalogues();

    /// <summary>
    ///     Returns all <see cref="ExtractionInformation" /> in all <see cref="Catalogue" /> which are marked as project
    ///     specific (for this <see cref="IProject" />)
    /// </summary>
    /// <param name="any"></param>
    /// <returns></returns>
    ExtractionInformation[] GetAllProjectCatalogueColumns(ExtractionCategory any);

    /// <summary>
    ///     <para>
    ///         Returns all <see cref="ExtractionInformation" /> in all <see cref="Catalogue" /> which are marked as project
    ///         specific (for this <see cref="IProject" />)
    ///     </para>
    ///     <para>
    ///         High performance overload for when you have a <see cref="ICoreChildProvider" />
    ///     </para>
    /// </summary>
    /// <param name="childProvider"></param>
    /// <param name="any"></param>
    /// <returns></returns>
    ExtractionInformation[] GetAllProjectCatalogueColumns(ICoreChildProvider childProvider, ExtractionCategory any);
}