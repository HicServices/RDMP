using System.ComponentModel;
using MapsDirectlyToDatabaseTable.Revertable;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// An IMapsDirectlyToDatabaseTable object who has a property/column called Name which is editable.  Note that you should ensure the ToString property of your
    /// class returns the Name field to in order to not drive users crazy.
    /// </summary>
    public interface INamed : IRevertable, IDeleteable, INotifyPropertyChanged
    {
        string Name { get; set; }
    }
}