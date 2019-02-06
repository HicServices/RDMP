// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Data.Common;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;

namespace CatalogueLibrary.Spontaneous
{
    /// <summary>
    /// SpontaneousObjects are 'memory only' versions of IMapsDirectlyToDatabaseTable classes which throw NotSupportedException on any attempt to persist / delete them etc but which
    /// you can initialize and set properties on towards your own nefarious ends.
    /// 
    /// <para>E.g. lets say during the course of your programming you want to bolt another container and filter onto an AggregateContainer (in your Catalogue) then you can
    /// SpontaneouslyInventedFilterContainer, put the AggregateContainer into it and create a SpontaneouslyInventedFilter along side it.  Then pass the Sponted container
    /// to an ISqlQueryBuilder and watch it treat it just like any other normal collection of (database based) filters / containers.</para>
    /// 
    /// <para>SpontaneousObjects all have NEGATIVE IDs which are randomly generated, this lets the RDMP software use ID for object equality without getting confused but prevents the
    /// system from ever accidentally saving a SpontaneousObject into a data table in the Catalogue</para>
    /// </summary>
    public abstract class SpontaneousObject: IRevertable
    {
        static Random random = new Random();

        private int _id;

        protected SpontaneousObject()
        {
            _id = random.Next(1000000000)*-1;
        }

        public virtual void DeleteInDatabase()
        {
            throw new System.NotSupportedException("Spontaneously Invented Objects cannot be deleted");
        }

        public virtual void SaveToDatabase()
        {
            throw new System.NotSupportedException("Spontaneously Invented Objects cannot be saved");
        }
        
        /// <summary>
        /// Returns a random negative number (set during construction) to satisfy interface.  SpontaneousObjects do not get stored in 
        /// the database so the ID is not very meaningful.
        /// </summary>
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
