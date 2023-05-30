// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi.Implementations.MicrosoftSQL;
using Moq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Curation.FilterImporting.Construction;

namespace Rdmp.Core.Tests.Curation.Integration.FilterImportingTests;

[Category("Unit")]
public class ParameterCreatorTests
{
    [Test]
    public void NoParametersTest_CreateNotCalled()
    {
        var f = Mock.Of<IFilter>(x => x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance);

        var factory = new Mock<IFilterFactory>();
        factory.Verify(m => m.CreateNewParameter(It.IsAny<IFilter>(), It.IsAny<string>()),Times.Never);

        var creator = new ParameterCreator(factory.Object, Array.Empty<ISqlParameter>(), null);
        creator.CreateAll(f,null);

        factory.Verify();
    }
        

    [Test]
    public void SingleParameterTest_NullReturnFromConstruct_Throws()
    {
        var f = Mock.Of<IFilter>(x => x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance);
        f.WhereSQL = "@bob = 'bob'";
            
        var factory = Mock.Of<IFilterFactory>(m => m.CreateNewParameter(f,"DECLARE @bob AS varchar(50);")==null);

        var creator = new ParameterCreator(factory, null, null);

        var ex = Assert.Throws<NullReferenceException>(()=>creator.CreateAll(f,null));

        Assert.IsTrue(ex.Message.StartsWith("Parameter construction method returned null"));
    }
       
    [Test]
    public void SingleParameterTest_OneParameter_CreateCalled()
    {
        var p = new Mock<ISqlParameter>();//save should be called because there is no VAlue on the parameter
        p.Setup(m => m.SaveToDatabase());

        var f = Mock.Of<IFilter>(x => x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance);
        f.WhereSQL = "@bob = 'bob'";

        var factory = new Mock<IFilterFactory>();
        factory.Setup(m => m.CreateNewParameter(f,"DECLARE @bob AS varchar(50);")).Returns(p.Object);
            
        var creator = new ParameterCreator(factory.Object, null, null);
        creator.CreateAll(f,null);

        p.Verify(m => m.SaveToDatabase(),Times.Once);
        p.Verify();
        factory.Verify(m => m.CreateNewParameter(f,"DECLARE @bob AS varchar(50);"),Times.Once);
    }
    [Test]
    public void SingleParameterTest_ParameterAlreadyExists_CreateNotCalled()
    {
        var p = new Mock<ISqlParameter>();//save should be called because there is no VAlue on the parameter
        p.Setup(m => m.SaveToDatabase());

        var existingParameter = Mock.Of<ISqlParameter>(x => 
            x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance &&
            x.ParameterName=="@bob"
        );

        var f = Mock.Of<IFilter>(x =>
            x.GetQuerySyntaxHelper() == MicrosoftQuerySyntaxHelper.Instance &&
            x.WhereSQL == "@bob = 'bob'" && 
            x.GetAllParameters() == new[] {existingParameter});

        var factory = new Mock<IFilterFactory>();
            
        var creator = new ParameterCreator(factory.Object, null, null);
        creator.CreateAll(f,null);
        creator.CreateAll(f, null);
        creator.CreateAll(f, null);//no matter how many times we call create it shouldn't make more because there is already one

        p.Verify(m=> m.SaveToDatabase(),Times.Never);
        factory.Verify(m=> m.CreateNewParameter(f, It.IsAny<string>()),Times.Never); //should never be called because the filter already has 
    }
        
    [Test]
    public void SingleParameterTest_GlobalOverrides_CreateNotCalled()
    {
        var f = Mock.Of<IFilter>(x => x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance);
        f.WhereSQL = "@bob = 'bob'";

        var global = Mock.Of<ISqlParameter>(x=>x.ParameterName=="@bob");

        var factory = new Mock<IFilterFactory>();
        factory
            .Setup(m => m.CreateNewParameter(It.IsAny<IFilter>(), It.IsAny<string>()))
            .Throws<InvalidOperationException>();

        var creator = new ParameterCreator(factory.Object, new[] { global }, null);
        creator.CreateAll(f,null);

        factory.Verify();
    }

    [Test]
    public void SingleParameterTest_GlobalButNotSameName_CreateCalled()
    {
        var f = Mock.Of<IFilter>(x => x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance);
        f.WhereSQL = "@bob = 'bob'";

        var global = Mock.Of<ISqlParameter>(x => x.ParameterName=="@bob");
            
        var factory = new Mock<IFilterFactory>();
        factory.Setup<ISqlParameter>(m => m.CreateNewParameter(f, "DECLARE @bob AS varchar(50);")).Returns(Mock.Of<ISqlParameter>);
            
        var creator = new ParameterCreator(factory.Object, null, null);
        creator.CreateAll(f,null);

        factory.Verify<ISqlParameter>(m => m.CreateNewParameter(f, "DECLARE @bob AS varchar(50);"),Times.Once);

    }


    [Test]
    public void SingleParameterTest_Template_TemplateValuesUsed()
    {
        //The constructor returns
        var pstub = Mock.Of<ISqlParameter>();
            
        //The filter that requires that the parameters be created
        var f = Mock.Of<IFilter>(x => x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance);
        f.WhereSQL = "@bob = 'bob'";

        //The template which is an existing known about parameter from the master filter that is being duplicated.  This template will be spotted and used to make the new parameter match the cloned filter's one
        var template = Mock.Of<ISqlParameter>(x => x.ParameterName=="@bob");
            
        template.ParameterSQL = "DECLARE @bob AS int";
        template.Value = "5";
        template.Comment = "fish";

        var factory = new Mock<IFilterFactory>();
        factory.Setup(m => m.CreateNewParameter(f, "DECLARE @bob AS int")).Returns(pstub);
            
        var creator = new ParameterCreator(factory.Object, null, new []{template});
        creator.CreateAll(f,null);

        Assert.AreEqual("5", pstub.Value);
        Assert.AreEqual("fish", pstub.Comment); 

        factory.Verify(m => m.CreateNewParameter(f, "DECLARE @bob AS int"),Times.Once);
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