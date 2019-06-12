// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using BadMedicine;
using BadMedicine.Datasets;
using FAnsi.Discovery;
using FAnsi.Discovery.TypeTranslation;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;
using System;
using System.Linq;

namespace Rdmp.Core.CommandLine.DatabaseCreation
{
    public class ExampleDatasetsCreation
    {
        private IRDMPPlatformRepositoryServiceLocator _repos;


        private const int NumberOfPeople = 5000;
        private const int NumberOfRowsPerDataset = 10000;


        public ExampleDatasetsCreation(IRDMPPlatformRepositoryServiceLocator repos)
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
            var carotid = Create<CarotidArteryScan>(db,people,r,notifier,NumberOfRowsPerDataset,"RECORD_NUMBER");

            CreateAdmissionsViews(db);
            var vConditions = ImportCatalogue(db.ExpectTable("vConditions"));
            var vOperations = ImportCatalogue(db.ExpectTable("vOperations"));    

            CreateGraph(biochem,"Test Codes","TestCode",false,null);
            CreateGraph(biochem,"Test Codes By Date","SampleDate",true,"TestCode");

            CreateFilter(biochem,"Creatinine","TestCode","TestCode like '%CRE%'",@"Serum creatinine is a blood measurement.  It is an indicator of renal health.");
            CreateFilter(biochem,"Test Code","TestCode","TestCode like @code","Filters any test code set");
            
            CreateFilter(
                CreateGraph(vConditions,"Conditions frequency","Condition",false,"Field"),
                "Common Conditions Only",
                @"(Condition in 
(select top 20 Condition from vConditions c
 WHERE Condition <> 'NULL' AND Condition <> 'Nul' 
 group by Condition order by count(*) desc))");
                        
            CreateFilter(
                CreateGraph(vOperations,"Operation frequency","Operation",false,"Field"),
                "Common Operation Only",
                @"(Operation in 
(select top 20 Operation from vOperations c
 WHERE Operation <> 'NULL' AND Operation <> 'Nul' 
 group by Operation order by count(*) desc))");            
            
            //group these all into the same folder
            admissions.Folder = new CatalogueFolder(admissions,@"\admissions");
            admissions.SaveToDatabase();
            vConditions.Folder = new CatalogueFolder(vConditions,@"\admissions");
            vConditions.SaveToDatabase();
            vOperations.Folder = new CatalogueFolder(vOperations,@"\admissions");
            vOperations.SaveToDatabase();


        }

        private IFilter CreateFilter(AggregateConfiguration graph, string name, string whereSql)
        {
            AggregateFilterContainer container;
            if(graph.RootFilterContainer_ID == null)
            {
                container = new AggregateFilterContainer(_repos.CatalogueRepository,FilterContainerOperation.AND);
                graph.RootFilterContainer_ID =container.ID;
                graph.SaveToDatabase();
            }
            else
                container = graph.RootFilterContainer;
            
            var filter = new AggregateFilter(_repos.CatalogueRepository,name,container);
            filter.WhereSQL = whereSql;
            filter.SaveToDatabase();

            return filter;
        }

        private void CreateAdmissionsViews(DiscoveredDatabase db)
        {
            using(var con = db.Server.GetConnection())
            {
                con.Open();
                var cmd = db.Server.GetCommand(

                @"create view vConditions as

SELECT chi,DateOfBirth,AdmissionDate,DischargeDate,Condition,Field
FROM
(
  SELECT chi,DateOfBirth,AdmissionDate,DischargeDate,MainCondition,OtherCondition1,OtherCondition2,OtherCondition3
  FROM HospitalAdmissions
) AS cp
UNPIVOT 
(
  Condition FOR Field IN (MainCondition,OtherCondition1,OtherCondition2,OtherCondition3)
) AS up;",con);
                cmd.ExecuteNonQuery();

                

                cmd = db.Server.GetCommand(
@"create view vOperations as

SELECT chi,DateOfBirth,AdmissionDate,DischargeDate,Operation,Field
FROM
(
  SELECT chi,DateOfBirth,AdmissionDate,DischargeDate,MainOperation,OtherOperation1,OtherOperation2,OtherOperation3
  FROM HospitalAdmissions
) AS cp
UNPIVOT 
(
  Operation FOR Field IN (MainOperation,OtherOperation1,OtherOperation2,OtherOperation3)
) AS up;",con);
                cmd.ExecuteNonQuery();
                

            }
            


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

        /// <summary>
        /// Creates a new AggregateGraph for the given dataset (<paramref name="cata"/>)
        /// </summary>
        /// <param name="cata"></param>
        /// <param name="name">The name to give the graph</param>
        /// <param name="dimension1">The first dimension e.g. pass only one dimension to create a bar chart</param>
        /// <param name="isAxis">True if <paramref name="dimension1"/> should be created as a axis (creates a line chart)</param>
        /// <param name="dimension2">Optional second dimension to create (this will be the pivot column)</param>
        private AggregateConfiguration CreateGraph(Catalogue cata, string name, string dimension1,bool isAxis, string dimension2)
        {
            var ac = new AggregateConfiguration(_repos.CatalogueRepository,cata,name);
            
            var mainDimension = ac.AddDimension(GetExtractionInformation(cata,dimension1));
            var otherDimension = string.IsNullOrWhiteSpace(dimension2) ? null : ac.AddDimension(GetExtractionInformation(cata,dimension2));
            
            if(isAxis)
            {
                var axis = new AggregateContinuousDateAxis(_repos.CatalogueRepository,mainDimension);
                axis.StartDate = "'1970-01-01'";
                axis.AxisIncrement = FAnsi.Discovery.QuerySyntax.Aggregation.AxisIncrement.Year;
                axis.SaveToDatabase();
            }

            if(otherDimension != null)
            {
                ac.PivotOnDimensionID = otherDimension.ID;
                ac.SaveToDatabase();
            }          
            
            return ac;
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
            var tbl = db.CreateTable(dataset,dt,GetExplicitColumnDefinitions<T>());

            if(primaryKey.Length != 0)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Creating Primary Key " + dataset,CheckResult.Success));
                var cols = primaryKey.Select(s=>tbl.DiscoverColumn(s)).ToArray();
                tbl.CreatePrimaryKey(5000,cols);
            }

            return tbl;            
        }

        private DatabaseColumnRequest[] GetExplicitColumnDefinitions<T>() where T : IDataGenerator
        {
            
            if(typeof(T) == typeof(HospitalAdmissions))
            {
                return new []{ 
                    new DatabaseColumnRequest("MainOperation",new DatabaseTypeRequest(typeof(string),4)),
                    new DatabaseColumnRequest("MainOperationB",new DatabaseTypeRequest(typeof(string),4)),
                    new DatabaseColumnRequest("OtherOperation1",new DatabaseTypeRequest(typeof(string),4)),
                    new DatabaseColumnRequest("OtherOperation1B",new DatabaseTypeRequest(typeof(string),4)),
                    new DatabaseColumnRequest("OtherOperation2",new DatabaseTypeRequest(typeof(string),4)),
                    new DatabaseColumnRequest("OtherOperation2B",new DatabaseTypeRequest(typeof(string),4)),
                    new DatabaseColumnRequest("OtherOperation3",new DatabaseTypeRequest(typeof(string),4)),
                    new DatabaseColumnRequest("OtherOperation3B",new DatabaseTypeRequest(typeof(string),4))
                    };
            }


            return null;
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