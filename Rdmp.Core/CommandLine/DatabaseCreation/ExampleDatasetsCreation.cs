// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using BadMedicine;
using BadMedicine.Datasets;
using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using ReusableLibraryCode.Checks;
using System;
using System.Linq;

namespace Rdmp.Core.CommandLine.DatabaseCreation
{
    internal class ExampleDatasetsCreation
    {
        private PlatformDatabaseCreationRepositoryFinder repo;

        public ExampleDatasetsCreation(PlatformDatabaseCreationRepositoryFinder repo)
        {
            this.repo = repo;
        }

        internal void Create(DiscoveredDatabase db,bool allowDrop, ICheckNotifier notifier)
        {
            if(db.Exists())
                if(allowDrop)
                    db.Drop();
                else
                    throw new Exception("Database " + db.GetRuntimeName() + " already exists and allowDrop option was not specified");
            
            notifier.OnCheckPerformed(new CheckEventArgs("About to create "+ db.GetRuntimeName(),CheckResult.Success));
            //create a new database for the datasets
            db.Create();

            notifier.OnCheckPerformed(new CheckEventArgs("Succesfully created "+ db.GetRuntimeName(),CheckResult.Success));

            //fixed seed so everyone gets the same datasets
            var r = new Random(500);

            notifier.OnCheckPerformed(new CheckEventArgs("Generating people",CheckResult.Success));
            //people
            var people = new PersonCollection();
            people.GeneratePeople(100000,r);

            //datasets
            var biochem = ImportCatalogue(Create<Biochemistry>(db,people,r,notifier,500000,"chi","Healthboard","SampleDate","TestCode"));
            var demography = ImportCatalogue(Create<Demography>(db,people,r,notifier,500000,"chi","dtCreated","hb_extract"));
            var prescribing = ImportCatalogue(Create<Prescribing>(db,people,r,notifier,50000,"chi","PrescribedDate","Name"));
            var admissions = ImportCatalogue(Create<HospitalAdmissions>(db,people,r,notifier,500000,"chi","AdmissionDate"));

            //create only on disk this one
            var carotid = Create<CarotidArteryScan>(db,people,r,notifier,500000,"RECORD_NUMBER");
        }

        private DiscoveredTable Create<T>(DiscoveredDatabase db,PersonCollection people, Random r, ICheckNotifier notifier,int numberOfRecords, params string[] primaryKey) where T:IDataGenerator
        {
            string dataset = typeof(T).Name;
            notifier.OnCheckPerformed(new CheckEventArgs(string.Format("Generating {0} records for {1}", numberOfRecords,dataset),CheckResult.Success));
            
            var factory = new DataGeneratorFactory();
            
            //half a million biochemistry results
            var biochem = factory.Create(typeof(T),r);
            var dt = biochem.GetDataTable(people,numberOfRecords);

            notifier.OnCheckPerformed(new CheckEventArgs("Uploading " + dataset,CheckResult.Success));
            var tbl = db.CreateTable(dataset,dt);

            if(primaryKey.Length != 0)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Creating Primary Key " + dataset,CheckResult.Success));
                var cols = primaryKey.Select(s=>tbl.DiscoverColumn(s)).ToArray();
                tbl.CreatePrimaryKey(5000,cols);
            }

            return tbl;            
        }
        private TableInfo ImportTableInfo(DiscoveredTable tbl)
        {
            var importer = new TableInfoImporter(repo.CatalogueRepository,tbl);
            importer.DoImport(out TableInfo ti,out _);
            
            return ti;
        }
        
        private Catalogue ImportCatalogue(DiscoveredTable tbl)
        {
            return ImportCatalogue(ImportTableInfo(tbl));
        }
        private Catalogue ImportCatalogue(TableInfo ti)
        {
            var forwardEngineer = new ForwardEngineerCatalogue(ti,ti.ColumnInfos);
            forwardEngineer.ExecuteForwardEngineering(out Catalogue cata, out _,out _);
            return cata;
        }
        
    }
}