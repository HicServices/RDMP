using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using NUnit.Framework;
using Rhino.Mocks;

namespace CatalogueLibraryTests.Unit
{
    public interface ITestMef
    {
        DirectoryInfo RootDirectory { get; }
    }

    [Export]
    public class TestMefImporter
    {
        [ImportMany]
        public List<Lazy<ITestMef>> TestMefList { get; set; }
    }

    [Export(typeof(ITestMef))]
    public class TestMef : ITestMef
    {
        public TestMef()
        {
            RootDirectory = null;
        }

        [ImportingConstructor]
        public TestMef(DirectoryInfo rootDirectory)
        {
            RootDirectory = rootDirectory;
        }

        public DirectoryInfo RootDirectory { get; private set; }
    }

    [Category("Unit")]
    public class LoadModuleAssemblyTests
    {
        [Test]
        public void TestMefWithConstructorParam()
        {
            var expectedConstructorParameterValue = new DirectoryInfo(".");
            var expectedPath = expectedConstructorParameterValue.FullName;

            var catalog = new TypeCatalog(typeof(TestMef), typeof(TestMefImporter));
            var container = new CompositionContainer(catalog);
            
            container.ComposeExportedValue<DirectoryInfo>(expectedConstructorParameterValue);
            
            var importer = container.GetExportedValue<TestMefImporter>();

            Assert.AreEqual(1, importer.TestMefList.Count, "Expect a single ITestMef import in the list");
            Assert.AreEqual(expectedPath, importer.TestMefList[0].Value.RootDirectory.FullName);
        }


        [Test]
        public void TestWithoutSeparateImporterClass()
        {
            var expectedConstructorParameterValue = new DirectoryInfo(".");
            var expectedPath = expectedConstructorParameterValue.FullName;

            var catalog = new TypeCatalog(typeof(TestMef));
            var container = new CompositionContainer(catalog);

            container.ComposeExportedValue<DirectoryInfo>(expectedConstructorParameterValue);

            var testMef = container.GetExportedValue<ITestMef>();

            Assert.AreEqual(expectedPath, testMef.RootDirectory.FullName);
        }
    }

    public class MefTest
    {
        [Test]
        public void TestConstructorInjectionWithMultipleConstructors()
        {
            var expectedConstructorParameterValue = new DirectoryInfo(".");
            var expectedFilepath = expectedConstructorParameterValue.FullName;

            var catalog = new TypeCatalog(typeof(Foo), typeof(FooImporter));
            var container = new CompositionContainer(catalog);

            container.ComposeExportedValue<DirectoryInfo>(expectedConstructorParameterValue);
            container.ComposeExportedValue<string>("bar");

            var fooImporter = container.GetExportedValue<FooImporter>();

            Assert.AreEqual(1, fooImporter.FooList.Count, "Expect a single IFoo import in the list");

            var foo = fooImporter.FooList[0].Value;
            Assert.AreEqual(expectedFilepath, foo.Directory.FullName, "Expected foo's ConstructorParameter to have the correct value.");
            Assert.AreEqual("bar", foo.Parameter2);
        }
    }
    

    public interface IFoo
    {
        DirectoryInfo Directory { get; }
        string Parameter2 { get; }
    }

    [Export(typeof(IFoo))]
    public class Foo : IFoo
    {
        

        public Foo()
        {
            this.Directory = null;
            this.Parameter2 = "";
        }

        [ImportingConstructor]
        public Foo(DirectoryInfo directory, string parameter2)
        {
            Parameter2 = parameter2;
            this.Directory = directory;
        }


        public DirectoryInfo Directory { get; private set; }
        public string Parameter2 { get; private set; }
    }

    [Export]
    public class FooImporter
    {
        [ImportMany]
        public List<Lazy<IFoo>> FooList { get; set; }
    }
}
