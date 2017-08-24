using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;

namespace CatalogueLibrary.Spontaneous
{
    /// <summary>
    /// SpontaneousObjects are 'memory only' versions of IMapsDirectlyToDatabaseTable classes which throw NotSupportedException on any attempt to persist / delete them etc but which
    /// you can initialize and set properties on towards your own nefarious ends.
    /// 
    /// E.g. lets say during the course of your programming you want to bolt another container and filter onto an AggregateContainer (in your Catalogue) then you can
    /// SpontaneouslyInventedFilterContainer, put the AggregateContainer into it and create a SpontaneouslyInventedFilter along side it.  Then pass the Sponted container
    /// to an ISqlQueryBuilder and watch it treat it just like any other normal collection of (database based) filters / containers.
    /// 
    /// SpontaneousObjects all have NEGATIVE IDs which are randomly generated, this lets the RDMP software use ID for object equality without getting confused but prevents the
    /// system from ever accidentally saving a SpontaneousObject into a data table in the Catalogue
    /// </summary>
    public abstract class SpontaneousObject:IMapsDirectlyToDatabaseTable,ISaveable,IDeleteable,IRevertable
    {
        static Random random = new Random();

        private int _id;

        protected SpontaneousObject()
        {
            _id = random.Next(1000000000)*-1;
        }

        public void DeleteInDatabase()
        {
            throw new System.NotSupportedException("Spontaneously Invented Objects cannot be deleted");
        }

        public void SaveToDatabase()
        {
            throw new System.NotSupportedException("Spontaneously Invented Objects cannot be saved");
        }

        public int ID { get { return _id; }  set{throw new NotSupportedException();}}
        public IRepository Repository { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }
        public event PropertyChangedEventHandler PropertyChanged;
        public void SetReadOnly()
        {
            //already is mate... kinda
        }

        public DbCommand UpdateCommand { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }

        public void RevertToDatabaseState()
        {
            //there will never be any persistence so we revert by doing nothing - it is assumed that any change is intended
        }

        public RevertableObjectReport HasLocalChanges()
        {
            return new RevertableObjectReport {Evaluation = ChangeDescription.NoChanges};
        }

        public bool Exists()
        {
            return true;
        }
    }
}
