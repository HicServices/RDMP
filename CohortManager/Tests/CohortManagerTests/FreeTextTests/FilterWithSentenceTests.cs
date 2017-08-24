using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CohortManagerLibrary.FreeText.Sentencing;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Tests.Common;

namespace CohortManagerTests.FreeTextTests
{
    public class FilterWithSentenceTests:DatabaseTests
    {
        private Catalogue cata;
        private AggregateFilterContainer root;

        private AggregateFilterContainer folder2;
        private AggregateFilterContainer folder3;
        private AggregateFilterContainer folder4;

        private AggregateFilter filter1;
        private AggregateFilter filter2;
        private AggregateFilter filter3;
        private AggregateFilter filter4;
        private AggregateConfiguration config;

        [SetUp]
        public void Setup()
        {
            cata = new Catalogue(CatalogueRepository, "MyCatalogue");
            config = new AggregateConfiguration(CatalogueRepository, cata, "FilterWithSentenceTestsAggregate");

            root = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND);
            config.RootFilterContainer_ID = root.ID;
            config.SaveToDatabase();

            folder2 = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.OR);
            folder3 = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.OR);
            folder4 = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.OR);

            
            filter1 = new AggregateFilter(CatalogueRepository, "Filter1");
            filter1.WhereSQL = "1=1";
            filter1.SaveToDatabase();

            filter2 = new AggregateFilter(CatalogueRepository, "Filter2");
            filter2.WhereSQL = "2=2";
            
            filter2.SaveToDatabase();

            filter3 = new AggregateFilter(CatalogueRepository, "Filter3");
            filter3.WhereSQL = "3=3";
            filter3.SaveToDatabase();

            filter4 = new AggregateFilter(CatalogueRepository, "Filter4");
            filter4.WhereSQL = "4=4";
            filter4.SaveToDatabase();
        }
        [Test]
        public void OneFilter()
        {
            CohortSentence sentence;
            
            //Empty dataset
            sentence = new CohortSentence(config);
            Assert.AreEqual("MyCatalogue", sentence.Text);
            
            //Add a filter to the root container
            root.AddChild(filter1);
            sentence = new CohortSentence(config);
            Assert.AreEqual("MyCatalogue Filter1", sentence.Text);

            //Remove the filter from the root container again
            filter1.MakeIntoAnOrphan();
            sentence = new CohortSentence(config);
            Assert.AreEqual("MyCatalogue", sentence.Text);
        }
        [Test]
        [TestCase(FilterContainerOperation.AND)]
        [TestCase(FilterContainerOperation.OR)]
        public void TwoFilters(FilterContainerOperation operation)
        {
            //Add two filters to the root container
            root.AddChild(filter1);
            root.AddChild(filter2);

            root.Operation = operation;
            root.SaveToDatabase();

            var sentence = new CohortSentence(config);
            Assert.AreEqual(operation == FilterContainerOperation.AND ?
                "MyCatalogue Filter1 AND Filter2" :
            "MyCatalogue Filter1 OR Filter2", sentence.Text);

            //remove them from the container again
            filter1.MakeIntoAnOrphan();
            filter2.MakeIntoAnOrphan();
        }

        [Test]
        [TestCase(FilterContainerOperation.AND)]
        [TestCase(FilterContainerOperation.OR)]
        public void ThreeFilters(FilterContainerOperation operation)
        {
            //Add two filters to the root container
            root.AddChild(filter1);
            root.AddChild(filter2);
            root.AddChild(filter4);

            root.Operation = operation;
            root.SaveToDatabase();

            var sentence = new CohortSentence(config);

            Assert.AreEqual(operation == FilterContainerOperation.AND?
                "MyCatalogue Filter1 AND Filter2 AND Filter4":
            "MyCatalogue Filter1 OR Filter2 OR Filter4", sentence.Text);

            //remove them from the container again
            filter1.MakeIntoAnOrphan();
            filter2.MakeIntoAnOrphan();
            filter4.MakeIntoAnOrphan();
        }
        
        [Test]
        [TestCase(FilterContainerOperation.AND,FilterContainerOperation.OR)]
        [TestCase(FilterContainerOperation.AND, FilterContainerOperation.AND)]
        [TestCase(FilterContainerOperation.OR, FilterContainerOperation.AND)]
        [TestCase(FilterContainerOperation.OR, FilterContainerOperation.OR)]
        public void TestOneSubcontainer(FilterContainerOperation operation1, FilterContainerOperation operation2)
        {
            root.AddChild(folder2);
            
            root.AddChild(filter1);
            root.AddChild(filter2);

            folder2.AddChild(filter3);
            folder2.AddChild(filter4);

            root.Operation = operation1;
            root.SaveToDatabase();

            folder2.Operation = operation2;
            folder2.SaveToDatabase();

            var sentence = new CohortSentence(config);

            string expectedString = "";

            if (operation1 == FilterContainerOperation.AND && operation2 == FilterContainerOperation.AND)
                expectedString = "MyCatalogue Filter1 AND Filter2 AND ( Filter3 AND Filter4 )";
            else
                if (operation1 == FilterContainerOperation.AND && operation2 == FilterContainerOperation.OR)
                    expectedString = "MyCatalogue Filter1 AND Filter2 AND ( Filter3 OR Filter4 )";
                else
                    if (operation1 == FilterContainerOperation.OR && operation2 == FilterContainerOperation.AND)
                        expectedString = "MyCatalogue Filter1 OR Filter2 OR ( Filter3 AND Filter4 )";
                    else
                        if (operation1 == FilterContainerOperation.OR && operation2 == FilterContainerOperation.OR)
                            expectedString = "MyCatalogue Filter1 OR Filter2 OR ( Filter3 OR Filter4 )";

            Assert.AreEqual(expectedString, sentence.Text);

            filter1.MakeIntoAnOrphan();
            filter2.MakeIntoAnOrphan();
            filter3.MakeIntoAnOrphan();
            filter4.MakeIntoAnOrphan();

            folder2.MakeIntoAnOrphan();
        }

        [Test]
        [TestCase(FilterContainerOperation.AND,FilterContainerOperation.OR, FilterContainerOperation.AND)]
        [TestCase(FilterContainerOperation.OR, FilterContainerOperation.OR, FilterContainerOperation.AND)]
        [TestCase(FilterContainerOperation.OR, FilterContainerOperation.AND, FilterContainerOperation.OR)]
        [TestCase(FilterContainerOperation.AND, FilterContainerOperation.OR, FilterContainerOperation.OR)]
        public void TestTwoSubcontainers(FilterContainerOperation rootOperation, FilterContainerOperation operationSubfolder1, FilterContainerOperation operationSubfolder2)
        {
            root.AddChild(folder2);
            root.AddChild(folder3);
            
            folder2.AddChild(filter1);
            folder2.AddChild(filter2);
            
            folder3.AddChild(filter3);
            folder3.AddChild(filter4);

            root.Operation = rootOperation;
            root.SaveToDatabase();

            folder2.Operation = operationSubfolder1;
            folder2.SaveToDatabase();

            folder3.Operation = operationSubfolder2;
            folder3.SaveToDatabase();

            var sentence = new CohortSentence(config);

            string expectedString = "";

            if (rootOperation == FilterContainerOperation.AND && operationSubfolder1 == FilterContainerOperation.OR && operationSubfolder2 == FilterContainerOperation.AND)
                expectedString = "MyCatalogue ( Filter1 OR Filter2 ) AND ( Filter3 AND Filter4 )";
            else
                if (rootOperation == FilterContainerOperation.OR && operationSubfolder1 == FilterContainerOperation.OR && operationSubfolder2 == FilterContainerOperation.AND)
                    expectedString = "MyCatalogue ( Filter1 OR Filter2 ) OR ( Filter3 AND Filter4 )";
            else
            if (rootOperation == FilterContainerOperation.OR && operationSubfolder1 == FilterContainerOperation.AND && operationSubfolder2 == FilterContainerOperation.OR)
                expectedString = "MyCatalogue ( Filter1 AND Filter2 ) OR ( Filter3 OR Filter4 )";
            else
                if (rootOperation == FilterContainerOperation.AND && operationSubfolder1 == FilterContainerOperation.OR && operationSubfolder2 == FilterContainerOperation.OR)
                    expectedString = "MyCatalogue ( Filter1 OR Filter2 ) AND ( Filter3 OR Filter4 )";
                else
                throw new NotSupportedException("No need to exhaustively test every single TestCase, if you added a new TestCase, add the expection here");

            Assert.AreEqual(expectedString, sentence.Text);
            
            filter1.MakeIntoAnOrphan();
            filter2.MakeIntoAnOrphan();
            filter3.MakeIntoAnOrphan();
            filter4.MakeIntoAnOrphan();

            folder2.MakeIntoAnOrphan();
            folder3.MakeIntoAnOrphan();
        }
        [TearDown]
        public void TearDown()
        {

            filter4.DeleteInDatabase();
            filter3.DeleteInDatabase();
            filter2.DeleteInDatabase();
            filter1.DeleteInDatabase();

            folder4.DeleteInDatabase();
            folder3.DeleteInDatabase();
            folder2.DeleteInDatabase();

            config.DeleteInDatabase();
            root.DeleteInDatabase();

            cata.DeleteInDatabase();
        }
    }
}

