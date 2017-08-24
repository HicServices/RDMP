using System;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable.Relationships;
using NUnit.Framework;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.Dependencies
{
    public class RelationshipTests:DatabaseTests
    {
        [Test]
        public void FindRelationshipsInCatalogueManager()
        {
            RelationshipMap map = new RelationshipMap((SqlConnectionStringBuilder) CatalogueRepository.ConnectionStringBuilder);


            //expect Cata to be cascading to cata item
            Assert.IsTrue(map.Relationships.Any(r =>
                r.Parent.Equals("Catalogue")
                &&
                r.Child.Equals("CatalogueItem")
                &&
                r.DeleteAction == CascadeType.CASCADE));

            Assert.IsTrue(map.Relationships.Any(
                r=>r.Parent.Equals("LoadMetadata")
                    &&
                    r.Child.Equals("Catalogue")
                    && 
                    r.DeleteAction == CascadeType.NO_ACTION));
        }

        [Test]
        [TestCase("Catalogue",10)]
        [TestCase("LoadMetadata",12)]
        public void FindAllTablesDependingOnParentTable(string tableName, int expectedAtLeastThisManyDependents)
        {
            RelationshipMap map = new RelationshipMap((SqlConnectionStringBuilder)CatalogueRepository.ConnectionStringBuilder);
            var dependents = map.GetRelationships(tableName, false);

            Console.WriteLine("Depending on "+tableName+":");
            foreach (Relationship r in dependents)
                Console.WriteLine(r.Child + "(" + r.ChildForeignKeyField + ")");

            Assert.Greater(dependents.Length, expectedAtLeastThisManyDependents);

            var dependentsWithCascade = map.GetRelationships(tableName, true);
            Assert.Less(dependentsWithCascade.Length, dependents.Length);

            Console.WriteLine("CASCADE DELETE on " + tableName + " would affect the following tables (in order of least dependency):");
            foreach (Relationship r in dependentsWithCascade)
                Console.WriteLine(r.Child + "(" + r.ChildForeignKeyField + ")");
        }
    }
}
