// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Cohort.Joinables;
using CohortManagerLibrary;
using CohortManagerLibrary.Execution;
using CohortManagerLibrary.Execution.Joinables;
using NUnit.Framework;

namespace CohortManagerTests
{
    public class CohortCompilerTests:CohortIdentificationTests
    {
        [Test]
        public void AddSameTaskTwice_StaysAtOne()
        {
            CohortCompiler compiler = new CohortCompiler(cohortIdentificationConfiguration);
            container1.AddChild(aggregate1,0);
            try
            {
                compiler.AddTask(aggregate1, null);

                Assert.AreEqual(1, compiler.Tasks.Count);

                var oldTask = compiler.Tasks.First();

                //adding it again with the same SQL should result in it ignoring it
                compiler.AddTask(aggregate1, null);
                Assert.AreEqual(1, compiler.Tasks.Count);

                //make a change to the SQL
                foreach (var d in aggregate1.AggregateDimensions)
                {
                    d.SelectSQL = "'fish'";
                    d.SaveToDatabase();
                }

                //now add it again
                var newAggregate1 = CatalogueRepository.GetObjectByID<AggregateConfiguration>(aggregate1.ID);

                compiler.AddTask(newAggregate1, null);
                Assert.AreEqual(1, compiler.Tasks.Count); //should still be 1 task

                //old task should have been asked to cancel
                Assert.IsTrue(oldTask.Key.CancellationToken.IsCancellationRequested);
                Assert.AreNotSame(oldTask, compiler.Tasks.Single()); //new task should not be the same as the old one
                Assert.IsFalse(compiler.Tasks.Single().Key.CancellationToken.IsCancellationRequested);
                    //new task should not be cancelled} finally {

            }
            finally
            {
                container1.RemoveChild(aggregate1);
            }
        }

        [Test]
        public void AddContainer_StaysAtOne()
        {
            CohortCompiler compiler = new CohortCompiler(cohortIdentificationConfiguration);
            rootcontainer.AddChild(aggregate1, 1);

            compiler.AddTask(rootcontainer, null);//add the root container

            Assert.AreEqual(1, compiler.Tasks.Count);
            var oldTask = compiler.Tasks.First();

            //adding it again with the same SQL should result in it ignoring it
            compiler.AddTask(rootcontainer, null);
            Assert.AreEqual(1, compiler.Tasks.Count);

            //add another aggregate into the container
            rootcontainer.AddChild(aggregate2, 1);

            compiler.AddTask(rootcontainer, null);
            Assert.AreEqual(1, compiler.Tasks.Count);//should still be 1 task

            //old task should have been asked to cancel
            Assert.IsTrue(oldTask.Key.CancellationToken.IsCancellationRequested);
            Assert.AreNotSame(oldTask, compiler.Tasks.Single());//new task should not be the same as the old one
            Assert.IsFalse(compiler.Tasks.Single().Key.CancellationToken.IsCancellationRequested);//new task should not be cancelled

            rootcontainer.RemoveChild(aggregate1);
            rootcontainer.RemoveChild(aggregate2);
        }

        public enum TestCompilerAddAllTasksTestCase
        {
            CIC,
            RootContainer,
            Subcontainer
        }

        [TestCase(TestCompilerAddAllTasksTestCase.CIC,true)]
        [TestCase(TestCompilerAddAllTasksTestCase.CIC, false)]
        [TestCase(TestCompilerAddAllTasksTestCase.RootContainer, true)]
        [TestCase(TestCompilerAddAllTasksTestCase.RootContainer, false)]
        [TestCase(TestCompilerAddAllTasksTestCase.Subcontainer, true)]
        [TestCase(TestCompilerAddAllTasksTestCase.Subcontainer, false)]
        public void TestCompilerAddAllTasks(TestCompilerAddAllTasksTestCase testCase,bool includeSubcontainers)
        {
            var aggregate4 =
            new AggregateConfiguration(CatalogueRepository, testData.catalogue, "UnitTestAggregate4");
            aggregate4.CountSQL = null;
            aggregate4.SaveToDatabase();
            new AggregateDimension(CatalogueRepository, testData.extractionInformations.Single(e => e.GetRuntimeName().Equals("chi")), aggregate4);
            
            var aggregate5 =
            new AggregateConfiguration(CatalogueRepository, testData.catalogue, "UnitTestAggregate5");
            aggregate5.CountSQL = null;
            aggregate5.SaveToDatabase();
            new AggregateDimension(CatalogueRepository, testData.extractionInformations.Single(e => e.GetRuntimeName().Equals("chi")), aggregate5);

            var joinable = new JoinableCohortAggregateConfiguration(CatalogueRepository,cohortIdentificationConfiguration, aggregate5);

            
            try
            {
                //EXCEPT
                    //Aggregate 1
                    //UNION
                        //Aggregate 3
                        //Aggregate 4
                    //Aggregate 2

                //Joinable:aggregate5 (patient index table, the other Aggregates could JOIN to this)

                CohortCompiler compiler = new CohortCompiler(cohortIdentificationConfiguration);
                rootcontainer.AddChild(aggregate1, 1);
                rootcontainer.AddChild(container1);
                container1.Order = 2;
                container1.SaveToDatabase();

                rootcontainer.AddChild(aggregate2, 3);

                container1.AddChild(aggregate3,1);
                container1.AddChild(aggregate4, 2);

                cohortIdentificationConfiguration.RootCohortAggregateContainer_ID = rootcontainer.ID;
                cohortIdentificationConfiguration.SaveToDatabase();
                
                //The bit we are testing
                List<ICompileable> tasks;
                switch (testCase)
                {
                    case TestCompilerAddAllTasksTestCase.CIC:
                        tasks = compiler.AddAllTasks(includeSubcontainers);
                        Assert.AreEqual(joinable,tasks.OfType<JoinableTask>().Single().Joinable); //should be a single joinable
                        Assert.AreEqual(includeSubcontainers?7:6,tasks.Count); //all joinables, aggregates and root container 

                        break;
                    case TestCompilerAddAllTasksTestCase.RootContainer:
                        tasks = compiler.AddTasksRecursively(new ISqlParameter[0], cohortIdentificationConfiguration.RootCohortAggregateContainer, includeSubcontainers);
                        Assert.AreEqual(includeSubcontainers?6:5,tasks.Count); //all aggregates and root container (but not joinables)
                        break;
                    case TestCompilerAddAllTasksTestCase.Subcontainer:
                        tasks = compiler.AddTasksRecursively(new ISqlParameter[0], container1, includeSubcontainers);
                        Assert.AreEqual(includeSubcontainers?3:2,tasks.Count); //subcontainer and it's aggregates
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("testCase");
                }


                rootcontainer.RemoveChild(aggregate1);
                rootcontainer.RemoveChild(aggregate2);

                container1.RemoveChild(aggregate3);
                container1.RemoveChild(aggregate4);
                container1.MakeIntoAnOrphan();

            }
            finally
            {
                aggregate4.DeleteInDatabase();
                joinable.DeleteInDatabase();
                aggregate5.DeleteInDatabase();
            }
        }
    }
}
