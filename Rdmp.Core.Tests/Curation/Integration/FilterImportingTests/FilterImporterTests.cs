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
        var master = Mock.Of<IFilter>(x => 
            x.GetQuerySyntaxHelper() == MicrosoftQuerySyntaxHelper.Instance && 
            x.Name == "Space Odyssey");
            
        //The factory will return this value
        var constructed = Mock.Of<IFilter>(x => x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance);

        //The factory Mock
        var factory = new Mock<IFilterFactory>();
        factory.Setup(m => m.CreateNewFilter("Space Odyssey")).Returns(constructed);

        //The thing we are testing
        var filterCreator = new FilterImporter(factory.Object,null);
        
        //The method we are testing
        filterCreator.ImportFilter(WhenIHaveA<AggregateFilterContainer>(),master,null);

        //Did the factory mock get ordered to create a filter called "Space Odyssey"?
        factory.Verify(f=>f.CreateNewFilter(It.IsAny<string>()),Times.Once);
    }

    [Test]
    public void FilterCreated_CopyBecauseExistsAlready()
    {
        //The thing we will be importing
        var master = Mock.Of<IFilter>(x => x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance);
        master.Name = "Space Odyssey";

        //An existing IFilter that is in the scope that is being imported into (e.g. a data extract configuration)
        var existing = Mock.Of<IFilter>(f=>
            f.Name == "Space Odyssey" &&
            f.GetAllParameters()==Array.Empty<ISqlParameter>());// has no parameters

        //The factory will return this value
        var constructed = Mock.Of<IFilter>(x => x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance);

        //The factory Mock
        var factory = new Mock<IFilterFactory>();
        factory.Setup(m => m.CreateNewFilter("Copy of Space Odyssey")).Returns(constructed);

        //The thing we are testing
        var filterCreator = new FilterImporter(factory.Object, null);

        //The method we are testing
        filterCreator.ImportFilter(WhenIHaveA<AggregateFilterContainer>(), master,new []{existing});

        //Did the factory mock get ordered to create a filter called "Copy of Space Odyssey" (because there was already one called "Space Odyssey" in the same scope)
        factory.Verify();
    }

    [Test]
    public void FilterCreated_Parameters()
    {
        var master = Mock.Of<IFilter>(x => x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance);
        master.Name = "Space Odyssey";
        master.WhereSQL = "@hall = 'active'";
            
        var constructed = Mock.Of<IFilter>(x => x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance);
        var constructedParameter = Mock.Of<ISqlParameter>();

        var factory = new Mock<IFilterFactory>();
        factory.Setup(m => m.CreateNewFilter("Space Odyssey")).Returns(constructed);
        factory.Setup(m => m.CreateNewParameter(constructed, "DECLARE @hall AS varchar(50);")).Returns(constructedParameter);

        var filterCreator = new FilterImporter(factory.Object, null);
        //Returns constructed
        filterCreator.ImportFilter(WhenIHaveA<AggregateFilterContainer>(), master, null);
            
        factory.Verify(m => m.CreateNewFilter("Space Odyssey"),Times.Once);
        factory.Verify(m=>m.CreateNewParameter(constructed, "DECLARE @hall AS varchar(50);"), Times.Once);
    }
    [Test]
    public void FilterCreated_ParametersWithMasterExplicitTyping()
    {
        //The filter we are cloning
        var master = Mock.Of<IFilter>(x =>
            x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance && 
            x.Name == "Space Odyssey" && 
            x.WhereSQL == "@hall = 'active'");

        //The existing parameter declared on the filter we are cloning
        var masterParameter = Mock.Of<ISqlParameter>(
            x => x.GetQuerySyntaxHelper() == MicrosoftQuerySyntaxHelper.Instance &&
                 x.ParameterName=="@hall" && 
                 x.Comment == "SomeComment" &&
                 x.Value == "500" &&
                 x.ParameterSQL == "DECLARE @hall AS int"
        );

        Mock.Get(master).Setup(m=> m.GetAllParameters()).Returns(new[] {masterParameter});
        //We expect that the filter we are cloning will be asked what its parameters are once (and we tell them the param above)
            
            
        //The return values for our Mock factory
        var constructed = Mock.Of<IFilter>(x => x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance);
        var constructedParameter = Mock.Of<ISqlParameter>();
        constructedParameter.ParameterSQL = "DECLARE @hall AS int";

        //The mock factory will return the above instances for the new cloned objects
        var factory = Mock.Of<IFilterFactory>( m=>
            m.CreateNewFilter("Space Odyssey")==constructed &&
            m.CreateNewParameter(constructed, "DECLARE @hall AS int")==constructedParameter                );

        //The thing we are actually testing
        var filterCreator = new FilterImporter(factory, null);
        filterCreator.ImportFilter(WhenIHaveA<AggregateFilterContainer>(), master, null);//Import it brah
            
        //Master filter should have been asked what its parameters are
        Mock.Get(master).Verify();

        //factory should have been asked to create a new filter called "Space Odyssey" and a parameter with a declaration that matches the master filter SQL (i.e. 'AS int')
        Mock.Get(factory).Verify(m=>m.CreateNewFilter("Space Odyssey"),Times.Once);

        //The master filter parameters should have been copied to the child
        Assert.AreEqual(constructedParameter.Comment, masterParameter.Comment);
        Assert.AreEqual(constructedParameter.ParameterSQL, masterParameter.ParameterSQL); //We actually manually set this above because that's the contract with "CreateNewParameter"
        Assert.AreEqual(constructedParameter.Value, masterParameter.Value);
    }

    [Test]
    public void FilterCreated_ParametersRenamingDueToExistingParameterInScopeWithSameName()
    {
        //The filter we are cloning
        var master = Mock.Of<IFilter>(x => x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance);
        master.Name = "Space Odyssey";
        master.WhereSQL = "@hall = 'active'";

        //An existing parameter that is in the scope that is being imported into
        var existingParameter = Mock.Of<ISqlParameter>(x => x.ParameterName=="@hall");

        //The filter to which the above existing parameter belongs
        var existing = Mock.Of<IFilter>(x => 
            x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance&&
            x.GetAllParameters()==new[] { existingParameter });
        existing.Name = "Space Odyssey";
            
        //The return value for our Mock factory
        var constructed = Mock.Of<IFilter>(x => x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance);
        var constructedParameter = Mock.Of<ISqlParameter>();
            
        //The mocked factory
        var factory = new Mock<IFilterFactory>();
        factory.Setup(m => m.CreateNewFilter("Copy of Space Odyssey")).Returns(constructed);
        factory.Setup(m => m.CreateNewParameter(constructed,"DECLARE @hall2 AS varchar(50);")).Returns(constructedParameter);

        //The thing we are testing
        var filterCreator = new FilterImporter(factory.Object, null);
        filterCreator.ImportFilter(WhenIHaveA<AggregateFilterContainer>(), master, new []{existing});

        //Existing filter in the scope should have been asked what its parameters are
        Mock.Get(existing).Verify(x=>x.GetAllParameters(),Times.Once);

        //The factory should have been asked to create a filter called "Copy of Space Odyssey" and a parameter "@hall2" (because @hall already exists in the import into scope)
        factory.Verify();
    }

    [Test]
    public void FilterCreated_ParametersRenamingDueToExistingParameterInScopeWithSameName_MasterContainsMasterParameter()
    {
        //The filter we are cloning
        var master = Mock.Of<IFilter>(x => x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance);
        master.Name = "Space Odyssey";
        master.WhereSQL = "@hall = 'active'";

        //The existing parameter declared on the filter we are cloning
        var masterParameter = Mock.Of<ISqlParameter>(p => p.ParameterName=="@hall");
        masterParameter.Comment = "SomeComment";
        masterParameter.Value = "500";
        masterParameter.ParameterSQL = "DECLARE @hall AS int";

        //We expect that the filter we are cloning will be asked what its parameters are once (and we tell them the param above)
        Mock.Get(master).Setup(m => m.GetAllParameters()).Returns(new[] { masterParameter });
            
        //An existing parameter that is in the scope that is being imported into
        var existingParameter = Mock.Of<ISqlParameter>(x => x.ParameterName=="@hall");

        //The filter to which the above existing parameter belongs
        var existing = Mock.Of<IFilter>(x => 
            x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance &&
            x.GetAllParameters()==new[] { existingParameter });
        existing.Name = "Space Odyssey";

        //The return value for our Mock factory
        var constructed = Mock.Of<IFilter>(x => x.GetQuerySyntaxHelper()==MicrosoftQuerySyntaxHelper.Instance);
        var constructedParameter = Mock.Of<ISqlParameter>();

        //The mocked factory
        var factory = new Mock<IFilterFactory>();
        factory.Setup(m => m.CreateNewFilter("Copy of Space Odyssey")).Returns(constructed);
        factory.Setup(m => m.CreateNewParameter(constructed, "DECLARE @hall2 AS int")).Returns(constructedParameter);

        //The thing we are testing
        var filterCreator = new FilterImporter(factory.Object, null);
        filterCreator.ImportFilter(WhenIHaveA<AggregateFilterContainer>(), master, new[] { existing });

        Assert.AreEqual("@hall2 = 'active'",constructed.WhereSQL);

        //Master filter should have been asked what its parameters are
        Mock.Get(master).Verify(m => m.GetAllParameters(),Times.Once);

        //Existing filter in the scope should have been asked what its parameters are
        Mock.Get(existing).Verify();

        //The factory should have been asked to create a filter called "Copy of Space Odyssey" and a parameter "@hall2" (because @hall already exists in the import into scope) with type int because master parameter is type int
        factory.Verify();
    }
}