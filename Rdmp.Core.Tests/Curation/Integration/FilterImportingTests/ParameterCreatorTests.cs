// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi.Implementations.MicrosoftSQL;
using NSubstitute;
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
                var f = Substitute.For<IFilter>();
                f.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);

                var factory = Substitute.For<IFilterFactory>();
                factory.DidNotReceive().CreateNewParameter(Arg.Any<IFilter>(), Arg.Any<string>());

                var creator = new ParameterCreator(factory, Array.Empty<ISqlParameter>(), null);
                creator.CreateAll(f, null);

                factory.Received(1);
        }


        [Test]
        public void SingleParameterTest_NullReturnFromConstruct_Throws()
        {
                var f = Substitute.For<IFilter>();
                f.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);
                f.WhereSQL = "@bob = 'bob'";

                var factory = Substitute.For<IFilterFactory>();
                factory.CreateNewParameter(f, "DECLARE @bob AS varchar(50);").Returns(l => null);

                var creator = new ParameterCreator(factory, null, null);

                var ex = Assert.Throws<NullReferenceException>(() => creator.CreateAll(f, null));

        Assert.That(ex.Message, Does.StartWith("Parameter construction method returned null"));
        }

        [Test]
        public void SingleParameterTest_OneParameter_CreateCalled()
        {
                var p = Substitute.For<ISqlParameter>(); //save should be called because there is no VAlue on the parameter
                p.SaveToDatabase();

                var f = Substitute.For<IFilter>();
                f.WhereSQL = "@bob = 'bob'";
                f.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);
                var factory = Substitute.For<IFilterFactory>();
                factory.CreateNewParameter(f, "DECLARE @bob AS varchar(50);").Returns(p);

                var creator = new ParameterCreator(factory, null, null);
                creator.CreateAll(f, null);

                p.Received(2).SaveToDatabase();
                p.Received(1);
                factory.Received(1).CreateNewParameter(f, "DECLARE @bob AS varchar(50);");
        }

        [Test]
        public void SingleParameterTest_ParameterAlreadyExists_CreateNotCalled()
        {
                var p = Substitute.For<ISqlParameter>(); //save should be called because there is no VAlue on the parameter

                var existingParameter = Substitute.For<ISqlParameter>();
                existingParameter.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);
                existingParameter.ParameterName.Returns("@bob");

                var f = Substitute.For<IFilter>();
                f.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);
                f.WhereSQL.Returns("@bob = 'bob'");
                f.GetAllParameters().Returns(new[] { existingParameter });
                var factory = Substitute.For<IFilterFactory>();

                var creator = new ParameterCreator(factory, null, null);
                creator.CreateAll(f, null);
                creator.CreateAll(f, null);
                creator.CreateAll(f,
                    null); //no matter how many times we call create it shouldn't make more because there is already one

                p.DidNotReceive().SaveToDatabase();
                factory.Received(0).CreateNewParameter(f, Arg.Any<string>()); //should never be called because the filter already has
        }

        [Test]
        public void SingleParameterTest_GlobalOverrides_CreateNotCalled()
        {
            var f = Substitute.For<IFilter>();
            f.WhereSQL = "@bob = 'bob'";
            f.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);
            var global = Substitute.For<ISqlParameter>();
            global.ParameterName.Returns("@bob");
            var factory = Substitute.For<IFilterFactory>();
            factory.CreateNewParameter(Arg.Any<IFilter>(), Arg.Any<string>()).Returns(static x => throw new InvalidOperationException());

            var creator = new ParameterCreator(factory, new[] { global }, null);
            creator.CreateAll(f, null);

            factory.Received(1);
        }

        [Test]
        public void SingleParameterTest_GlobalButNotSameName_CreateCalled()
        {
                var f = Substitute.For<IFilter>();
                f.WhereSQL = "@bob = 'bob'";
                f.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);
                var global = Substitute.For<ISqlParameter>();
                global.ParameterName.Returns("@bob");

                var factory = Substitute.For<IFilterFactory>();
                factory.CreateNewParameter(f, "DECLARE @bob AS varchar(50);")
                    .Returns(Substitute.For<ISqlParameter>());

                var creator = new ParameterCreator(factory, null, null);
                creator.CreateAll(f, null);

                factory.Received(1).CreateNewParameter(f, "DECLARE @bob AS varchar(50);");
        }


        [Test]
        public void SingleParameterTest_Template_TemplateValuesUsed()
        {
                //The constructor returns
                var pstub = Substitute.For<ISqlParameter>();

                //The filter that requires that the parameters be created
                var f = Substitute.For<IFilter>();
                f.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);
                f.WhereSQL = "@bob = 'bob'";

                //The template which is an existing known about parameter from the master filter that is being duplicated.  This template will be spotted and used to make the new parameter match the cloned filter's one
                var template = Substitute.For<ISqlParameter>();
                template.ParameterName.Returns("@bob");

                template.ParameterSQL = "DECLARE @bob AS int";
                template.Value = "5";
                template.Comment = "fish";

                var factory = Substitute.For<IFilterFactory>();
                factory.CreateNewParameter(f, "DECLARE @bob AS int").Returns(pstub);

                var creator = new ParameterCreator(factory, null, new[] { template });
                creator.CreateAll(f, null);

        Assert.Multiple(() =>
        {
            Assert.That(pstub.Value, Is.EqualTo("5"));
            Assert.That(pstub.Comment, Is.EqualTo("fish"));
        });

        factory.Received(1).CreateNewParameter(f, "DECLARE @bob AS int");
        }

        [TestCase("[MyTable].[MyCol] = @name", "@name", "@name2", "[MyTable].[MyCol] = @name2")]
        [TestCase("[Col]=@name OR [Col]=@name2", "@name", "@chtulhu", "[Col]=@chtulhu OR [Col]=@name2")]
        [TestCase("([MyTable].[MyCol] = @name) OR ...", "@name", "@name2", "([MyTable].[MyCol] = @name2) OR ...")]
        [TestCase("[MyTable].[MyCol] = @name2", "@name", "@cthulhu",
            "[MyTable].[MyCol] = @name2")] //No match since it is a substring
        [TestCase("[MyTable].[MyCol] = @name_2", "@name", "@cthulhu", "[MyTable].[MyCol] = @name_2")]
        [TestCase("[MyTable].[MyCol] = @name@@coconuts", "@name", "@cthulhu",
            "[MyTable].[MyCol] = @name@@coconuts")] //No match since @ is a legit word to use in a parameter name making @name@coconuts legal name for a
        [TestCase("@a=a", "@a", "@b", "@b=a")]
        [TestCase(@"a=@a
    OR
b=@b", "@a", "@cthulhu", @"a=@cthulhu
    OR
b=@b")]
        public void ReplaceParametersSQL(string haystack, string needle, string replacement, string expectedOutput)
        {
            var output = ParameterCreator.RenameParameterInSQL(haystack, needle, replacement);
            Assert.That(output, Is.EqualTo(expectedOutput));
        }

        [Test]
        public void SequentialReplacementSQL()
        {
            const string haystack = """
                                    /*Paracetamol*/
                                    [test]..[prescribing].[approved_name] LIKE @drugName
                                    OR
                                    /*Ketamine*/
                                    [test]..[prescribing].[approved_name] LIKE @drugName2
                                    OR
                                    /*Approved Name Like*/
                                    [test]..[prescribing].[approved_name] LIKE @drugName3
                                    """;


            var newString = ParameterCreator.RenameParameterInSQL(haystack, "@drugName", "@drugName_2");
            newString = ParameterCreator.RenameParameterInSQL(newString, "@drugName2", "@drugName2_2");
            newString = ParameterCreator.RenameParameterInSQL(newString, "@drugName3", "@drugName3_2");


            const string expectedoutput = """
                                          /*Paracetamol*/
                                          [test]..[prescribing].[approved_name] LIKE @drugName_2
                                          OR
                                          /*Ketamine*/
                                          [test]..[prescribing].[approved_name] LIKE @drugName2_2
                                          OR
                                          /*Approved Name Like*/
                                          [test]..[prescribing].[approved_name] LIKE @drugName3_2
                                          """;


            Assert.That(newString, Is.EqualTo(expectedoutput));
        }
}