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
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.DataExport.Data;
using ReusableLibraryCode.Checks;
using System;
using System.Linq;

namespace Rdmp.Core.CommandLine.DatabaseCreation
{
    internal class ExampleDatasetsCreation
    {
        private PlatformDatabaseCreationRepositoryFinder _repos;


        private const int NumberOfPeople = 5000;
        private const int NumberOfRowsPerDataset = 10000;


        public ExampleDatasetsCreation(PlatformDatabaseCreationRepositoryFinder repos)
        {
            this._repos = repos;
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
            people.GeneratePeople(NumberOfPeople,r);

            //datasets
            var biochem = ImportCatalogue(Create<Biochemistry>(db,people,r,notifier,NumberOfRowsPerDataset,"chi","Healthboard","SampleDate","TestCode"));
            var demography = ImportCatalogue(Create<Demography>(db,people,r,notifier,NumberOfRowsPerDataset,"chi","dtCreated","hb_extract"));
            var prescribing = ImportCatalogue(Create<Prescribing>(db,people,r,notifier,NumberOfRowsPerDataset/10,"chi","PrescribedDate","Name")); //<- this is slooo!
            var admissions = ImportCatalogue(Create<HospitalAdmissions>(db,people,r,notifier,NumberOfRowsPerDataset,"chi","AdmissionDate"));

            //create only on disk this one
            var carotid = Create<CarotidArteryScan>(db,people,r,notifier,NumberOfRowsPerDataset,"RECORD_NUMBER");

            CreateGraph(biochem,"Test Codes","TestCode",null,false);
            CreateGraph(biochem,"Test Codes By Date","TestCode","SampleDate",true);
            
            CreateFilter(biochem,"Creatinine","TestCode","TestCode like '%CRE%'",@"Serum creatinine is a blood measurement.  It is an indicator of renal health.");

            CreateFilter(biochem,"Test Code","TestCode","TestCode like @code","Filters any test code set");
        }

        private IFilter CreateFilter(Catalogue cata, string name,string parentExtractionInformation, string whereSql,string desc)
        {
            var filter = new ExtractionFilter(_repos.CatalogueRepository,name,GetExtractionInformation(cata,parentExtractionInformation));
            filter.WhereSQL = whereSql;
            filter.Description = desc;
            filter.SaveToDatabase();

            var parameterCreator = new ParameterCreator(filter.GetFilterFactory(),null,null);
            parameterCreator.CreateAll(filter,null);

            return filter;
        }

        private void CreateGraph(Catalogue cata, string name, string dimension, string axisIfAny, bool pivot)
        {
            var ac = new AggregateConfiguration(_repos.CatalogueRepository,cata,name);
            
            var mainDimension = ac.AddDimension(GetExtractionInformation(cata,dimension));
            
            if(!string.IsNullOrWhiteSpace(axisIfAny))
            {
                var axisDimension = ac.AddDimension(GetExtractionInformation(cata,axisIfAny));
                var axis = new AggregateContinuousDateAxis(_repos.CatalogueRepository,axisDimension);
                axis.StartDate = "'1970-01-01'";
                axis.AxisIncrement = FAnsi.Discovery.QuerySyntax.Aggregation.AxisIncrement.Year;
                axis.SaveToDatabase();
            }

            if(pivot)
            {
                ac.PivotOnDimensionID = mainDimension.ID;
                ac.SaveToDatabase();
            }
                
        }

        private ExtractionInformation GetExtractionInformation(Catalogue cata, string name)
        {
            try
            {
                return cata.GetAllExtractionInformation(ExtractionCategory.Any).Single(ei=>ei.GetRuntimeName().Equals(name,StringComparison.CurrentCultureIgnoreCase));
            }
            catch
            {
                throw new Exception("Could not find an ExtractionInformation called '" + name + "' in dataset " + cata.Name);
            }
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
            var importer = new TableInfoImporter(_repos.CatalogueRepository,tbl);
            importer.DoImport(out TableInfo ti,out _);
            
            return ti;
        }
        
        private Catalogue ImportCatalogue(DiscoveredTable tbl)
        {
            return ImportCatalogue(ImportTableInfo(tbl));
        }
        private Catalogue ImportCatalogue(TableInfo ti)
        {
            var forwardEngineer = new ForwardEngineerCatalogue(ti,ti.ColumnInfos,true);
            forwardEngineer.ExecuteForwardEngineering(out Catalogue cata, out _,out ExtractionInformation[] eis);
            
            //get descriptions of the columns from BadMedicine
            var desc = new Descriptions();
            cata.Description = desc.Get(cata.Name);
            if(cata.Description != null)
            {
                cata.SaveToDatabase();

                foreach(var ci in cata.CatalogueItems)
                {
                    var ciDescription = desc.Get(cata.Name,ci.Name);
                    if(ciDescription != null)
                    {
                        ci.Description = ciDescription.Trim();
                        ci.SaveToDatabase();
                    }
                }
            }           

            var chi = eis.SingleOrDefault(e=>e.GetRuntimeName().Equals("chi",StringComparison.CurrentCultureIgnoreCase));
            if(chi != null)
            {
                chi.IsExtractionIdentifier = true;
                chi.SaveToDatabase();

                var eds = new ExtractableDataSet(_repos.DataExportRepository,cata);
            }
            return cata;
        }
        
    }
}