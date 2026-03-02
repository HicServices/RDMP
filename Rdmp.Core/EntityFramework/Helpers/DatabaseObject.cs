using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.EntityFramework.Helpers
{
    public  class DatabaseObject: IMapsDirectlyToDatabaseTable
    {
        public virtual int ID { get; set; }

        public virtual RDMPDbContext CatalogueDbContext { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(object oldValue, object newValue,
[CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedExtendedEventArgs(propertyName, oldValue, newValue));
        }


        protected void SetField<T>(ref T field, T value)
        {
            OnPropertyChanged(field, value, null);
            field = value;
        }
    }
}
