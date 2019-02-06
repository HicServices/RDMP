// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.FilterImporting;
using CatalogueLibrary.FilterImporting.Construction;
using Fansi.Implementations.MicrosoftSQL;
using NUnit.Framework;
using Rhino.Mocks;

namespace CatalogueLibraryTests.Integration.FilterImportingTests
{
    public class FilterImporterTests
    {
        
        [Test]
        public void FilterCreated_NewFilterGetsSameName()
        {
            //Thing we will be cloning
            var master = MockRepository.GenerateStub<IFilter>();
            master.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            master.Name = "Space Odyssey";
            
            //The factory will return this value
            var constructed = MockRepository.GenerateStub<IFilter>();
            constructed.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());

            //The factory Mock
            var factory = MockRepository.GenerateStrictMock<IFilterFactory>();
            factory.Expect(m => m.CreateNewFilter("Space Odyssey")).Return(constructed).Repeat.Once();

            //The thing we are testing
            var filterCreator = new FilterImporter(factory,null);
        
            //The method we are testing
            filterCreator.ImportFilter(master,null);

            //Did the factory mock get ordered to create a filter called "Space Odyssey"?
            factory.VerifyAllExpectations();
        }

        [Test]
        public void FilterCreated_CopyBecauseExistsAlready()
        {
            //The thing we will be importing
            var master = MockRepository.GenerateStub<IFilter>();
            master.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            master.Name = "Space Odyssey";

            //An existing IFilter that is in the scope that is being imported into (e.g. a data extract configuration)
            var existing = MockRepository.GenerateStub<IFilter>();
            existing.Name = "Space Odyssey";
            existing.Expect(m => m.GetAllParameters()).Return(new ISqlParameter[0]);// has no parameters

            //The factory will return this value
            var constructed = MockRepository.GenerateStub<IFilter>();
            constructed.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());

            //The factory Mock
            var factory = MockRepository.GenerateStrictMock<IFilterFactory>();
            factory.Expect(m => m.CreateNewFilter("Copy of Space Odyssey")).Return(constructed).Repeat.Once();

            //The thing we are testing
            var filterCreator = new FilterImporter(factory, null);

            //The method we are testing
            filterCreator.ImportFilter(master,new []{existing});

            //Did the factory mock get ordered to create a filter called "Copy of Space Odyssey" (because there was already one called "Space Odyssey" in the same scope)
            factory.VerifyAllExpectations();
        }

        [Test]
        public void FilterCreated_Parameters()
        {
            var master = MockRepository.GenerateStub<IFilter>();
            master.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            master.Name = "Space Odyssey";
            master.WhereSQL = "@hall = 'active'";
            
            var constructed = MockRepository.GenerateStub<IFilter>();
            constructed.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            var constructedParameter = MockRepository.GenerateStub<ISqlParameter>();

            var factory = MockRepository.GenerateStrictMock<IFilterFactory>();
            factory.Expect(m => m.CreateNewFilter("Space Odyssey")).Return(constructed).Repeat.Once();
            factory.Expect(m => m.CreateNewParameter(constructed, "DECLARE @hall AS varchar(50);")).Return(constructedParameter).Repeat.Once();

            var filterCreator = new FilterImporter(factory, null);
            //Returns constructed
            filterCreator.ImportFilter(master, null);
            factory.VerifyAllExpectations();
        }
        [Test]
        public void FilterCreated_ParametersWithMasterExplicitTyping()
        {
            //The filter we are cloning
            var master = MockRepository.GenerateStub<IFilter>();
            master.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            master.Name = "Space Odyssey";
            master.WhereSQL = "@hall = 'active'";

            //The existing parameter declared on the filter we are cloning
            var masterParameter = MockRepository.GenerateStub<ISqlParameter>();
            masterParameter.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            masterParameter.Stub(p => p.ParameterName).Return("@hall");
            masterParameter.Comment = "SomeComment";
            masterParameter.Value = "500";
            masterParameter.ParameterSQL = "DECLARE @hall AS int";

            //We expect that the filter we are cloning will be asked what it's parameters are once (and we tell them the param above)
            master.Expect(m => m.GetAllParameters()).Return(new[] {masterParameter}).Repeat.Once();


            //The return values for our Mock factory
            var constructed = MockRepository.GenerateStub<IFilter>();
            constructed.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            var constructedParameter = MockRepository.GenerateStub<ISqlParameter>();
            constructedParameter.ParameterSQL = "DECLARE @hall AS int";

            //The mock factory will return the above instances for the new cloned objects
            var factory = MockRepository.GenerateStrictMock<IFilterFactory>();
            factory.Expect(m => m.CreateNewFilter("Space Odyssey")).Return(constructed).Repeat.Once();
            factory.Expect(m => m.CreateNewParameter(constructed, "DECLARE @hall AS int")).Return(constructedParameter).Repeat.Once();

            //The thing we are actually testing
            var filterCreator = new FilterImporter(factory, null);
            filterCreator.ImportFilter(master, null);//Import it brah
            
            //Master filter should have been asked what it's parameters are
            master.VerifyAllExpectations();

            //factory should have been asked to create a new filter called "Space Odyssey" and a parameter with a declaration that matches the master filter SQL (i.e. 'AS int')
            factory.VerifyAllExpectations();

            //The master filter parameters should have been copied to the child
            Assert.AreEqual(constructedParameter.Comment, masterParameter.Comment);
            Assert.AreEqual(constructedParameter.ParameterSQL, masterParameter.ParameterSQL); //We actually manually set this above because that's the contract with "CreateNewParameter"
            Assert.AreEqual(constructedParameter.Value, masterParameter.Value);
        }

        [Test]
        public void FilterCreated_ParametersRenamingDueToExistingParameterInScopeWithSameName()
        {
            //The filter we are cloning
            var master = MockRepository.GenerateStub<IFilter>();
            master.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            master.Name = "Space Odyssey";
            master.WhereSQL = "@hall = 'active'";

            //An existing parameter that is in the scope that is being imported into
            var existingParameter = MockRepository.GenerateStub<ISqlParameter>();
            existingParameter.Stub(x => x.ParameterName).Return("@hall");

            //The filter to which the above existing parameter belongs
            var existing = MockRepository.GenerateStub<IFilter>();
            existing.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            existing.Name = "Space Odyssey";
            existing.Expect(m => m.GetAllParameters()).Return(new[] { existingParameter }).Repeat.Once();

            //The return value for our Mock factory
            var constructed = MockRepository.GenerateStub<IFilter>();
            constructed.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            var constructedParameter = MockRepository.GenerateStub<ISqlParameter>();
            
            //The mocked factory
            var factory = MockRepository.GenerateStrictMock<IFilterFactory>();
            factory.Expect(m => m.CreateNewFilter("Copy of Space Odyssey")).Return(constructed).Repeat.Once();
            factory.Expect(m => m.CreateNewParameter(constructed,"DECLARE @hall2 AS varchar(50);")).Return(constructedParameter).Repeat.Once();

            //The thing we are testing
            var filterCreator = new FilterImporter(factory, null);
            filterCreator.ImportFilter(master, new []{existing});

            //Existing filter in the scope should have been asked what it's parameters are
            existing.VerifyAllExpectations();

            //The factory should have been asked to create a filter called "Copy of Space Odyssey" and a parameter "@hall2" (because @hall already exists in the import into scope)
            factory.VerifyAllExpectations();
        }

        [Test]
        public void FilterCreated_ParametersRenamingDueToExistingParameterInScopeWithSameName_MasterContainsMasterParameter()
        {
            //The filter we are cloning
            var master = MockRepository.GenerateStub<IFilter>();
            master.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            master.Name = "Space Odyssey";
            master.WhereSQL = "@hall = 'active'";

            //The existing parameter declared on the filter we are cloning
            var masterParameter = MockRepository.GenerateStub<ISqlParameter>();
            masterParameter.Stub(p => p.ParameterName).Return("@hall");
            masterParameter.Comment = "SomeComment";
            masterParameter.Value = "500";
            masterParameter.ParameterSQL = "DECLARE @hall AS int";

            //We expect that the filter we are cloning will be asked what it's parameters are once (and we tell them the param above)
            master.Expect(m => m.GetAllParameters()).Return(new[] { masterParameter }).Repeat.Once();
            

            //An existing parameter that is in the scope that is being imported into
            var existingParameter = MockRepository.GenerateStub<ISqlParameter>();
            existingParameter.Stub(x => x.ParameterName).Return("@hall");

            //The filter to which the above existing parameter belongs
            var existing = MockRepository.GenerateStub<IFilter>();
            existing.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            existing.Name = "Space Odyssey";
            existing.Expect(m => m.GetAllParameters()).Return(new[] { existingParameter }).Repeat.Once();

            //The return value for our Mock factory
            var constructed = MockRepository.GenerateStub<IFilter>();
            constructed.Stub(x => x.GetQuerySyntaxHelper()).Return(new MicrosoftQuerySyntaxHelper());
            var constructedParameter = MockRepository.GenerateStub<ISqlParameter>();

            //The mocked factory
            var factory = MockRepository.GenerateStrictMock<IFilterFactory>();
            factory.Expect(m => m.CreateNewFilter("Copy of Space Odyssey")).Return(constructed).Repeat.Once();
            factory.Expect(m => m.CreateNewParameter(constructed, "DECLARE @hall2 AS int")).Return(constructedParameter).Repeat.Once();

            //The thing we are testing
            var filterCreator = new FilterImporter(factory, null);
            filterCreator.ImportFilter(master, new[] { existing });

            Assert.AreEqual("@hall2 = 'active'",constructed.WhereSQL);

            //Master filter should have been asked what it's parameters are
            master.VerifyAllExpectations();

            //Existing filter in the scope should have been asked what it's parameters are
            existing.VerifyAllExpectations();

            //The factory should have been asked to create a filter called "Copy of Space Odyssey" and a parameter "@hall2" (because @hall already exists in the import into scope) with type int because master parameter is type int
            factory.VerifyAllExpectations();
        }
    }
}


