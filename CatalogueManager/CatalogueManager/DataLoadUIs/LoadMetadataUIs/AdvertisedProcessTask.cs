using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs
{
    public class AdvertisedProcessTask
    {
        public ProcessTaskType ProcessTaskType { get; set; }
        public Type AdvertisedType { get; set; }
        public string Description { get; set; }
        
        public string ComponentCategory
        {
            get
            {
                switch (ProcessTaskType)
                {
                    case ProcessTaskType.Attacher:
                        return "Attachers";
                    case ProcessTaskType.DataProvider:
                        return "Data Providers";
                    case ProcessTaskType.MutilateDataTable:
                        return "Data Table Mutilators";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        public AdvertisedProcessTask(ProcessTaskType processTaskType, Type advertisedType)
        {
            AdvertisedType = advertisedType;
            ProcessTaskType = processTaskType;
           
            if(processTaskType != ProcessTaskType.Attacher && processTaskType != ProcessTaskType.DataProvider && processTaskType != ProcessTaskType.MutilateDataTable)
                throw new ArgumentException("Process Task Type must be attacher, provider or mutilator","processTaskType");

            Description = GetDescription(advertisedType);
        }

        private string GetDescription(Type advertisedType)
        {
            var descriptions = (DescriptionAttribute[])advertisedType.GetCustomAttributes(typeof(DescriptionAttribute), true);
            
            if (descriptions.Length == 0)
                return null;

            return GetDescriptionForTypeIncludingBaseTypes(advertisedType, true);
        }

        /// <summary>
        /// Parses entire class hierarchy looking for [Description("something")] elements which are all agregated together (recursively) and returned
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetDescriptionForTypeIncludingBaseTypes(Type type, bool isRootClass)
        {
            var descriptions = (DescriptionAttribute[]) type.GetCustomAttributes(typeof (DescriptionAttribute), false);
            
            string message = "";

            if (descriptions.Length == 0)
                message = "No description found for Type:" + type.FullName;
            else
            {
                message = descriptions.Single().Description.TrimEnd();

                if (isRootClass)
                    message = "CLASS:" + type.Name + Environment.NewLine + "Description:" + Environment.NewLine + message;
                else
                    message = "PARENT CLASS:" + type.Name + Environment.NewLine + "Description:" + Environment.NewLine + message;
            }

             if (type.BaseType != null)
                 return message + Environment.NewLine + Environment.NewLine + GetDescriptionForTypeIncludingBaseTypes(type.BaseType, false);
            
            return message;
        }
        
        public override string ToString()
        {
            return AdvertisedType.Name;
        }
    }
}
