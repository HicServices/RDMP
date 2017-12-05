using System;
using CatalogueLibrary.Data.Dashboarding;
using MapsDirectlyToDatabaseTable;

namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence
{
    /// <summary>
    /// Hydrateable class used to represent an attempt to restore the state of a docked window after a user closed RDMP and reopened it
    /// records the Type of the user interface Control which should be shown and the instance of the object (or collection) that should
    /// be shown in it once it has been created.
    /// </summary>
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
