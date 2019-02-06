// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CohortManagerLibrary.FreeText;
using CohortManagerLibrary.FreeText.Paragraphing;
using CohortManagerLibrary.FreeText.Sentencing;
using NUnit.Framework;
using Tests.Common;

namespace CohortManagerTests.FreeTextTests
{
    public class BasicFreeTextTests:DatabaseTests
    {

        CohortIdentificationConfiguration cic;
        Catalogue c;
        TableInfo t;
        ExtractionInformation ei;
        AggregateConfiguration aggregate;

        
        [SetUp]
        public void CreateEntities()
        {
            cic = new CohortIdentificationConfiguration(CatalogueRepository, "MyConfig");

            c = new Catalogue(CatalogueRepository, "MyDataset1");
            CatalogueItem citem = new CatalogueItem(CatalogueRepository, c, "MyCol");

            t = new TableInfo(CatalogueRepository, "MyTable");
            ColumnInfo cinfo = new ColumnInfo(CatalogueRepository, "MyTable..MyCol", "varchar(10)", t);

            ei = new ExtractionInformation(CatalogueRepository, citem, cinfo, "MyCol");
            ei.IsExtractionIdentifier = true;
            ei.SaveToDatabase();

            
            aggregate = new AggregateConfiguration(CatalogueRepository, c, "MyAggregate");
            aggregate.AddDimension(ei);
            

            cic.CreateRootContainerIfNotExists();
            cic.RootCohortAggregateContainer.AddChild(aggregate, 1);
        }

        [TearDown]
        public void DeleteEntities()
        {
            cic.DeleteInDatabase();
            aggregate.DeleteInDatabase();
            t.DeleteInDatabase();
            c.DeleteInDatabase();
        }



        [Test]
        public void TextParsing_RootContainerIsWrongType()
        {
            var ex = Assert.Throws<NotSupportedException>(() => new FreeTextCompiler(cic));
            Assert.IsTrue(ex.Message.Contains("Root container must be an EXCEPT to work"));
        }

        [Test]
        public void TextParsing_SimplestSingleDataset()
        {
            var container = cic.RootCohortAggregateContainer;
            container.Operation = SetOperation.EXCEPT;
            container.SaveToDatabase();

            FreeTextCompiler compiler = new FreeTextCompiler(cic);
            Assert.AreEqual("MyDataset1",compiler.InclusionCriteria[0].Text);

            CohortParagraph paragraph = compiler.InclusionCriteria[0];
            Assert.AreEqual(ParagraphSectionType.Sentence,paragraph.Sections[0].SectionType);
            Assert.AreEqual(0, paragraph.Sections[0].StartIndex);
            Assert.AreEqual(9, paragraph.Sections[0].EndIndex);
        }

        [Test]
        public void TextParsing_SimplestTwoDatasets()
        {
           
            //make the root container an empty EXCEPT
            var container = cic.RootCohortAggregateContainer;
            container.Operation = SetOperation.EXCEPT;
            container.SaveToDatabase();
            container.RemoveChild(aggregate);

                //add a UNION so it looks like this:

                //EXCEPT
                    //UNION
                        //Dataset1
                        //Dataset1 (could have different filters)


            var container2 = new CohortAggregateContainer(CatalogueRepository, SetOperation.UNION);

            AggregateConfiguration aggregate2 = new AggregateConfiguration(CatalogueRepository, c, "MyAggregate2");
            aggregate2.AddDimension(ei);
            try
            {
                container2.AddChild(aggregate,0);
                container2.AddChild(aggregate2, 1);

                container.AddChild(container2);


                FreeTextCompiler compiler = new FreeTextCompiler(cic);
                Assert.AreEqual("MyDataset1 or MyDataset1", compiler.InclusionCriteria[0].Text);
                ////guide////////0123456789_123456789_123
                
                CohortParagraph paragraph = compiler.InclusionCriteria[0];
                Assert.AreEqual(ParagraphSectionType.Sentence, paragraph.Sections[0].SectionType);
                Assert.AreEqual(ParagraphSectionType.SetOperation, paragraph.Sections[1].SectionType);
                Assert.AreEqual(ParagraphSectionType.Sentence, paragraph.Sections[2].SectionType);
                
                //paragraph
                Assert.AreEqual(0, paragraph.Sections[0].StartIndex);
                Assert.AreEqual(9, paragraph.Sections[0].EndIndex);
                Assert.AreEqual(10, paragraph.Sections[0].Length);
                
                //sentence 1
                var sentence1 = paragraph.Sections[0].CohortSentence;
                Assert.AreEqual(SentenceSectionType.Catalogue, sentence1.Sections[0].SectionType);
                Assert.AreEqual(0, sentence1.Sections[0].StartIndex);
                Assert.AreEqual(9, sentence1.Sections[0].EndIndex);
                Assert.AreEqual(10, sentence1.Sections[0].Length);

                //splitter (paragraph section 2)
                Assert.AreEqual(10, paragraph.Sections[1].StartIndex);
                Assert.AreEqual(13, paragraph.Sections[1].EndIndex);
                Assert.AreEqual(4, paragraph.Sections[1].Length);

                //sentence 2 (paragraph section 3)
                Assert.AreEqual(14, paragraph.Sections[2].StartIndex);
                Assert.AreEqual(23, paragraph.Sections[2].EndIndex);
                Assert.AreEqual(10, paragraph.Sections[2].Length);

                var sentence2 = paragraph.Sections[2].CohortSentence;

                Assert.AreEqual(0, sentence2.Sections[0].StartIndex);
                Assert.AreEqual(9, sentence2.Sections[0].EndIndex);
                Assert.AreEqual(10, sentence2.Sections[0].Length);

                var sentence2Relative = sentence2.Sections[0].GetPositionRelativeToParagraphStart(paragraph.Sections[2]);
                Assert.AreEqual(14, sentence2Relative.StartIndex);
                Assert.AreEqual(23, sentence2Relative.EndIndex);
                Assert.AreEqual(10, sentence2Relative.Length);
            }
            finally
            {
                //swithc it back to the first container for teardown
                container2.RemoveChild(aggregate);
                container.AddChild(aggregate,0);

                //nuke the new stuff
                container2.DeleteInDatabase();
                aggregate2.DeleteInDatabase();
            }
            
        }
    }
}
