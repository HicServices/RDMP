using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;

namespace CatalogueWebService.Modules.Data
{
    class SupportingDocumentData
    {
        public int ID { get; set; }
        public string Name { get; set; }
      
        public SupportingDocumentData(SupportingDocument supportingDocument)
        {
            ID = supportingDocument.ID;
            Name = supportingDocument.Name;
        }
    }
}
