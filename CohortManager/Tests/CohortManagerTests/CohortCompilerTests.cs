using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Aggregation;
using CohortManagerLibrary.Execution;
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
    }
}
