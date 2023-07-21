// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.DataExport.DataRelease.Potential;

/// <summary>
///     Describes whether a given extraction artifact (e.g. a data extraction to CSV) is in a fit state for release (e.g.
///     to the researcher)
/// </summary>
public enum Releaseability
{
    /// <summary>
    ///     Releasability has not yet been evaluated (i.e. null)
    /// </summary>
    Undefined = 0,

    /// <summary>
    ///     Something went wrong while RDMP was trying to determine the releasability of the dataset (figuring out if the files
    ///     and current configuration match.)
    /// </summary>
    ExceptionOccurredWhileEvaluatingReleaseability,

    /// <summary>
    ///     The RDMP has no record of an extraction having taken place on the dataset (including a failed one)
    /// </summary>
    NeverBeenSuccessfullyExecuted,

    /// <summary>
    ///     The RDMP has a record of the dataset being extracted but either the extracted data file or accompanying metadata
    ///     file(s) was
    ///     not found in the correct location on disk (either it was moved or the extraction crashed halfway through)
    /// </summary>
    ExtractFilesMissing,

    /// <summary>
    ///     Because project extractions can take some time to do it is possible that another data analyst (or you without
    ///     realising it) makes a change to a dataset
    ///     in the project which has already been extracted (e.g. selecting an additional column for extraction).  If this
    ///     happens your extracted file will be wrong
    ///     (it no longer reflects the current configuration).  In such a situation you will see this icon.  To resolve this
    ///     you must either rectify the configuration
    ///     or re-extract the file.
    /// </summary>
    ExtractionSQLDesynchronisation,

    /// <summary>
    ///     Similar to <see cref="ExtractionSQLDesynchronisation" /> except that the change to the configuration is to switch
    ///     to a different cohort.  This is the worst
    ///     case scenario for release error where you supply a file to a researcher when the file doesn't even relate to the
    ///     cohort he is asking for (or has ethics approval
    ///     for).
    /// </summary>
    CohortDesynchronisation,

    /// <summary>
    ///     Considered only a warning.  You have changed the definition of columns for your extract (overridden the catalogue
    ///     version of one or more columns - See
    ///     ConfigureDatasetUI for how to do this).  Alternatively this can occur if someone has edited the master Catalogue
    ///     implementation of a transform which is part
    ///     of your configuration (Configuration is outdated vs the catalogue).  You should evaluate the differences and make
    ///     sure they are intended before doing a release.
    /// </summary>
    ColumnDifferencesVsCatalogue,

    /// <summary>
    ///     The files extracted match expectations and can be released to the researcher
    /// </summary>
    Releaseable
}