using System.ComponentModel;
using MapsDirectlyToDatabaseTable.Revertable;

namespace MapsDirectlyToDatabaseTable
{
    public interface INamed : IRevertable, IDeleteable, INotifyPropertyChanged
    {
        string Name { get; set; }
    }
}