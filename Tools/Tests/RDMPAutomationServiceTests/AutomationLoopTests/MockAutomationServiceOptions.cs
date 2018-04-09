using CatalogueLibrary.Repositories;
using RDMPAutomationService;

namespace RDMPAutomationServiceTests.AutomationLoopTests
{
    public class MockAutomationServiceOptions : AutomationServiceOptions
    {
        private readonly IRDMPPlatformRepositoryServiceLocator mockRepositoryLocator;
        
        public MockAutomationServiceOptions(IRDMPPlatformRepositoryServiceLocator mockRepositoryLocator)
        {
            this.mockRepositoryLocator = mockRepositoryLocator;
        }

        public override IRDMPPlatformRepositoryServiceLocator GetRepositoryLocator()
        {
            return mockRepositoryLocator;
        }
    }
}