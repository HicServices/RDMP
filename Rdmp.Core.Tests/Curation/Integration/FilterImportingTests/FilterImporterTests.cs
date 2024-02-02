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
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration.FilterImportingTests;

[Category("Unit")]
public class FilterImporterTests : UnitTests
{
        [Test]
        public void FilterCreated_NewFilterGetsSameName()
        {
                //Thing we will be cloning
                var master = Substitute.For<IFilter>();
                master.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);

                master.Name.Returns("Space Odyssey");

                //The factory will return this value
                var constructed = Substitute.For<IFilter>();
                constructed.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);


                //The factory Mock
                var factory = Substitute.For<IFilterFactory>();
                factory.CreateNewFilter("Space Odyssey").Returns(constructed);

                //The thing we are testing
                var filterCreator = new FilterImporter(factory, null);

                //The method we are testing
                filterCreator.ImportFilter(WhenIHaveA<AggregateFilterContainer>(), master, null);

                //Did the factory mock get ordered to create a filter called "Space Odyssey"?
                factory.Received(1).CreateNewFilter(Arg.Any<string>());
        }

        [Test]
        public void FilterCreated_CopyBecauseExistsAlready()
        {
                //The thing we will be importing
                var master = Substitute.For<IFilter>();
                master.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);

                master.Name = "Space Odyssey";

                //An existing IFilter that is in the scope that is being imported into (e.g. a data extract configuration)
                var existing = Substitute.For<IFilter>(); // has no parameters
                existing.GetAllParameters().Returns(Array.Empty<ISqlParameter>());
                existing.Name.Returns("Space Odyssey");
                //The factory will return this value
                var constructed = Substitute.For<IFilter>();
                constructed.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);


                //The factory Mock
                var factory = Substitute.For<IFilterFactory>();
                factory.CreateNewFilter("Copy of Space Odyssey").Returns(constructed);

                //The thing we are testing
                var filterCreator = new FilterImporter(factory, null);

                //The method we are testing
                filterCreator.ImportFilter(WhenIHaveA<AggregateFilterContainer>(), master, new[] { existing });

                //Did the factory mock get ordered to create a filter called "Copy of Space Odyssey" (because there was already one called "Space Odyssey" in the same scope)
                factory.Received(1);
        }

        [Test]
        public void FilterCreated_Parameters()
        {
                var master = Substitute.For<IFilter>();
                master.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);

                master.Name = "Space Odyssey";
                master.WhereSQL = "@hall = 'active'";

                var constructed = Substitute.For<IFilter>();
                constructed.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);

                var constructedParameter = Substitute.For<ISqlParameter>();

                var factory = Substitute.For<IFilterFactory>();
                factory.CreateNewFilter("Space Odyssey").Returns(constructed);
                factory.CreateNewParameter(constructed, "DECLARE @hall AS varchar(50);")
                    .Returns(constructedParameter);

                var filterCreator = new FilterImporter(factory, null);
                //Returns constructed
                filterCreator.ImportFilter(WhenIHaveA<AggregateFilterContainer>(), master, null);

                factory.Received(1).CreateNewFilter("Space Odyssey");
                factory.Received(1).CreateNewParameter(constructed, "DECLARE @hall AS varchar(50);");
        }

        [Test]
        public void FilterCreated_ParametersWithMasterExplicitTyping()
        {
                //The filter we are cloning
                var master = Substitute.For<IFilter>();
                master.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);
                master.Name.Returns("Space Odyssey");
                master.WhereSQL.Returns("@hall = 'active'");

                //The existing parameter declared on the filter we are cloning
                var masterParameter = Substitute.For<ISqlParameter>();
                masterParameter.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);
                masterParameter.ParameterName.Returns("@hall");
                masterParameter.Comment.Returns("SomeComment");
                masterParameter.Value.Returns("500");
                masterParameter.ParameterSQL.Returns("DECLARE @hall AS int");

                master.GetAllParameters().Returns(new[] { masterParameter });
                //We expect that the filter we are cloning will be asked what its parameters are once (and we tell them the param above)


                //The return values for our Mock factory
                var constructed = Substitute.For<IFilter>();
                constructed.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);
                var constructedParameter = Substitute.For<ISqlParameter>();
                constructedParameter.ParameterSQL = "DECLARE @hall AS int";

                //The mock factory will return the above instances for the new cloned objects
                var factory = Substitute.For<IFilterFactory>();
                factory.CreateNewFilter("Space Odyssey").Returns(constructed);
                factory.CreateNewParameter(constructed, "DECLARE @hall AS int").Returns(constructedParameter);

                //The thing we are actually testing
                var filterCreator = new FilterImporter(factory, null);
                filterCreator.ImportFilter(WhenIHaveA<AggregateFilterContainer>(), master, null); //Import it brah

                //Master filter should have been asked what its parameters are
                master.Received(1);

                //factory should have been asked to create a new filter called "Space Odyssey" and a parameter with a declaration that matches the master filter SQL (i.e. 'AS int')
                factory.Received(1).CreateNewFilter("Space Odyssey");

        Assert.Multiple(() =>
        {
            //The master filter parameters should have been copied to the child
            Assert.That(masterParameter.Comment, Is.EqualTo(constructedParameter.Comment));
            Assert.That(masterParameter
                            .ParameterSQL, Is.EqualTo(constructedParameter.ParameterSQL)); //We actually manually set this above because that's the contract with "CreateNewParameter"
            Assert.That(masterParameter.Value, Is.EqualTo(constructedParameter.Value));
        });
    }

        [Test]
        public void FilterCreated_ParametersRenamingDueToExistingParameterInScopeWithSameName()
        {
                //The filter we are cloning
                var master = Substitute.For<IFilter>();
                master.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);
                master.Name = "Space Odyssey";
                master.WhereSQL = "@hall = 'active'";

                //An existing parameter that is in the scope that is being imported into
                var existingParameter = Substitute.For<ISqlParameter>();
                existingParameter.ParameterName.Returns("@hall");

                //The filter to which the above existing parameter belongs
                var existing = Substitute.For<IFilter>();
                existing.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);
                existing.GetAllParameters().Returns(new[] { existingParameter });

                existing.Name = "Space Odyssey";

                //The return value for our Mock factory
                var constructed = Substitute.For<IFilter>();
                constructed.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);
                var constructedParameter = Substitute.For<ISqlParameter>();

                //The mocked factory
                var factory = Substitute.For<IFilterFactory>();
                factory.CreateNewFilter("Copy of Space Odyssey").Returns(constructed);
                factory.CreateNewParameter(constructed, "DECLARE @hall2 AS varchar(50);")
                    .Returns(constructedParameter);

                //The thing we are testing
                var filterCreator = new FilterImporter(factory, null);
                filterCreator.ImportFilter(WhenIHaveA<AggregateFilterContainer>(), master, new[] { existing });

                //Existing filter in the scope should have been asked what its parameters are
                existing.Received(1).GetAllParameters();

                //The factory should have been asked to create a filter called "Copy of Space Odyssey" and a parameter "@hall2" (because @hall already exists in the import into scope)
                factory.Received(1);
        }

        [Test]
        public void
            FilterCreated_ParametersRenamingDueToExistingParameterInScopeWithSameName_MasterContainsMasterParameter()
        {
                //The filter we are cloning
                var master = Substitute.For<IFilter>();
                master.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);
                master.Name = "Space Odyssey";
                master.WhereSQL = "@hall = 'active'";

                //The existing parameter declared on the filter we are cloning
                var masterParameter = Substitute.For<ISqlParameter>();
                masterParameter.ParameterName.Returns("@hall");
                masterParameter.Comment.Returns("SomeComment");
                masterParameter.Value.Returns("400");
                masterParameter.ParameterSQL.Returns("DECLARE @hall AS int");

                //We expect that the filter we are cloning will be asked what its parameters are once (and we tell them the param above)
                master.GetAllParameters().Returns(new[] { masterParameter });

                //An existing parameter that is in the scope that is being imported into
                var existingParameter = Substitute.For<ISqlParameter>();
                existingParameter.ParameterName.Returns("@hall");

                //The filter to which the above existing parameter belongs
                var existing = Substitute.For<IFilter>();
                existing.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);
                existing.GetAllParameters().Returns(new[] { existingParameter });
                existing.Name.Returns("Space Odyssey");

                //The return value for our Mock factory
                var constructed = Substitute.For<IFilter>();
                constructed.GetQuerySyntaxHelper().Returns(MicrosoftQuerySyntaxHelper.Instance);
                var constructedParameter = Substitute.For<ISqlParameter>();

                //The mocked factory
                var factory = Substitute.For<IFilterFactory>();
                factory.CreateNewFilter("Copy of Space Odyssey").Returns(constructed);
                factory.CreateNewParameter(constructed, "DECLARE @hall2 AS int").Returns(constructedParameter);

                //The thing we are testing
                var filterCreator = new FilterImporter(factory, null);
                filterCreator.ImportFilter(WhenIHaveA<AggregateFilterContainer>(), master, new[] { existing });

        Assert.That(constructed.WhereSQL, Is.EqualTo("@hall2 = 'active'"));

                //Master filter should have been asked what its parameters are
                master.Received(1).GetAllParameters();

                //Existing filter in the scope should have been asked what its parameters are
                existing.Received(1);

                //The factory should have been asked to create a filter called "Copy of Space Odyssey" and a parameter "@hall2" (because @hall already exists in the import into scope) with type int because master parameter is type int
                factory.Received(1);
        }
}