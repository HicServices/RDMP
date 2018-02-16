using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using CommitAssemblyEmptyAssembly;
using DataLoadEngine.Attachers;
using DataLoadEngine.DataProvider;
using NUnit.Framework;
using ReusableLibraryCode;
using Rhino.Mocks;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class CommitAssemblyTest : DatabaseTests
    {
        byte[] _testDllBytes;
        FileInfo _testDll;
        
        [TestFixtureSetUp]
        protected override void SetUp()
        {
            Console.WriteLine( "Assembly is:"+ typeof (CommitAssemblyEmptyAssembly.MyExampleAttacherFor_CommitAssemblyTest).Assembly);

            base.SetUp();
            
            //move the test dll out of the bin directory
            if(File.Exists(".\\Integration\\CommitAssemblyEmptyAssembly.dll"))
                File.Delete(".\\Integration\\CommitAssemblyEmptyAssembly.dll");

            if (!Directory.Exists(".\\Integration\\"))
                Directory.CreateDirectory(".\\Integration\\");

            Console.Write("About to move ");

            File.Move(".\\CommitAssemblyEmptyAssembly.dll", ".\\Integration\\CommitAssemblyEmptyAssembly.dll");

            //find the test dll which contains 2 example (empty) IAttacher and IDataProviders
            _testDll = new FileInfo(".\\Integration\\CommitAssemblyEmptyAssembly.dll");

            //tell console about the file then upload it into database
            Console.WriteLine("Setup thought that the test dll was:" + _testDll.FullName);

            var remnant = CatalogueRepository.GetAllObjects<Plugin>().SingleOrDefault(p => p.Name.Equals("Imaginary.Zip"));

            if(remnant != null)
                remnant.DeleteInDatabase();

            Plugin plugin = new Plugin(CatalogueRepository, new FileInfo("Imaginary.Zip").Name, new FileInfo("Imaginary.Zip").DirectoryName);
            Assert.AreEqual(plugin.Name, "Imaginary.Zip");

            new LoadModuleAssembly(CatalogueRepository, _testDll, plugin);
            
            //now delete it from the bin directory (but first get all bytes so in teardown we can write it back out again into the bin dir
            _testDllBytes = File.ReadAllBytes(_testDll.FullName);

            try
            {
                _testDll.Delete();

            }
            catch (UnauthorizedAccessException)
            {
                
            }          
            
            //confirm it now exists only in database
            Assert.IsTrue(CatalogueRepository.GetAllObjects<LoadModuleAssembly>().Any(lma => lma.Name.Equals("CommitAssemblyEmptyAssembly.dll")));
            
            //make sure it is not in the bin folder too
            if(File.Exists(".\\CommitAssemblyEmptyAssembly.dll"))
                File.Delete(".\\CommitAssemblyEmptyAssembly.dll");

            plugin.DeleteInDatabase();
        }

        [TestFixtureTearDown]
        protected void TearDown()
        {
            //make sure it exists in the bin directory so that repeat runs of this test work
            if (!File.Exists(".\\CommitAssemblyEmptyAssembly.dll"))
                File.WriteAllBytes(".\\CommitAssemblyEmptyAssembly.dll", _testDllBytes);
        }


        [Test]
        public void UploadWithPattern()
        {
            Uri codeBase = new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            FileInfo fileInfo = new FileInfo(codeBase.LocalPath);
            Assert.AreEqual(0,CommitAssembly.Program.Main(new string[] {fileInfo.DirectoryName + "\\*.dll"}));
         }

        [Test]
        public void GetExportsDirectFromDatabase()
        {
            
            Console.WriteLine("Looking for IDataProvider:");
            try
            {
                var answer = CatalogueRepository.MEF.GetTypes<IDataProvider>().ToArray();

                //make sure we got at least one IDataProvider
                Assert.GreaterOrEqual(answer.Count(),1);

                foreach (var type in answer)
                    Console.WriteLine(type.ToString());
            }
            catch (ReflectionTypeLoadException e)
            {
                throw new Exception("Problem occurred reflecting types:" + ExceptionHelper.ExceptionToListOfInnerMessages(e));
            }


            Console.WriteLine("Looking for IAttachers:");

            try
            {
                var answer2 = CatalogueRepository.MEF.GetTypes<IAttacher>().ToArray();

                //make sure we got at least one IAttacher
                Assert.GreaterOrEqual(answer2.Count(), 1);

                foreach (var type in answer2)
                    Console.WriteLine(type.ToString());
            }
            catch (ReflectionTypeLoadException e)
            {
                throw new Exception("Problem occurred reflecting types:" + ExceptionHelper.ExceptionToListOfInnerMessages(e));
            }
        }

        [Test]
        public void CreateTestTypesDirectFromDatabase()
        {
            //apparently naming convention is to replace underscores with + in assembly anmes, weird eh?
            string toCreate = "CommitAssemblyEmptyAssembly.MyExampleDataProviderFor_CommitAssemblyTest";

            CatalogueRepository.MEF.AddTypeToCatalogForTesting(typeof(MyExampleDataProviderFor_CommitAssemblyTest));

            IDataProvider answer = CatalogueRepository.MEF.FactoryCreateA<IDataProvider>(toCreate);
            Assert.NotNull(answer);
            Assert.AreEqual(answer.GetType().FullName,toCreate);


            toCreate = "CommitAssemblyEmptyAssembly.MyExampleAttacherFor_CommitAssemblyTest";
            IAttacher answer2 = CatalogueRepository.MEF.FactoryCreateA<IAttacher>(toCreate);
            Assert.NotNull(answer2);
            Assert.AreEqual(answer2.GetType().FullName, toCreate);
        }


        [Test]
        public void TestTemplatedInheritance_IdealCase_ThrowsBecauseFishermanIsntDeclaredExporterAndDoesntImplementBasicInterface()
        {
            //We can create an IBasicFisher
            var bob = CatalogueRepository.MEF.FactoryCreateA<IFishFor<IBasicFish>>(typeof(BasicFisherman).FullName);
            Assert.NotNull(bob);
            Assert.NotNull(bob.CatchFish());

            //We can create an IRenderedFisher
            var frank = CatalogueRepository.MEF.FactoryCreateA<IFishFor<IRenderedFish>>(typeof(RenderedFisherman).FullName);
            Assert.NotNull(frank);
            Assert.NotNull(frank.CatchFish());
            
            //But can we create an IRenderedFisher as a 
            Assert.Throws<KeyNotFoundException>(()=>CatalogueRepository.MEF.FactoryCreateA<IFishFor<IBasicFish>>(typeof(RenderedFisherman).FullName));
        }

        [Test]
        public void TestTemplatedInheritance_TruthTeller_ThrowsBecauseFishermanIsntDeclaredWithIBasicFishExport()
        {
            //But can we create an IRenderedFisher as a 
            var ex = Assert.Throws <KeyNotFoundException>(()=>CatalogueRepository.MEF.FactoryCreateA<IFishFor<IBasicFish>>(typeof(RenderedFishermanWhoIsExplicitlyHonest).FullName));

            Assert.IsTrue(ex.Message.Contains(@"Cannot cast the underlying exported value of type 'CatalogueLibraryTests.Integration.RenderedFishermanWhoLies"));
        }

        [Test]
        public void TestTemplatedInheritance_Liar_CompositionErrorTheExportsDontMatchTheImplementation()
        {
            //But can we create an IRenderedFisher as a 
            Assert.Throws<KeyNotFoundException>(() => CatalogueRepository.MEF.FactoryCreateA<IFishFor<IBasicFish>>(typeof(RenderedFishermanWhoLies).FullName));
        }
        
        [Test]
        public void TestTemplatedInheritance_NastyDuplicateDeclarations()
        {
            //We can create an IBasicFisher
            var bob = CatalogueRepository.MEF.FactoryCreateA<IFishFor<IBasicFish>>(typeof(BasicFisherman).FullName);
            Assert.NotNull(bob);
            Assert.NotNull(bob.CatchFish());

            //We can create an IRenderedFisher
            var frank = CatalogueRepository.MEF.FactoryCreateA<IFishFor<IRenderedFish>>(typeof(RenderedFishermanExplicitDuplicateDeclaration).FullName);
            Assert.NotNull(frank);
            Assert.NotNull(frank.CatchFish());

            //But can we create an IRenderedFisher as a 
            var frank2 = CatalogueRepository.MEF.FactoryCreateA<IFishFor<IBasicFish>>(typeof(RenderedFishermanExplicitDuplicateDeclaration).FullName);
            Assert.NotNull(frank2);
            Assert.NotNull(frank2.CatchFish());
        }


       

        [Test]
        public void GetTypesFromAllLoadModules()
        {
            var answer = CatalogueRepository.MEF.GetAllTypes();

            foreach (var type in answer)
                Console.WriteLine(type.ToString());
        }

    }
    [InheritedExport(typeof(IFishFor<>))]
    public interface IFishFor<T>  where T :IBasicFish
    {
        T CatchFish();
    }
    
    public interface IBasicFish
    {
        int FinsCount { get; set; }
    }

    public interface IRenderedFish : IBasicFish
    {
        int Colour { get; set; }
        int Texture { get; set; }
    }
    public class BasicFisherman : IFishFor<IBasicFish>
    {
        public IBasicFish CatchFish()
        {
            return MockRepository.GenerateMock<IBasicFish>();
        }
    }

    public class RenderedFisherman : IFishFor<IRenderedFish>
    {
        public IRenderedFish CatchFish()
        {
            return MockRepository.GenerateMock<IRenderedFish>();
        }
    }

    [InheritedExport(typeof(IFishFor<IRenderedFish>))]
    public class RenderedFishermanWhoIsExplicitlyHonest : IFishFor<IRenderedFish>
    {
        public IRenderedFish CatchFish()
        {
            return MockRepository.GenerateMock<IRenderedFish>();
        }
    }

    [InheritedExport(typeof(IFishFor<IBasicFish>))]
    [InheritedExport(typeof(IFishFor<IRenderedFish>))]
    public class RenderedFishermanWhoLies : IFishFor<IRenderedFish>
    {
        public IRenderedFish CatchFish()
        {
            return MockRepository.GenerateMock<IRenderedFish>();
        }
    }

    public class RenderedFishermanExplicitDuplicateDeclaration : IFishFor<IRenderedFish>, IFishFor<IBasicFish>
    {
        public IRenderedFish CatchFish()
        {
            return MockRepository.GenerateMock<IRenderedFish>();
        }

        IBasicFish IFishFor<IBasicFish>.CatchFish()
        {
            return CatchFish();
        }
    }
}
