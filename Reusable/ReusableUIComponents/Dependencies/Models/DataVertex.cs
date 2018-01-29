using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using GraphX.PCL.Common.Models;
using ReusableLibraryCode;
using ReusableLibraryCode.Annotations;

namespace ReusableUIComponents.Dependencies.Models
{
    /* DataVertex is the data class for the vertices. It contains all custom vertex data specified by the user.
     * This class also must be derived from VertexBase that provides properties and methods mandatory for
     * correct GraphX operations.
     * Some of the useful VertexBase members are:
     *  - ID property that stores unique positive identfication number. Property must be filled by user.
     *  
     */

    public class DataVertex : VertexBase, INotifyPropertyChanged
    {
        private readonly object _coreObjectHeldInVertex;
        private readonly IObjectVisualisation _visualiser;

        /// <summary>
        /// Some string property for example purposes
        /// </summary>
       
        public Bitmap Image { get; set; }
        public string GradStartColor { get; set; }
        public string GradEndColor { get; set; }

        public OrderedDictionary EntityInformation { get; set; }
        public string[] NameAndType { get; set; }
        private readonly Type _coreObjectHeldInVertexType;
        public string[] Keys { get; set; }
        public string[] Values { get; set; }

        
        public object CoreObjectHeldInVertex 
        {
            get { return _coreObjectHeldInVertex; }
        }


        public Type CoreObjectType
        {
            get { return _coreObjectHeldInVertexType; }
        }

        /// <summary>
        /// Default parameterless constructor for this class
        /// (required for YAXLib serialization)
        /// </summary>
        public DataVertex():this(null,null,false)
        {
        }

       
        public DataVertex(IHasDependencies coreObjectHeldInVertex, IObjectVisualisation visualiser, bool isHighlighted)
        {
            //see HIC.DataManagementPlatform\Reusable\ReusableUIComponents\Dependencies\Templates (for data bindings to these arrays - 3 elements btw not 5 atm)
            Keys = new string[5];
            Values = new string[5];
            NameAndType = new string[3];
            _coreObjectHeldInVertex = coreObjectHeldInVertex;
            _visualiser = visualiser;
            _coreObjectHeldInVertexType = coreObjectHeldInVertex.GetType();

            RefreshState(isHighlighted);
        }

        public void RefreshState(bool isHighlighted)
        {
            //ask the third party what we should be displaying for the object model 
            EntityInformation = _visualiser.EntityInformation(_coreObjectHeldInVertex);

            //they told us a bunch of key value pairs that describe the object but they might have cheekily sent us some non string types
            //so make everything into a string
            var keys = EntityInformation.Keys.Cast<object>().Select(k=>k.ToString()).ToArray();
            var vals = EntityInformation.Values.Cast<object>().Select(v => v == null? "<Null>" :v.ToString()).ToArray();
            
            //and copy the results into Keys and Values which are used by the xaml bindings to render the object in the UI
            keys.CopyTo(Keys,0);
            vals.CopyTo(Values,0);

            NameAndType = _visualiser.GetNameAndType(_coreObjectHeldInVertex);
            Image = _visualiser.GetImage(_coreObjectHeldInVertex);
            var colorsToUse = _visualiser.GetColor(_coreObjectHeldInVertex, new ColorRequest(isHighlighted));

            //update the colours
            var before = GradStartColor;
            GradStartColor = colorsToUse.GradientStartColor.ToString();
            if (GradStartColor != before)
                OnPropertyChanged("GradStartColor");//if the colours actually changed then fire the changed event binding

            before = GradEndColor;
            GradEndColor = colorsToUse.GradientEndColor.ToString();
            if(before != GradEndColor)
                OnPropertyChanged("GradEndColor");//if the colours actually changed then fire the changed event binding
            
        }


        #region Calculated or static props

        public override string ToString()
        {
            return _coreObjectHeldInVertex.ToString();
        }


        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
