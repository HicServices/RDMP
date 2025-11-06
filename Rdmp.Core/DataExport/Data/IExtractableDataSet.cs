// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.Repositories;
using System.Collections.Generic;

namespace Rdmp.Core.DataExport.Data;

/// <summary>
/// Controls whether a given <see cref="Catalogue"/> is extractable or not.  Includes whether it is usable only with a specific <see cref="Project"/> or
/// if extraction of the <see cref="Catalogue"/> has been temporarily disabled.
/// </summary>
public interface IExtractableDataSet : IMapsDirectlyToDatabaseTable, IRevertable
{
    /// <summary>
    /// The <see cref="Curation.Data.Catalogue"/> (dataset) which this object allows the extraction of.  The Catalogue object will exist in
    /// the <see cref="ICatalogueRepository"/> database (while the <see cref="IExtractableDataSet"/> exists in the <see cref="IDataExportRepository"/>).
    /// </summary>
    int Catalogue_ID { get; set; }

    /// <summary>
    /// True to temporarily disable extractions of the dataset.
    /// </summary>
    bool DisableExtraction { get; set; }

    /// <inheritdoc cref="Catalogue_ID"/>
    ICatalogue Catalogue { get; }

    /// <summary>
    /// Indicates that the referenced <see cref="Catalogue_ID"/> is associated only with a <see cref="IProject"/> and should not be used outside of that.
    /// 
    /// <para>Usually this means the data is bespoke project data e.g. questionnaire answers for a cohort etc.  These data tables are treated exactly like regular Catalogues and
    /// extracted in the same way as all the regular data.</para>
    /// 
    /// <para>In addition, you can use the columns in the referenced <see cref="Catalogue_ID"/> by joining them to any regular Catalogue being extracted in the Project.  These
    /// selected columns will be bolted on as additional columns.  You can also reference them in the WhereSQL of filters which will trigger an similar Join>.</para>
    /// 
    /// <para>For example imagine you have a custom data set which is 'Patient ID,Date Consented' then you could configure an extraction filters that only extracted records from
    ///  Prescribing, Demography, Biochemistry catalogues AFTER each patients consent date.</para>
    /// </summary>
    List<IProject> Projects{ get; set; }

    /// <summary>
    /// Returns true if the <see cref="ICatalogue"/> behind this dataset has been deleted or is marked <see cref="IMightBeDeprecated.IsDeprecated"/>
    /// </summary>
    bool IsCatalogueDeprecated { get; }
}