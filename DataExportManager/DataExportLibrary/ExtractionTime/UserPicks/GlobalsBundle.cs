using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;

namespace DataExportLibrary.ExtractionTime.UserPicks
{
    public class GlobalsBundle:Bundle
    {
        public List<SupportingDocument> Documents { get; private set; }
        public List<SupportingSQLTable> SupportingSQL { get; private set; }

        public GlobalsBundle(SupportingDocument[] documents, SupportingSQLTable[] supportingSQL) :
                base(
                new object[0].Union(documents).Union(supportingSQL).ToArray()
                //pass all the objects to the base class so it can allocate initial States
                )
        {
            Documents = documents.ToList();
            SupportingSQL = supportingSQL.ToList();
        }

        public bool Any()
        {
            return Documents.Any() || SupportingSQL.Any();
        }


        protected override void OnDropContent(object toDrop)
        {
            var item = toDrop as SupportingDocument;
            if (item != null)
            {
                Documents.Remove(item);
                return;
            }

            var drop = toDrop as SupportingSQLTable;
            if (drop != null)
            {
                SupportingSQL.Remove(drop);
                return;
            }


            throw new NotSupportedException("Did not know how to drop object of type "+toDrop.GetType());

        }
    }
}
