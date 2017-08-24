using System;
using System.Linq;
using System.Threading;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using Ticketing;

namespace DataExportLibrary.DataRelease
{
    /// <summary>
    /// Evaluates things that are not within the control area of the DataExportManager but which might prevent a release e.g. ticketing system is not available / tickets in wrong status / safehaven down for maintencence etc
    /// </summary>
    public class ReleaseEnvironmentPotential
    {
        private readonly DataExportRepository _repository;
        public IExtractionConfiguration Configuration { get; private set; }
        public IProject Project { get; private set; }

        public Exception Exception { get; private set; }
        public TicketingReleaseabilityEvaluation Assesment { get; private set; }
        public string Reason { get; private set; }

        
        public ReleaseEnvironmentPotential(IExtractionConfiguration configuration)
        {
            _repository = (DataExportRepository) configuration.Repository;
            Configuration = configuration;
            Project = configuration.Project;
            MakeAssessment();
        }
        
        private void MakeAssessment()
        {
            Assesment = TicketingReleaseabilityEvaluation.TicketingLibraryMissingOrNotConfiguredCorrectly;

            var configuration = _repository.CatalogueRepository.GetAllObjects<TicketingSystemConfiguration>("WHERE IsActive = 1").SingleOrDefault();
            if (configuration == null) return;

            TicketingSystemFactory factory = new TicketingSystemFactory(_repository.CatalogueRepository);
            ITicketingSystem ticketingSystem = factory.CreateIfExists(configuration);

            if (ticketingSystem == null) return;

            try
            {
                Exception e;
                string reason;
                Assesment = ticketingSystem.GetDataReleaseabilityOfTicket(Project.MasterTicket,
                    Configuration.RequestTicket, Configuration.ReleaseTicket, out reason, out e);
                Exception = e;
                Reason = reason;
            }
            catch (Exception e)
            {
                if (e is ThreadInterruptedException)
                    throw;

                Assesment = TicketingReleaseabilityEvaluation.TicketingLibraryCrashed;
                Exception = e;
            }
        }
    }
}
