using System;
using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueLibrary.ExternalDatabaseServerPatching;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction.Exceptions;
using NUnit.Framework;
using RDMPStartup;
using Tests.Common;

namespace CatalogueLibraryTests.Unit
{
    public class PluginBootstrapperTests : DatabaseTests
    {
        private class TestPluginPatcher : PluginPatcher
        {
            public TestPluginPatcher(CatalogueRepository catalogueRepository) : base(catalogueRepository)
            {
            }

            public override IExternalDatabaseServer[] FindDatabases(out Assembly hostAssembly, out Assembly dbAssembly)
            {
                throw new NotImplementedException();
            }
        }

        private class TestPluginPatcherWithNoConstructor : IPluginPatcher
        {
            public IExternalDatabaseServer[] FindDatabases(out Assembly hostAssembly, out Assembly dbAssembly)
            {
                throw new NotImplementedException();
            }
        }

        private class TestPluginPatcherWithNoValidConstructor : IPluginPatcher
        {
            public TestPluginPatcherWithNoValidConstructor(int a)
            {
            }

            public TestPluginPatcherWithNoValidConstructor(string a)
            {
            }

            public IExternalDatabaseServer[] FindDatabases(out Assembly hostAssembly, out Assembly dbAssembly)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void LoadPluginPatcher_PluginClassHasNoConstructor()
        {
            var bootstrapper = new PluginBootstrapper(CatalogueRepository);
            var ex = Assert.Throws<ObjectLacksCompatibleConstructorException>(() => bootstrapper.Create(typeof(TestPluginPatcherWithNoConstructor)));
            Assert.IsTrue(ex.Message.Contains("does not have a constructor taking"));
        }

        [Test]
        public void LoadPluginPatcher_PluginClassHasNoValidConstructor()
        {
            var bootstrapper = new PluginBootstrapper(CatalogueRepository);
            var ex = Assert.Throws<ObjectLacksCompatibleConstructorException>(() => bootstrapper.Create(typeof(TestPluginPatcherWithNoValidConstructor)));
            Assert.IsTrue(ex.Message.Contains("does not have a constructor taking"));
        }

        [Test]
        public void LoadPluginPatcher_ValidPluginClass()
        {
            CatalogueRepository.MEF.AddTypeToCatalogForTesting(typeof(TestPluginPatcher));
            var bootstrapper = new PluginBootstrapper(CatalogueRepository);
            Assert.DoesNotThrow(() => bootstrapper.Create(typeof(TestPluginPatcher)));
        }
    }

}