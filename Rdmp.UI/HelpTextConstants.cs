// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.UI
{
    internal class HelpTextConstants
    {
        public const string CatalogueTab =
@"A Catalogue is the technical term for a dataset, this pane lets you modify high level descriptive information about a dataset e.g. the name, geographical coverage,
time coverage etc.  This tab pane also lets you configure dataset level operations such as which Catalogues (datasets) are deprecated, the load procedures, validation,
aggregates etc";
        
        public const string CatalogueItemsTab =
@"A CatalogueItem is a column in a Catalogue (Dataset), as might be expected there are usually many CatalogueItems per Catalogue e.g. MyColumn1, MyColumn2 etc.

CatalogueItems contain high level descriptive information about a concept in a dataset e.g. Age in Demography dataset.  

CatalogueItems are divorced from the underlying technical structure of the database and can exist even if there are no underlying Tables/Columns even configured, you
can hot swap underlying columns without affecting the CatalogueItem.  This tab pane lets you adjust the high level researcher level descriptive metadata about the 
columns in your datasets as well as mapping their underlying technical entities (ColumnInfos), configuring which are extractable, extraction transformations, 
pre-canned filters etc.";

        public const string TableInfoTab =
@"A TableInfo is the technical metadata for an underlying database table.  This tab lets you import tables from your database, annotate them
with additional information such as: How to join tables when building extraction queries, lookup relationships, which columns to anonymise,
additional virtual columns that appear in the data load pipeline but are removed before reaching the final LIVE database.";

        public const string DITATab =
@"DITA stands for Darwin Information Typing Architecture and is an XML data modelling standard.  This tab lets you extract all the descriptive
metadata for your Catalogues into one DITA directory for the creation of metadata websites/PDF documents etc.";
    }
}
