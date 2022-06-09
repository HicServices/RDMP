// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using BadMedicine;
using BadMedicine.Datasets;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Rdmp.Core.CommandLine.DatabaseCreation
{
    internal class NightmareDatasets : DataGenerator
    {
        private IRDMPPlatformRepositoryServiceLocator _repos;
        
        public NightmareDatasets(IRDMPPlatformRepositoryServiceLocator repos):base(new Random(123))
        {
            _repos = repos;
        }

        /// <summary>
        /// <para>Generates a lot of metadata in the RDMP platform databases.  This is for testing
        /// system scalability.
        /// </para>
        /// <remarks>We use <see cref="DataGenerator.GetRandomGPCode(Random)"/> a lot, this is just because it is a nice
        /// short string of letter and numbers not because we are actually using GP codes</remarks>
        /// </summary>
        [SuppressMessage("Security", "SCS0005:Weak random number generator.", Justification = "We are generating random metadata, security does not enter into the equation")]
        public void Create()
        {
            var catalogues = new BucketList<Catalogue>();
            var extractableDatasets = new BucketList<ExtractableDataSet>();

            // Based on DLS figures see: https://github.com/HicServices/RDMP/issues/1224
            for (int i = 0; i < 1000; i++)
            {
                var cata = new Catalogue(_repos.CatalogueRepository, $"Catalogue {GetRandomGPCode(r)}");
                cata.Description = GetRandomSentence(r);
                cata.SaveToDatabase();
                catalogues.Add(1,cata);

                var ti = new TableInfo(_repos.CatalogueRepository, $"[MyDb].[Table{i}]");

                // half of datasets have linkage identifiers
                bool hasExtractionIdentifier = r.Next(2) == 0;

                for (int j = 0; j < 40; j++)
                {
                    var col = new ColumnInfo(_repos.CatalogueRepository, $"MyCol{j}", "varchar(10)", ti);
                    var ci = new CatalogueItem(_repos.CatalogueRepository, cata, col.Name);
                    var ei = new ExtractionInformation(_repos.CatalogueRepository, ci, col, col.Name);

                    // make the first field the linkage identifier
                    // if we are doing that
                    if (j == 0 && hasExtractionIdentifier)
                    {
                        ei.IsExtractionIdentifier = true;
                        ei.SaveToDatabase();
                    }
                }

                // half of the Catalogues have IsExtractionIdentifier
                // but lets make only 75% of those extractable datasets
                if (r.Next(5) > 0 && hasExtractionIdentifier)
                {
                    var eds = new ExtractableDataSet(_repos.DataExportRepository, cata);
                    extractableDatasets.Add(1, eds);
                }
            }
            
            
            for (int i = 0; i < 200; i++)
            {
                // each project
                Project p = new Project(_repos.DataExportRepository, $"Project {i}");

                // has an average of 5 ExtractionConfigurations but could have 0 to 10
                var configs = GetGaussianInt(0, 10);

                for(int c = 0;c<configs;c++)
                {
                    var config = new ExtractionConfiguration(_repos.DataExportRepository, p, "Extraction " + GetRandomGPCode(r));
                    if (r.Next(2) == 0)
                        config.RequestTicket = GetRandomGPCode(r); // some have request tickets
                    if (r.Next(4) == 0)
                        config.ReleaseTicket = GetRandomGPCode(r); // some have release tickets
                    config.SaveToDatabase();

                }
            }
        }

        // we are not actually interested in these methods, just want to use GetGaussian etc
        public override object[] GenerateTestDataRow(Person p)
        {
            throw new NotSupportedException();
        }

        protected override string[] GetHeaders()
        {
            throw new NotSupportedException();
        }
    }
}