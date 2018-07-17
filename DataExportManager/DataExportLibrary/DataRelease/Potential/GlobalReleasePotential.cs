using System;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.DataRelease.Potential
{
    public abstract class GlobalReleasePotential : ICheckable
    {
        protected readonly IRDMPPlatformRepositoryServiceLocator RepositoryLocator;
        protected readonly ISupplementalExtractionResults GlobalResult;
        
        public IMapsDirectlyToDatabaseTable RelatedGlobal { get; protected set; }
        public Releaseability Releasability { get; protected set; }

        protected GlobalReleasePotential(IRDMPPlatformRepositoryServiceLocator repositoryLocator, ISupplementalExtractionResults globalResult, IMapsDirectlyToDatabaseTable globalToCheck)
        {
            RepositoryLocator = repositoryLocator;
            GlobalResult = globalResult;
            RelatedGlobal = globalToCheck;
        }

        public virtual void Check(ICheckNotifier notifier)
        {
            if (GlobalResult == null) // this should be already covered by the NoGlobalPotential
                return;
            
            if (GlobalResult.SQLExecuted != null)
            {
                var table = RelatedGlobal as SupportingSQLTable;
                if (table == null)
                { 
                    notifier.OnCheckPerformed(new CheckEventArgs("The executed Global " + GlobalResult.ExtractedName + 
                                                                 " has a SQL script but the extracted type (" + GlobalResult.ExtractedType + 
                                                                 ") is not a SupportingSQLTable", CheckResult.Fail));
                    Releasability = Releaseability.ExtractionSQLDesynchronisation;
                }
                if (table.SQL != GlobalResult.SQLExecuted)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("The executed Global " + GlobalResult.ExtractedName +
                                                                 " has a SQL script which is different from the current one!", CheckResult.Fail));
                    Releasability = Releaseability.ExtractionSQLDesynchronisation;
                }
            }

            if (GlobalResult.GetExtractedType() == typeof (SupportingDocument))
                CheckFileExists(notifier, GlobalResult.DestinationDescription);
            else
                CheckDestination(notifier, GlobalResult);

            if (Releasability == Releaseability.Undefined)
                Releasability = Releaseability.Releaseable;
        }

        protected void CheckFileExists(ICheckNotifier notifier, string filePath)
        {
            var file = new FileInfo(filePath);
            if (!file.Exists)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("File: " + file.FullName + " was not found", CheckResult.Fail));
                Releasability = Releaseability.ExtractFilesMissing;
            }
        }

        protected abstract void CheckDestination(ICheckNotifier notifier, ISupplementalExtractionResults globalResult);
    }
}