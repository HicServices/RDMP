using System.ComponentModel;

namespace MapsDirectlyToDatabaseTable.Revertable
{
    /// <summary>
    /// Enhanced version of c# core class <see cref="PropertyChangedEventArgs"/> which supports recording the old and new values that changed when
    /// the property was updated.
    /// </summary>
    public class PropertyChangedExtendedEventArgs : PropertyChangedEventArgs
    {
        public virtual object OldValue { get; private set; }
        public virtual object NewValue { get; private set; }

        public PropertyChangedExtendedEventArgs(string propertyName, object oldValue, object newValue)
            : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}