using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataQualityEngine.Data;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using Tests.Common;

namespace DataQualityEngine.Tests
{
    public class DQEGraphAnnotationTests:DatabaseTests
    {
        [Test]
        public void TestCreatingOne()
        {
            Catalogue c = new Catalogue(CatalogueRepository,"FrankyMicky");

            
            try
            {

                var dqeRepo = new DQERepository(CatalogueRepository);
                Evaluation evaluation = new Evaluation(dqeRepo, c);
            
                var annotation = new DQEGraphAnnotation(dqeRepo,1, 2, 3, 4, "Fishesfly", evaluation,DQEGraphType.TimePeriodicityGraph,"ALL");
                
                Assert.AreEqual(annotation.StartX,1);
                Assert.AreEqual(annotation.StartY, 2);
                Assert.AreEqual(annotation.EndX, 3);
                Assert.AreEqual(annotation.EndY, 4);
                Assert.AreEqual(annotation.AnnotationIsForGraph,DQEGraphType.TimePeriodicityGraph);

                //should be about 2 milliseconds ago
                Assert.IsTrue(annotation.CreationDate <= DateTime.Now.AddSeconds(3));
                //certainly shouldnt be before yesterday!
                Assert.IsTrue(annotation.CreationDate > DateTime.Now.AddDays(-1));

                //text should match
                Assert.AreEqual(annotation.Text, "Fishesfly");

                annotation.Text = "flibble";
                annotation.SaveToDatabase();

                annotation.Text = "";
                
                //new copy is flibble
                Assert.AreEqual("flibble", dqeRepo.GetObjectByID<DQEGraphAnnotation>(annotation.ID).Text); 

                annotation.DeleteInDatabase();
            }
            finally
            {
                c.DeleteInDatabase();
            }


        }
    }
}
