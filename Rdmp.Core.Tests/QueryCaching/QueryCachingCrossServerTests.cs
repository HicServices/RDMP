// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BadMedicine;
using BadMedicine.Datasets;
using FAnsi;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable.Versioning;
using NUnit.Framework;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Databases;
using ReusableLibraryCode.Checks;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.QueryCaching
{
    class QueryCachingCrossServerTests: TestsRequiringA
    {
        [TestCase(DatabaseType.MicrosoftSQLServer,typeof(QueryCachingPatcher))]
        [TestCase(DatabaseType.MySql, typeof(QueryCachingPatcher))]
        [TestCase(DatabaseType.Oracle, typeof(QueryCachingPatcher))] 

        [TestCase(DatabaseType.MicrosoftSQLServer, typeof(DataQualityEnginePatcher))]
        public void Create_QueryCache(DatabaseType dbType,Type patcherType)
        {
            var db = GetCleanedServer(dbType);

            var patcher = (Patcher)Activator.CreateInstance(patcherType);

            var mds = new MasterDatabaseScriptExecutor(db);
            mds.CreateAndPatchDatabase(patcher, new AcceptAllCheckNotifier());
        }

        [TestCase(DatabaseType.Oracle)]
        [TestCase(DatabaseType.MySql)]
        [TestCase(DatabaseType.MicrosoftSQLServer)]
        public void Join_PatientIndexTable_WithCache(DatabaseType dbType)
        {
            /*
             *           Server1
                       _____________
             *        |Biochemistry|
             *               ↓
             *       _______________
             *       |     Cache    |
             *            ↓ join ↓
             *    _____________________
             *    | Hospital Admissions|
             *
             */

            var db = GetCleanedServer(dbType);


            var r = new Random(500);
            var people = new PersonCollection();
            people.GeneratePeople(5000,r);

            var cic = new CohortIdentificationConfiguration(CatalogueRepository, "cic");

            var joinable = SetupPatientIndexTable(db,people,r,cic);
            
            cic.CreateRootContainerIfNotExists();

            var hospitalAdmissions = SetupPatientIndexTableUser(db, people, r, cic, joinable);
            cic.RootCohortAggregateContainer.AddChild(hospitalAdmissions,0);

            var compiler = new CohortCompiler(cic);
            var runner = new CohortCompilerRunner(compiler, 50000);

            runner.Run(new CancellationToken());

            AssertNoErrors(compiler);
        }


        /// <summary>
        /// Creates a new patient index table based on Biochemistry which selects the distinct dates of "NA" test results
        /// for every patient
        /// </summary>
        /// <param name="db"></param>
        /// <param name="people"></param>
        /// <param name="r"></param>
        /// <param name="cic"></param>
        /// <returns></returns>
        private JoinableCohortAggregateConfiguration SetupPatientIndexTable(DiscoveredDatabase db, PersonCollection people, Random r, CohortIdentificationConfiguration cic)
        {
            var tbl = CreateDataset<Biochemistry>(db, people, 10000, r);
            var cata = Import(tbl,out _, out _, out _,out ExtractionInformation[] eis);

            var chi = eis.Single(ei => ei.GetRuntimeName().Equals("chi", StringComparison.CurrentCultureIgnoreCase));
            var code = eis.Single(ei => ei.GetRuntimeName().Equals("TestCode", StringComparison.CurrentCultureIgnoreCase));
            var date = eis.Single(ei => ei.GetRuntimeName().Equals("SampleDate", StringComparison.CurrentCultureIgnoreCase));

            chi.IsExtractionIdentifier = true;
            chi.SaveToDatabase();

            var ac = new AggregateConfiguration(CatalogueRepository, cata, "NA by date");
            ac.AddDimension(chi);
            ac.AddDimension(code);
            ac.AddDimension(date);
            ac.CountSQL = null;
            
            cic.EnsureNamingConvention(ac);

            var and = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);
            var filter = new AggregateFilter(CatalogueRepository,"TestCode is NA",and);
            filter.WhereSQL = "TestCode = 'NA'";
            filter.SaveToDatabase();

            ac.RootFilterContainer_ID = and.ID;
            ac.SaveToDatabase();

            return new JoinableCohortAggregateConfiguration(CatalogueRepository, cic, ac);
        }

        /// <summary>
        /// Creates a table HospitalAdmissions that uses the patient index table <paramref name="joinable"/> to return distinct patients
        /// who have been hospitalised after receiving an NA test (no time limit)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="people"></param>
        /// <param name="r"></param>
        /// <param name="cic"></param>
        /// <param name="joinable"></param>
        private AggregateConfiguration SetupPatientIndexTableUser(DiscoveredDatabase db, PersonCollection people, Random r, CohortIdentificationConfiguration cic, JoinableCohortAggregateConfiguration joinable)
        {
            var tbl = CreateDataset<HospitalAdmissions>(db, people, 10000, r);
            var cata = Import(tbl, out _, out _, out _, out ExtractionInformation[] eis);

            var chi = eis.Single(ei => ei.GetRuntimeName().Equals("chi", StringComparison.CurrentCultureIgnoreCase));
            chi.IsExtractionIdentifier = true;
            chi.SaveToDatabase();

            var ac = new AggregateConfiguration(CatalogueRepository, cata, "Hospitalised after NA");
            ac.AddDimension(chi);

            ac.CountSQL = null;
            cic.EnsureNamingConvention(ac);
            
            var and = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);
            var filter = new AggregateFilter(CatalogueRepository, "Hospitalised after an NA", and);
            filter.WhereSQL = "AdmissionDate > SampleDate";
            filter.SaveToDatabase();

            ac.RootFilterContainer_ID = and.ID;
            ac.SaveToDatabase();

            //ac joins against the joinable
            new JoinableCohortAggregateConfigurationUse(CatalogueRepository, ac, joinable);

            return ac;

        }


        /// <summary>
        /// Shows the state of the <paramref name="compiler"/> and asserts that all the jobs are finished
        /// </summary>
        /// <param name="compiler"></param>
        private void AssertNoErrors(CohortCompiler compiler)
        {
            Assert.IsNotEmpty(compiler.Tasks);

            TestContext.WriteLine($"| Task | Type | State | Error | RowCount |");


            int i = 0;
            foreach (var kvp in compiler.Tasks)
                TestContext.WriteLine($"{i++} - {kvp.Key.ToString()} | {kvp.Key.GetType()} | {kvp.Key.State} | {kvp.Key?.CrashMessage} | {kvp.Key.FinalRowCount}");
            
            Assert.IsTrue(compiler.Tasks.All(t => t.Key.State == CompilationState.Finished), "Expected all tasks to finish without error");
        }

        
    }
}
