using System;
using CatalogueLibrary.Data.Dashboarding;
using MapsDirectlyToDatabaseTable;

namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence
{
    public class DeserializeInstruction
    {
        public Type UIControlType;
        public IMapsDirectlyToDatabaseTable DatabaseObject;
        public IPersistableObjectCollection ObjectCollection;

        public DeserializeInstruction(Type uiControlType, IMapsDirectlyToDatabaseTable databaseObject)
        {
            UIControlType = uiControlType;
            DatabaseObject = databaseObject;
        }

        public DeserializeInstruction(Type uiControlType, IPersistableObjectCollection objectCollection)
        {
            UIControlType = uiControlType;
            ObjectCollection = objectCollection;
        }
    }
}
