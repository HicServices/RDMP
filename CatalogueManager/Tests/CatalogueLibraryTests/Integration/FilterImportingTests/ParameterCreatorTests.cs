using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.FilterImporting;
using CatalogueLibrary.FilterImporting.Construction;
using CatalogueLibrary.Spontaneous;
using NUnit.Framework;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;
using Rhino.Mocks;

namespace CatalogueLibraryTests.Integration.FilterImportingTests
{
    public class ParameterCreatorTests
    {
        [Test]
        public void NoParametersTest_CreateNotCalled()
        {
            var f = MockRepository.GenerateMock<IFilter>();
            f.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());

            var factory = MockRepository.GenerateMock<IFilterFactory>();
            factory.Expect(m => m.CreateNewParameter(null, null)).Repeat.Never();

            var creator = new ParameterCreator(factory, new ISqlParameter[0], null);
            creator.CreateAll(f,null);

            factory.VerifyAllExpectations();
        }
        

        [Test]
        public void SingleParameterTest_NullReturnFromConstruct_Throws()
        {
            var f = MockRepository.GenerateStub<IFilter>();
            f.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            f.WhereSQL = "@bob = 'bob'";
            
            var factory = MockRepository.GenerateStrictMock<IFilterFactory>();
            factory.Expect(m => m.CreateNewParameter(f,"DECLARE @bob AS varchar(50);")).Return(null);

            var creator = new ParameterCreator(factory, null, null);

            var ex = Assert.Throws<NullReferenceException>(()=>creator.CreateAll(f,null));

            Assert.IsTrue(ex.Message.StartsWith("Parameter construction method returned null"));
        }
       
        [Test]
        public void SingleParameterTest_OneParameter_CreateCalled()
        {
            var p = MockRepository.GenerateMock<ISqlParameter>();
            p.Expect(m => m.SaveToDatabase()).Repeat.Once();//save should be called because there is no VAlue on the parameter

            var f = MockRepository.GenerateStub<IFilter>();
            f.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            f.WhereSQL = "@bob = 'bob'";

            var factory = MockRepository.GenerateMock<IFilterFactory>();
            factory.Expect(m => m.CreateNewParameter(f,"DECLARE @bob AS varchar(50);")).Return(p).Repeat.Once();
            
            var creator = new ParameterCreator(factory, null, null);
            creator.CreateAll(f,null);

            p.VerifyAllExpectations();
            factory.VerifyAllExpectations();
        }
        [Test]
        public void SingleParameterTest_ParameterAlreadyExists_CreateNotCalled()
        {
            var p = MockRepository.GenerateMock<ISqlParameter>();
            p.Expect(m => m.SaveToDatabase()).Repeat.Never();//save should not be called

            var existingParameter = MockRepository.GenerateStub<ISqlParameter>();
            existingParameter.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            existingParameter.Stub(x => x.ParameterName).Return("@bob");

            var f = MockRepository.GenerateStub<IFilter>();
            f.WhereSQL = "@bob = 'bob'";
            f.Expect(m => m.GetAllParameters()).Return(new[] {existingParameter});

            var factory = MockRepository.GenerateMock<IFilterFactory>();
            factory.Expect(m => m.CreateNewParameter(f, "")).IgnoreArguments().Return(p).Repeat.Never(); //should never be called because the filter already has 

            var creator = new ParameterCreator(factory, null, null);
            creator.CreateAll(f,null);
            creator.CreateAll(f, null);
            creator.CreateAll(f, null);//no matter how many times we call create it shouldn't make more because there is already one

            p.VerifyAllExpectations();
            factory.VerifyAllExpectations();
        }
        
        [Test]
        public void SingleParameterTest_GlobalOverrides_CreateNotCalled()
        {
            var f = MockRepository.GenerateStub<IFilter>();
            f.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            f.WhereSQL = "@bob = 'bob'";

            var global = MockRepository.GenerateStub<ISqlParameter>();
            global.Stub(x=>x.ParameterName).Return("@bob");

            var factory = MockRepository.GenerateMock<IFilterFactory>();
            factory.Expect(m => m.CreateNewParameter(null, null)).Repeat.Never();

            var creator = new ParameterCreator(factory, new[] { global }, null);
            creator.CreateAll(f,null);

            factory.VerifyAllExpectations();
        }

        [Test]
        public void SingleParameterTest_GlobalButNotSameName_CreateCalled()
        {
            var f = MockRepository.GenerateStub<IFilter>();
            f.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            f.WhereSQL = "@bob = 'bob'";

            var global = MockRepository.GenerateStub<ISqlParameter>();
            global.Stub(x => x.ParameterName).Return("@bob");
            
            var factory = MockRepository.GenerateMock<IFilterFactory>();
            factory.Expect(m => m.CreateNewParameter(f, "DECLARE @bob AS varchar(50);")).Repeat.Once();

            var creator = new ParameterCreator(factory, null, null);
            creator.CreateAll(f,null);

            factory.VerifyAllExpectations();

        }


        [Test]
        public void SingleParameterTest_Template_TemplateValuesUsed()
        {
            //The constructor returns
            var pstub = MockRepository.GenerateStub<ISqlParameter>();
            
            //The filter that requires that the parameters be created
            var f = MockRepository.GenerateStub<IFilter>();
            f.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            f.WhereSQL = "@bob = 'bob'";

            //The template which is an existing known about parameter from the master filter that is being duplicated.  This template will be spotted and used to make the new parameter match the cloned filter's one
            var template = MockRepository.GenerateStub<ISqlParameter>();
            template.Stub(x => x.ParameterName).Return("@bob");
            
            template.ParameterSQL = "DECLARE @bob AS int";
            template.Value = "5";
            template.Comment = "fish";

            var factory = MockRepository.GenerateMock<IFilterFactory>();
            factory.Expect(m => m.CreateNewParameter(f, "DECLARE @bob AS int")).Return(pstub).Repeat.Once();

            var creator = new ParameterCreator(factory, null, new []{template});
            creator.CreateAll(f,null);

            Assert.AreEqual("5", pstub.Value);
            Assert.AreEqual("fish", pstub.Comment); 

            factory.VerifyAllExpectations();
        }

        [TestCase("[MyTable].[MyCol] = @name", "@name", "@name2", "[MyTable].[MyCol] = @name2")]
        [TestCase("[Col]=@name OR [Col]=@name2", "@name", "@chtulhu", "[Col]=@chtulhu OR [Col]=@name2")]
        [TestCase("([MyTable].[MyCol] = @name) OR ...", "@name", "@name2", "([MyTable].[MyCol] = @name2) OR ...")]
        [TestCase("[MyTable].[MyCol] = @name2", "@name", "@cthulhu", "[MyTable].[MyCol] = @name2")]//No match since it is a substring
        [TestCase("[MyTable].[MyCol] = @name_2", "@name", "@cthulhu", "[MyTable].[MyCol] = @name_2")]
        [TestCase("[MyTable].[MyCol] = @name@@coconuts", "@name", "@cthulhu", "[MyTable].[MyCol] = @name@@coconuts")]//No match since @ is a legit word to use in a parameter name making @name@coconuts legal name for a 
        [TestCase("@a=a", "@a", "@b", "@b=a")]
        [TestCase(@"a=@a
    OR
b=@b", "@a", "@cthulhu", @"a=@cthulhu
    OR
b=@b")]
        public void ReplaceParametersSQL(string haystack, string needle, string replacement, string expectedOutput)
        {
            var output = ParameterCreator.RenameParameterInSQL(haystack, needle, replacement);
            Assert.AreEqual(expectedOutput,output);
        }

        [Test]
        public void SequentialReplacementSQL()
        {
            var haystack =
                @"/*Paracetamol*/
[test]..[prescribing].[approved_name] LIKE @drugName
OR
/*Ketamine*/
[test]..[prescribing].[approved_name] LIKE @drugName2
OR
/*Approved Name Like*/
[test]..[prescribing].[approved_name] LIKE @drugName3";


            var newString = ParameterCreator.RenameParameterInSQL(haystack, "@drugName", "@drugName_2");
            newString = ParameterCreator.RenameParameterInSQL(newString, "@drugName2", "@drugName2_2");
            newString = ParameterCreator.RenameParameterInSQL(newString, "@drugName3", "@drugName3_2");


            var expectedoutput =
                @"/*Paracetamol*/
[test]..[prescribing].[approved_name] LIKE @drugName_2
OR
/*Ketamine*/
[test]..[prescribing].[approved_name] LIKE @drugName2_2
OR
/*Approved Name Like*/
[test]..[prescribing].[approved_name] LIKE @drugName3_2";


            Assert.AreEqual(expectedoutput,newString);
        }
    }
}
