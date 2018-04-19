using CatalogueLibrary.Data;
using HIC.Logging;

namespace RDMPAutomationService.EventHandlers
{
    public delegate void LoggingStartedEventHandler(ExternalDatabaseServer loggingServer, DataLoadInfo loadStarted);
}