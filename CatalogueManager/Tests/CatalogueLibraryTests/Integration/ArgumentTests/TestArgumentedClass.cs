using CatalogueLibrary.Data;

namespace CatalogueLibraryTests.Integration.ArgumentTests
{
    public class TestArgumentedClass
    {
        [DemandsInitialization("Fishes", DemandType.Unspecified,true)]
        public bool MyBool { get; set; }
    }
}