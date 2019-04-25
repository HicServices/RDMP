// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataExport.Data
{
    /// <summary>
    /// While the Catalogue Manager database includes support for marking which columns in which Catalogues are extractable (via ExtractionInformation) we need an additional layer
    /// in the Data Export Manager database.  This layer is the ExtractableDataSet object.  An ExtractableDataSet is 'the permission to perform extractions of a given Catalogue'.  We
    /// have this second layer for two main reasons.  The first is so that there is no cross database referential integrity problem for example if you delete a Catalogue 5 years after
    /// performing an extract we can still report to the user the facts in a graceful manner if they clone the old configuration.  The second reason is that you could (if you were crazy)
    /// have multiple DataExportManager databases all feeding off the same Catalogue database - e.g. one that does identifiable extracts and one which does anonymous extracts.  Some
    /// datasets (Catalogues) would therefore be extractable in one DataExportManager database while a different set would be extractable in the other DataExportManager database.
    /// </summary>
    public interface IExtractableDataSet:IMapsDirectlyToDatabaseTable,IRevertable
    {
        /// <summary>
        /// The <see cref="Catalogue"/> (dataset) which this object allows the extraction of.  The Catalogue object will exist in
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
        /// Indicates that the referenced <see cref="Catalogue_ID"/> is associated only with one <see cref="IProject"/> and should not be used outside of that.
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
        int? Project_ID { get; set; }
    }
}